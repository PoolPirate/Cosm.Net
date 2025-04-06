using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Tx;
using Grpc.Core;
using Google.Protobuf;

using System.Threading.Channels;
using QueueEntry = (Cosm.Net.Tx.ICosmTx Tx, ulong GasWanted, System.Collections.Generic.IEnumerable<Cosm.Net.Models.Coin> TxFees,
    System.DateTime? Deadline, System.Threading.CancellationToken CancellationToken,
    System.Threading.Tasks.TaskCompletionSource<string> CompletionSource);
using Cosm.Net.Adapters.Internal;
using Cosm.Net.Wallet;

namespace Cosm.Net.Services;

public class RobustTxScheduler : ITxScheduler
{
    private readonly IInternalAuthAdapter _authAdapter;
    private readonly IChainConfiguration _chainConfiguration;
    private readonly IGasFeeProvider _gasFeeProvider;
    private readonly Channel<QueueEntry> _pendingTxChannel;
    private readonly ICosmSigner _signer;
    private readonly ByteString _signerPubkey;

    private readonly ITxEncoder _txEncoder;
    private readonly IInternalTxAdapter _txModuleAdapater;
    private readonly ITxPublisher _txPublisher;

    public RobustTxScheduler(ITxEncoder txEncoder, ICosmSigner signer, IInternalAuthAdapter authAdapter,
        IChainConfiguration chainConfiguration, IInternalTxAdapter txModuleAdapater, IGasFeeProvider gasFeeProvider,
        ITxPublisher txPublisher)
    {
        _pendingTxChannel = Channel.CreateUnbounded<QueueEntry>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        _txEncoder = txEncoder;
        _signer = signer;
        _authAdapter = authAdapter;
        _chainConfiguration = chainConfiguration;
        _txModuleAdapater = txModuleAdapater;
        _gasFeeProvider = gasFeeProvider;
        _txPublisher = txPublisher;

        _signerPubkey = ByteString.CopyFrom(_signer.PublicKey);

        _ = Task.Run(BackgroundTxProcessor);
    }

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }


    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var accountData = await _authAdapter.GetAccountAsync(
            _signer.GetAddress(_chainConfiguration.Bech32Prefix),
            cancellationToken: cancellationToken);

        AccountNumber = accountData.AccountNumber;
        CurrentSequence = accountData.Sequence;
    }

    public async Task<TxSimulation> SimulateTxAsync(ICosmTx tx, CancellationToken cancellationToken)
    {
        ulong sequence = CurrentSequence;
        int attempts = 3;
        for(int i = 0; i < attempts; i++)
        {
            try
            {
                var encodedTx = _txEncoder.EncodeTx(tx, _signerPubkey, sequence, _gasFeeProvider.GasFeeDenom);
                return await _txModuleAdapater.SimulateAsync(encodedTx, cancellationToken: cancellationToken);
            }
            catch(RpcException ex)
                when(ex.StatusCode == StatusCode.Unknown
                     && ex.Status.Detail.StartsWith("account sequence"))
            {
                if(i == attempts - 1)
                {
                    throw;
                }

                sequence = UInt64.Parse(ex.Status.Detail.Split("expected ")[1].Split(',')[0]);
            }
        }

        throw new Exception("Impossible");
    }

    public async Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, IEnumerable<Coin> txFees, DateTime? deadline, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource<string>();
        await _pendingTxChannel.Writer.WriteAsync(new QueueEntry(tx, gasWanted, txFees, deadline, cancellationToken, source), cancellationToken);
        return await source.Task;
    }

    private async Task BackgroundTxProcessor()
    {
        while(true)
        {
            try
            {
                var entry = await _pendingTxChannel.Reader.ReadAsync();
                await ProcessTxAsync(entry);
            }
            catch(ChannelClosedException)
            {
                return;
            }
            catch(Exception)
            {
                //
            }
        }
    }

    private bool TryCreateAndSignTx(QueueEntry entry, out SignedTx signedTx)
    {
        var signDoc = _txEncoder.GetSignSignDoc(
            entry.Tx,
            _signerPubkey,
            entry.GasWanted,
            entry.TxFees,
            AccountNumber,
            CurrentSequence
        );

        Span<byte> signatureBuffer = stackalloc byte[64];

        int signDocSize = signDoc.CalculateSize();
        Span<byte> signDocBuffer = signDocSize < 8192
            ? stackalloc byte[signDocSize]
            : new byte[signDocSize];

        signDoc.WriteTo(signDocBuffer);

        if(!_signer.SignMessage(signDocBuffer, signatureBuffer))
        {
            signedTx = null!;
            return false;
        }

        signedTx = new SignedTx(entry.Tx, entry.GasWanted, entry.TxFees, CurrentSequence, _signer.PublicKey, signatureBuffer);
        return true;
    }

    private async Task ProcessTxAsync(QueueEntry entry)
    {
        ulong generatedWithSequence = ulong.MaxValue;
        SignedTx signedTx = null!;

        Span<byte> signature = stackalloc byte[64];

        for(int i = 0; i < 3; i++)
        {
            if(generatedWithSequence != CurrentSequence)
            {
                if(!TryCreateAndSignTx(entry, out signedTx))
                {
                    entry.CompletionSource.SetException(new NotSupportedException());
                    return;
                }

                generatedWithSequence = CurrentSequence;
            }

            await Task.Delay(1000 * i);
            if(entry.CancellationToken.IsCancellationRequested)
            {
                entry.CompletionSource.SetCanceled(entry.CancellationToken);
                return;
            }

            try
            {
                var txSubmission = await _txPublisher.PublishTxAsync(signedTx, entry.Deadline, entry.CancellationToken);

                switch(txSubmission.Code)
                {
                    case 0:
                        CurrentSequence++;
                        entry.CompletionSource.SetResult(txSubmission.TxHash);
                        return;
                    case 32:
                        if(i + 1 == 3)
                        {
                            entry.CompletionSource.SetException(
                                new TxPublishException(txSubmission.Code, txSubmission.RawLog));
                            return;
                        }

                        var expected = ulong.Parse(txSubmission.RawLog
                            .Split("expected ")[1].Split(',')[0]);

                        CurrentSequence = i == 0
                            ? Math.Max(CurrentSequence, expected)
                            : expected;

                        await Task.Delay(5000 * i);
                        continue;
                    default:
                        entry.CompletionSource.SetException(
                            new TxPublishException(txSubmission.Code, txSubmission.RawLog));
                        return;
                }
            }
            catch(TaskCanceledException)
            {
                entry.CompletionSource.SetCanceled(entry.CancellationToken);
                return;
            }
            catch(OperationCanceledException)
            {
                entry.CompletionSource.SetCanceled(entry.CancellationToken);
                return;
            }
            catch(Exception ex)
            {
                if(i + 1 == 3)
                {
                    entry.CompletionSource.SetException(ex);
                    return;
                }
            }
        }
    }
}
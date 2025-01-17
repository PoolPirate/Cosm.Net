using Cosm.Net.Adapters;
using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using System.Threading.Channels;
using Google.Protobuf;

using QueueEntry = (Cosm.Net.Tx.ICosmTx Tx, ulong GasWanted, System.Collections.Generic.IEnumerable<Cosm.Net.Models.Coin> TxFees,
    System.DateTime? Deadline, System.Threading.CancellationToken CancellationToken,
    System.Threading.Tasks.TaskCompletionSource<string> CompletionSource);

namespace Cosm.Net.Services;
public class SequentialTxScheduler : ITxScheduler
{
    private readonly Channel<QueueEntry> _txChannel;
    private readonly ITxEncoder _txEncoder;
    private readonly ICosmSigner _signer;
    private readonly IAuthModuleAdapter _authAdapter;
    private readonly IChainConfiguration _chainConfiguration;
    private readonly ITxModuleAdapter _txModuleAdapater;
    private readonly IGasFeeProvider _gasFeeProvider;
    private readonly ITxPublisher _txPublisher;

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }

    public SequentialTxScheduler(ITxEncoder txEncoder, ICosmSigner signer, IAuthModuleAdapter authAdapter,
        IChainConfiguration chainConfiguration, ITxModuleAdapter txModuleAdapater, IGasFeeProvider gasFeeProvider, ITxPublisher txPublisher)
    {
        _txChannel = Channel.CreateUnbounded<QueueEntry>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        _txEncoder = txEncoder;
        _signer = signer;
        _authAdapter = authAdapter;
        _chainConfiguration = chainConfiguration;
        _txModuleAdapater = txModuleAdapater;
        _gasFeeProvider = gasFeeProvider;
        _txPublisher = txPublisher;

        _ = Task.Run(BackgroundTxProcessor);
    }

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
        var encodedTx = _txEncoder.EncodeTx(tx, ByteString.CopyFrom(_signer.PublicKey), CurrentSequence, _gasFeeProvider.GasFeeDenom);
        return await _txModuleAdapater.SimulateAsync(encodedTx, cancellationToken: cancellationToken);
    }

    public async Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, IEnumerable<Coin> txFees, DateTime? deadline, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource<string>();
        await _txChannel.Writer.WriteAsync(new QueueEntry(tx, gasWanted, txFees, deadline, cancellationToken, source));
        return await source.Task;
    }

    private async Task BackgroundTxProcessor()
    {
        while(true)
        {
            try
            {
                var entry = await _txChannel.Reader.ReadAsync();
                await ProcessTxAsync(entry);
            }
            catch(ChannelClosedException)
            {
                return;
            }
        }
    }

    private async Task ProcessTxAsync(QueueEntry entry)
    {
        if(entry.CancellationToken.IsCancellationRequested)
        {
            entry.CompletionSource.SetCanceled(entry.CancellationToken);
            return;
        } 

        byte[] signDoc = _txEncoder.GetSignSignDoc(entry.Tx, ByteString.CopyFrom(_signer.PublicKey), entry.GasWanted, entry.TxFees, AccountNumber, CurrentSequence);
        byte[] signature = _signer.SignMessage(signDoc);

        var signedTx = new SignedTx(entry.Tx, entry.GasWanted, entry.TxFees, CurrentSequence, _signer.PublicKey, signature);

        try
        {
            var txSubmission = await _txPublisher.PublishTxAsync(signedTx, entry.Deadline, entry.CancellationToken);

            if (txSubmission.Code != 0)
            {
                throw new TxPublishException(txSubmission.Code, txSubmission.RawLog);
            }

            CurrentSequence++;
            entry.CompletionSource.SetResult(txSubmission.TxHash);
        }
        catch(TxPublishException ex)
        {
            entry.CompletionSource.SetException(ex);
            return;
        }
        catch
        {
            // Sequential processor, keep trying...
            // ToDo: Refresh Sequence
            await Task.Delay(1000);
            await ProcessTxAsync(entry);
        }
    }
}

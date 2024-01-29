﻿using Cosm.Net.Adapters;
using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using System.Threading.Channels;

using QueueEntry = (Cosm.Net.Tx.ICosmTx Tx, Cosm.Net.Models.GasFeeAmount GasFee,
    System.Threading.Tasks.TaskCompletionSource<string>? CompletionSource);

namespace Cosm.Net.Services;
public class SequentialTxScheduler : ITxScheduler
{
    private readonly Channel<QueueEntry> _txChannel;
    private readonly ITxEncoder _txEncoder;
    private readonly IOfflineSigner _signer;
    private readonly IAuthModuleAdapter _authAdapter;
    private readonly ITxPublisher _txPublisher;
    private readonly IChainConfiguration _chainConfiguration;

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }

    public SequentialTxScheduler(ITxEncoder txEncoder, IOfflineSigner signer, IAuthModuleAdapter authAdapter,
        ITxPublisher txPublisher, IChainConfiguration chainConfiguration)
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
        _txPublisher = txPublisher;
        _chainConfiguration = chainConfiguration;

        _ = Task.Run(BackgroundTxProcessor);
    }

    public async Task InitializeAsync()
    {
        var accountData = await _authAdapter.GetAccountAsync(
            _signer.GetAddress(_chainConfiguration.Bech32Prefix));

        AccountNumber = accountData.AccountNumber;
        CurrentSequence = accountData.Sequence;
    }

    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx)
        => _txPublisher.SimulateTxAsync(tx, CurrentSequence);

    public async Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee)
    {
        var source = new TaskCompletionSource<string>();
        await _txChannel.Writer.WriteAsync(new QueueEntry(tx, gasFee, source));
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
        byte[] signDoc = _txEncoder.GetSignSignDoc(entry.Tx, entry.GasFee, AccountNumber, CurrentSequence);
        byte[] signature = _signer.SignMessage(signDoc);

        var signedTx = new SignedTx(entry.Tx, entry.GasFee, CurrentSequence, signature);

        try
        {
            string txHash = await _txPublisher.PublishTxAsync(signedTx);
            CurrentSequence++;
            entry.CompletionSource?.SetResult(txHash);
        }
        catch(TxPublishException ex)
        {
            entry.CompletionSource?.SetException(ex);
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

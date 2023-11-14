using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using System.Threading.Channels;

using QueueEntry = (ulong GasWanted, string FeeDenom, ulong FeeAmount, Cosm.Net.Tx.ICosmTx Tx,
    System.Threading.Tasks.TaskCompletionSource<string>? CompletionSource);

namespace Cosm.Net.Tx;
public class SequentialTxScheduler : ITxScheduler
{
    private readonly Channel<QueueEntry> _txChannel;
    private readonly ITxEncoder _txEncoder;
    private readonly IOfflineSigner _signer;
    private readonly ITxPublisher _txPublisher;
    private readonly IChainDataProvider _accountDataProvider;
    private readonly ITxChainConfiguration _chainConfiguration;

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }

    public SequentialTxScheduler(ITxEncoder txEncoder, IOfflineSigner signer,
        ITxPublisher txPublisher, IChainDataProvider accountDataProvider, ITxChainConfiguration chainConfiguration)
    {
        _txChannel = Channel.CreateUnbounded<QueueEntry>(new UnboundedChannelOptions()
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        _txEncoder = txEncoder;
        _signer = signer;
        _txPublisher = txPublisher;
        _accountDataProvider = accountDataProvider;
        _chainConfiguration = chainConfiguration;

        _ = Task.Run(BackgroundTxProcessor);
    }

    public async Task InitializeAsync()
    {
        var accountData = await _accountDataProvider.GetAccountDataAsync(
            _signer.GetAddress(_chainConfiguration.Prefix));

        AccountNumber = accountData.AccountNumber;
        CurrentSequence = accountData.Sequence;
    }

    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx)
        => _txPublisher.SimulateTxAsync(tx, CurrentSequence);

    public async Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount)
    {
        var source = new TaskCompletionSource<string>();
        await _txChannel.Writer.WriteAsync(new QueueEntry(gasWanted, feeDenom, feeAmount, tx, source));
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
        var signDoc = _txEncoder.GetSignSignDoc(entry.Tx, AccountNumber, CurrentSequence, 
            entry.GasWanted, entry.FeeDenom, entry.FeeAmount);
        var signature = _signer.SignMessage(signDoc);

        var signedTx = new SignedTx(entry.Tx, CurrentSequence, signature,
            entry.GasWanted, entry.FeeDenom, entry.FeeAmount);

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

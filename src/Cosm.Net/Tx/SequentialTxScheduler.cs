using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Google.Protobuf;
using System.Threading.Channels;
using QueueEntry = (ulong GasWanted, string FeeDenom, ulong FeeAmount, Cosm.Net.Tx.ICosmTx Tx);

namespace Cosm.Net.Tx;
public class SequentialTxScheduler : ITxScheduler
{
    private readonly Channel<QueueEntry> _txChannel;
    private readonly ITxEncoder _txEncoder;
    private readonly IOfflineSigner _signer;
    private readonly ITxPublisher _txPublisher;
    private readonly IChainDataProvider _accountDataProvider;

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }

    public SequentialTxScheduler(ITxEncoder txEncoder, IOfflineSigner signer, 
        ITxPublisher txPublisher, IChainDataProvider accountDataProvider)
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
        _ = Task.Run(BackgroundTxProcessor);
        _accountDataProvider = accountDataProvider;
    }

    public async Task InitializeAsync()
    {
        var accountData = await _accountDataProvider.GetAccountDataAsync("");

        AccountNumber = accountData.AccountNumber;
        CurrentSequence = accountData.Sequence;
    }

    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx)
        => _txPublisher.SimulateTxAsync(tx, CurrentSequence);

    public ValueTask QueueTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount) 
        => _txChannel.Writer.WriteAsync(new QueueEntry(gasWanted, feeDenom, feeAmount, tx));

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
            catch (Exception ex)
            {

            }
        }
    }

    private async Task ProcessTxAsync(QueueEntry entry)
    {
        var signDoc = _txEncoder.GetSignSignDoc(entry.Tx, AccountNumber, CurrentSequence, 
            entry.GasWanted, entry.FeeDenom, entry.FeeAmount);
        var signature = _signer.SignMessage(signDoc);

        var signedTx = new SignedTx(entry.Tx, CurrentSequence, ByteString.CopyFrom(signature),
            entry.GasWanted, entry.FeeDenom, entry.FeeAmount);

        try
        {
            await _txPublisher.PublishTxAsync(signedTx);
            CurrentSequence++;
        }
        catch
        {
            await Task.Delay(1000);
            await ProcessTxAsync(entry);
        }
    }
}

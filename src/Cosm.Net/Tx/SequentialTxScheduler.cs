using Cosm.Net.Services;
using Cosm.Net.Signer;
using Google.Protobuf;
using System.Threading.Channels;

namespace Cosm.Net.Tx;
public class SequentialTxScheduler : ITxScheduler
{
    private readonly Channel<ICosmTx> _txChannel;
    private readonly ITxEncoder _txEncoder;
    private readonly IOfflineSigner _signer;
    private readonly ITxPublisher _txPublisher;
    private readonly IAccountDataProvider _accountDataProvider;

    public ulong AccountNumber { get; private set; }
    public ulong CurrentSequence { get; private set; }

    public SequentialTxScheduler(ITxEncoder txEncoder, IOfflineSigner signer, 
        ITxPublisher txPublisher, IAccountDataProvider accountDataProvider)
    {
        _txChannel = Channel.CreateUnbounded<ICosmTx>(new UnboundedChannelOptions()
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
        var accountData = await _accountDataProvider.GetAccountDataAsync();

        AccountNumber = accountData.AccountNumber;
        CurrentSequence = accountData.Sequence;
    }

    public Task SimulateTxAsync(ICosmTx tx)
        => _txPublisher.SimulateTxAsync(tx, CurrentSequence);

    public ValueTask QueueTxAsync(ICosmTx tx) 
        => _txChannel.Writer.WriteAsync(tx);

    private async Task BackgroundTxProcessor()
    {
        while(true)
        {
            try
            {
                var tx = await _txChannel.Reader.ReadAsync();
                await ProcessTxAsync(tx);
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

    private async Task ProcessTxAsync(ICosmTx tx)
    {
        var signDoc = _txEncoder.GetSignSignDoc(tx, AccountNumber, CurrentSequence);
        var signature = _signer.SignMessage(signDoc);

        var signedTx = new SignedTx(tx, CurrentSequence, ByteString.CopyFrom(signature));

        try
        {
            await _txPublisher.PublishTxAsync(signedTx);
            CurrentSequence++;
        }
        catch
        {
            await Task.Delay(1000);
            await ProcessTxAsync(tx);
        }
    }
}

namespace Cosm.Net.Core.Tx;
public interface ITxPublisher
{
    public Task<TResponse> PublishTxAsync<TResponse>(ICosmTx tx);
}

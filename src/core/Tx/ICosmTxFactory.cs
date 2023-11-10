namespace Cosm.Net.Core.Tx;
public interface ICosmTxFactory
{
    public ICosmTxBuilder CreateCosmTx();
}

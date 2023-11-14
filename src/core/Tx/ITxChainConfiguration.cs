namespace Cosm.Net.Tx;

public interface ITxChainConfiguration
{
    string ChainId { get; }
    string Prefix { get; }
    public string FeeDenom { get; }
    public decimal GasPrice { get; }
}
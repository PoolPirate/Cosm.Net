namespace Cosm.Net.Tx;
public class TxChainConfiguration : ITxChainConfiguration
{
    public string ChainId { get; }
    public string Prefix { get; }
    public string FeeDenom { get; }
    public decimal GasPrice { get; }

    public TxChainConfiguration(string? chainId, string? prefix, string? feeDenom, decimal gasPrice)
    {
        if (chainId is null)
        {
            throw new ArgumentException("ChainId not set");
        }
        if(prefix is null)
        {
            throw new ArgumentException("Prefix not set");
        }
        if(feeDenom is null)
        {
            throw new ArgumentException("GasDenom not set");
        }

        Prefix = prefix;
        FeeDenom = feeDenom;
        GasPrice = gasPrice;
        ChainId = chainId;
    }
}

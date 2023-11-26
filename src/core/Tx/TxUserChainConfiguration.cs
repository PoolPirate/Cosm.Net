namespace Cosm.Net.Tx;
public class TxUserChainConfiguration
{
    public string? Bech32Prefix { get; set; }
    public string? FeeDenom { get; set; }
    public decimal GasPrice { get; set; }

    public TxUserChainConfiguration()
    {
    }
}

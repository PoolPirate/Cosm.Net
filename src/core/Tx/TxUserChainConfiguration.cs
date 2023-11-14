namespace Cosm.Net.Tx;
public class TxUserChainConfiguration
{
    public string? Prefix { get; set; }
    public string? FeeDenom { get; set; }
    public decimal GasPrice { get; set; }

    public TxUserChainConfiguration()
    {
    }
}

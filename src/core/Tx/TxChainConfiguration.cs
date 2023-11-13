namespace Cosm.Net.Tx;
public class TxChainConfiguration : ITxChainConfiguration
{
    public string Prefix { get; set; } = null!;
    public string ChainId { get; set; } = null!;

    public string GasDenom { get; set; } = null!;
    public decimal GasPrice { get; set; } = 0;

    public void Validate()
    {
        if (Prefix is null)
        {
            throw new ArgumentException("Prefix not set");
        }
        if(ChainId is null)
        {
            throw new ArgumentException("Chainid not set");
        }
        if (GasDenom is null)
        {
            throw new ArgumentException("GasDenom not set");
        }
    }
}

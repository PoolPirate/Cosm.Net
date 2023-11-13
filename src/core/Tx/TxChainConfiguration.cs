namespace Cosm.Net.Tx;
public class TxChainConfiguration : ITxChainConfiguration
{
    public string Prefix { get; set; } = null!;
    public string ChainId { get; set; } = null!;

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
    }
}

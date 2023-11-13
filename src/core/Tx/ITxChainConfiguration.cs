namespace Cosm.Net.Tx;

public interface ITxChainConfiguration
{
    string ChainId { get; }
    string Prefix { get; }
    public string GasDenom { get; }
    public decimal GasPrice { get; }

    public void Validate();
}
namespace Cosm.Net.Tx;

public interface ITxChainConfiguration
{
    string ChainId { get; }
    string Prefix { get; }

    public void Validate();
}
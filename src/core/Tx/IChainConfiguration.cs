namespace Cosm.Net.Tx;

public interface IChainConfiguration
{
    string ChainId { get; }
    string Bech32Prefix { get; }
}
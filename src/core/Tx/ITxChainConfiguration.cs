namespace Cosm.Net.Tx;

public interface ITxChainConfiguration
{
    string ChainId { get; }
    string Bech32Prefix { get; }

    internal void Initialize(string chainId, string bech32Prefix);
}
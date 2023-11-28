namespace Cosm.Net.Tx;
public class TxChainConfiguration : ITxChainConfiguration
{
    private string? _chainId = null;
    private string? _bech32Prefix = null;

    public string ChainId => _chainId
        ?? throw new InvalidOperationException("TxChainConfiguration cannot be accessed before Initalization has been called");
    public string Bech32Prefix => _bech32Prefix
        ?? throw new InvalidOperationException("TxChainConfiguration cannot be accessed before Initalization has been called");

    void ITxChainConfiguration.Initialize(string chainId, string bech32Prefix)
    {
        _chainId = chainId;
        _bech32Prefix = bech32Prefix;
    }
}

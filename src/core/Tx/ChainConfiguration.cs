namespace Cosm.Net.Tx;
internal class ChainConfiguration : IChainConfiguration
{
    private bool _isInitialized = false;
    private string? _chainId = null;

    public string Bech32Prefix { get; private set; }
    public string ChainId => _chainId
        ?? throw new InvalidOperationException("TxChainConfiguration cannot be accessed before Initalization has been called");
    public TimeSpan TransactionTimeout { get; private set; }

    public ChainConfiguration(string bech32Prefix, TimeSpan transactionTimeout)
    {
        Bech32Prefix = bech32Prefix;
        TransactionTimeout = transactionTimeout;
    }

    public void Initialize(string chainId)
    {
        if(_isInitialized)
        {
            throw new InvalidOperationException($"{nameof(ChainConfiguration)} already initialized");
        }

        _chainId = chainId;
        _isInitialized = true;
    }
}

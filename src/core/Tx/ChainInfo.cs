namespace Cosm.Net.Tx;
internal class ChainInfo
{
    public string Bech32Prefix { get; }
    public TimeSpan TransactionTimeout { get; set; }

    public ChainInfo(string bech32Prefix, TimeSpan transactionTimeout)
    {
        Bech32Prefix = bech32Prefix;
        TransactionTimeout = transactionTimeout;
    }
}

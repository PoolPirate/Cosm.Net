namespace Cosm.Net.Tx;

public interface IChainConfiguration
{
    /// <summary>
    /// ChainId of the connected chain.
    /// </summary>
    string ChainId { get; }

    /// <summary>
    /// Bech32 address prefix of the connected chain.
    /// </summary>
    string Bech32Prefix { get; }

    /// <summary>
    /// Time after which to stop waiting for a transaction to be confirmed.
    /// </summary>
    TimeSpan TransactionTimeout { get; }
}
namespace Cosm.Net.Exceptions;

public class TxRevertedException : Exception
{
    public TxRevertedException(string chainId, string txHash)
        : base($"Transaction {txHash} on {chainId} reverted")
    {
    }
}
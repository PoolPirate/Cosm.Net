namespace Cosm.Net.Exceptions;

public class TxRevertedException(string chainId, string txHash, string rawLog)
    : Exception($"{txHash} on {chainId} reverted; RawLog={rawLog}")
{
}
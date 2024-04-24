namespace Cosm.Net.Models;
public class TxExecution
{
    public bool Success { get; }
    public string TxHash { get; }
    public long BlockNumber { get; }
    public string Memo { get; }
    public TxEvent[] Events { get; }

    public TxExecution(bool success, string txHash, long blockNumber, string memo, TxEvent[] events)
    {
        Success = success;
        TxHash = txHash;
        BlockNumber = blockNumber;
        Memo = memo;
        Events = events;
    }
}

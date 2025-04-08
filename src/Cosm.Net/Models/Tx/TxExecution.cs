namespace Cosm.Net.Models.Tx;
public class TxExecution(uint code, string txHash, long blockNumber, string rawLog, string memo, ulong gasWanted, Coin[] txFees, TxEvent[] events)
{
    public bool Success => Code == 0;
    public uint Code { get; } = code;
    public string TxHash { get; } = txHash;
    public long BlockNumber { get; } = blockNumber;
    public string RawLog { get; } = rawLog;
    public string Memo { get; } = memo;
    public ulong GasWanted { get; } = gasWanted;
    public Coin[] TxFees { get; } = txFees;
    public TxEvent[] Events { get; } = events;
}

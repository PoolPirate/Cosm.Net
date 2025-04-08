namespace Cosm.Net.Models.Tx;
public class TxSubmission(long code, string txHash, string rawLog)
{
    public long Code { get; } = code;
    public string TxHash { get; } = txHash;
    public string RawLog { get; } = rawLog;
}

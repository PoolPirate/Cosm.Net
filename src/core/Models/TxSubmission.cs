namespace Cosm.Net.Models;
public class TxSubmission
{
    public long Code { get; }
    public string TxHash { get; }
    public string RawLog { get; }

    public TxSubmission(long code, string txHash, string rawLog)
    {
        Code = code;
        TxHash = txHash;
        RawLog = rawLog;
    }
}

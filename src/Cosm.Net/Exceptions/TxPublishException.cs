namespace Cosm.Net.Exceptions;
public class TxPublishException(long code, string reason)
    : Exception($"Eror Code {code}: {reason}")
{
    public long Code { get; } = code;
}

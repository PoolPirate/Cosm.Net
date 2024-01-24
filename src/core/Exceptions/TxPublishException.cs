namespace Cosm.Net.Exceptions;
public class TxPublishException : Exception
{
    public long Code { get; }

    public TxPublishException(long code, string reason)
        :base($"Eror Code {code}: {reason}")
    {
        Code = code;
    }
}

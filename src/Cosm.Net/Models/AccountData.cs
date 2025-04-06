namespace Cosm.Net.Models;
public class AccountData
{
    public ulong AccountNumber { get; }
    public ulong Sequence { get; }

    public AccountData(ulong accountNumber, ulong sequence)
    {
        AccountNumber = accountNumber;
        Sequence = sequence;
    }
}

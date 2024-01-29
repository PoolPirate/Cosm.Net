namespace Cosm.Net.Models;
public sealed class Coin
{
    public string Denom { get; }
    public ulong Amount { get; }

    public Coin(string denom, ulong amount)
    {
        Denom = denom;
        Amount = amount;
    }
}

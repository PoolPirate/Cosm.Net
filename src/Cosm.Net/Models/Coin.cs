using System.Numerics;

namespace Cosm.Net.Models;
public sealed class Coin
{
    public string Denom { get; }
    public BigInteger Amount { get; }

    public Coin(string denom, ulong amount)
    {
        Denom = denom;
        Amount = amount;
    }

    public Coin(string denom, BigInteger amount)
    {
        Denom = denom;
        Amount = amount;
    }
}

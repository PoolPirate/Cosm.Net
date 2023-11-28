using System.Numerics;

namespace Cosm.Net.Models;
public class GasFeeAmount
{
    public ulong GasWanted { get; }
    public string FeeDenom {  get; }
    public ulong FeeAmount {  get; }

    public GasFeeAmount(ulong gasWanted, string feeDenom, ulong feeAmount)
    {
        GasWanted = gasWanted;
        FeeDenom = feeDenom;
        FeeAmount = feeAmount;
    }

    public static GasFeeAmount FromFixedGasPrice(ulong gasWanted, string feeDenom, decimal gasPrice)
        => new GasFeeAmount(gasWanted, feeDenom, (ulong) Math.Ceiling(gasWanted * gasPrice));
}

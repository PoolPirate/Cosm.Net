using Cosm.Net.Models;

namespace Cosm.Net.Services;
internal class ConstantGasFeeProvider : IGasFeeProvider<ConstantGasFeeProvider.Configuration>
{
    public record Configuration(string BaseGasFeeDenom, decimal GasPrice);

    public string BaseGasFeeDenom { get; private set; }
    public decimal GasPrice { get; private set; }

    public ConstantGasFeeProvider(Configuration configuration)
    {
        BaseGasFeeDenom = configuration.BaseGasFeeDenom;
        GasPrice = configuration.GasPrice;
    }

    public ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted)
        => ValueTask.FromResult(new Coin(BaseGasFeeDenom, (ulong) Math.Ceiling(gasWanted * GasPrice)));
}

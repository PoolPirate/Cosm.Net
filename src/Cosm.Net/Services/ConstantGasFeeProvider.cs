using Cosm.Net.Configuration;
using Cosm.Net.Models;

namespace Cosm.Net.Services;
internal class ConstantGasFeeProvider : IGasFeeProvider<ConstantGasFeeProvider.Configuration>
{
    public record Configuration(string GasFeeDenom, decimal GasPrice);

    public string GasFeeDenom { get; }

    private readonly IGasBufferConfiguration _gasBufferConfiguration;
    private readonly decimal _gasPrice;

    public ConstantGasFeeProvider(Configuration configuration, IGasBufferConfiguration gasBufferConfiguration)
    {
        _gasBufferConfiguration = gasBufferConfiguration;
        _gasPrice = configuration.GasPrice;
        GasFeeDenom = configuration.GasFeeDenom;
    }

    public ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted)
        => ValueTask.FromResult(new Coin(GasFeeDenom, 
            (ulong) Math.Ceiling(gasWanted * _gasPrice)));

    public ulong ApplyGasBuffers(ulong gasWanted, double? gasMultiplier = null, ulong? gasOffset = null) 
        => (ulong) (gasWanted * (gasMultiplier ?? _gasBufferConfiguration.GasMultiplier))
            + (gasOffset ?? _gasBufferConfiguration.GasOffset);
}

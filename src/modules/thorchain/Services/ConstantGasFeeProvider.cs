using Cosm.Net.Configuration;
using Cosm.Net.Models;

namespace Cosm.Net.Services;
internal class ConstantGasFeeProvider : IGasFeeProvider<ConstantGasFeeProvider.Configuration>
{
    public record Configuration(Coin GasFee);

    public Coin GasFee { get; }

    public string GasFeeDenom => GasFee.Denom;

    private readonly IGasBufferConfiguration _gasBufferConfiguration;

    public ConstantGasFeeProvider(Configuration configuration, IGasBufferConfiguration gasBufferConfiguration)
    {
        GasFee = configuration.GasFee;
        _gasBufferConfiguration = gasBufferConfiguration;
    }

    public ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(GasFee);

    public ulong ApplyGasBuffers(ulong gasWanted, double? gasMultiplier = null, ulong? gasOffset = null)
        => (ulong) (gasWanted * (gasMultiplier ?? _gasBufferConfiguration.GasMultiplier))
            + (gasOffset ?? _gasBufferConfiguration.GasOffset);
}

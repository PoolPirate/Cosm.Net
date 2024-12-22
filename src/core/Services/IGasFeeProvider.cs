using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IGasFeeProvider
{
    public string GasFeeDenom { get; }

    public ulong ApplyGasBuffers(ulong gasWanted, double? gasMultiplier = null, ulong? gasOffset = null);
    public ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted, CancellationToken cancellationToken = default);
}

public interface IGasFeeProvider<TConfiguration> : IGasFeeProvider;

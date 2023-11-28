using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IGasFeeProvider
{
    public string BaseGasFeeDenom { get; }

    public ValueTask<GasFeeAmount> GetFeeForGasAsync(ulong gasWanted);
}

public interface IGasFeeProvider<TConfiguration> : IGasFeeProvider;

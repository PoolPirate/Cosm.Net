using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IGasFeeProvider
{
    public string BaseGasFeeDenom { get; }

    public ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted);
}

public interface IGasFeeProvider<TConfiguration> : IGasFeeProvider;

using Cosm.Net.Configuration;
using Cosm.Net.Models;
using Cosm.Net.Modules;

namespace Cosm.Net.Services;
internal class InitiaTxModuleGasFeeProvider : IGasFeeProvider<InitiaTxModuleGasFeeProvider.Configuration>
{
    private const decimal GasPriceDenom = 1000000000000000000;

    public record Configuration(string GasFeeDenom, decimal GasPriceOffset, int RefreshIntervalSeconds);

    public string GasFeeDenom { get; }

    private readonly IGasBufferConfiguration _gasBufferConfiguration;
    private readonly IInitiaTxV1Module _initiaTxModule;

    private readonly decimal _gasPriceOffset;
    private decimal? _currentBaseGasPrice;
    private readonly int _refreshIntervalSeconds;

    private bool _started;
    private readonly object _startupLock = new object();
    private readonly PeriodicTimer _refreshTimer;

    public InitiaTxModuleGasFeeProvider(Configuration configuration, IGasBufferConfiguration gasBufferConfiguration, IInitiaTxV1Module initiaTxModule)
    {
        _initiaTxModule = initiaTxModule;
        _gasBufferConfiguration = gasBufferConfiguration;

        _gasPriceOffset = configuration.GasPriceOffset;
        _refreshIntervalSeconds = configuration.RefreshIntervalSeconds;
        GasFeeDenom = configuration.GasFeeDenom;

        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(_refreshIntervalSeconds));
    }

    private async Task RunGasPriceUpdaterAsync()
    {
        while(await _refreshTimer.WaitForNextTickAsync())
        {
            try
            {
                _currentBaseGasPrice = await GetCurrentBaseFeeAsync();
            }
            catch
            {
            }
        }
    }

    private async Task<decimal> GetCurrentBaseFeeAsync(CancellationToken cancellationToken = default)
    {
        var gasPriceResponse = await _initiaTxModule.GasPriceAsync(GasFeeDenom, cancellationToken: cancellationToken);
        decimal gasPricee18 = decimal.Parse(gasPriceResponse.GasPrice.Amount);
        return gasPricee18 / GasPriceDenom;
    }

    public async ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted, CancellationToken cancellationToken = default)
    {
        if(_currentBaseGasPrice.HasValue)
        {
            return new Coin(GasFeeDenom, (ulong) Math.Ceiling(gasWanted * (_currentBaseGasPrice.Value + _gasPriceOffset)));
        }

        lock(_startupLock)
        {
            if(!_started)
            {
                _started = true;
                _ = Task.Run(RunGasPriceUpdaterAsync, CancellationToken.None);
            }
        }

        decimal gasPrice = await GetCurrentBaseFeeAsync(cancellationToken);
        return new Coin(GasFeeDenom, (ulong) Math.Ceiling(gasWanted * (gasPrice + _gasPriceOffset)));
    }

    public ulong ApplyGasBuffers(ulong gasWanted, double? gasMultiplier = null, ulong? gasOffset = null)
        => (ulong) (gasWanted * (gasMultiplier ?? _gasBufferConfiguration.GasMultiplier))
            + (gasOffset ?? _gasBufferConfiguration.GasOffset);
}

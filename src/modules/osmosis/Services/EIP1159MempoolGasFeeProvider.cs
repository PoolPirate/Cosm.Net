using Cosm.Net.Models;
using Cosm.Net.Modules;

namespace Cosm.Net.Services;
internal class EIP1159MempoolGasFeeProvider : IGasFeeProvider<EIP1159MempoolGasFeeProvider.Configuration>
{
    private const decimal GasPriceDenom = 1000000000000000000;

    public record Configuration(string FeeDenom, decimal GasPriceOffset, int RefreshIntervalSeconds);

    private readonly ITxFeesModule _txFeesModule;

    public string BaseGasFeeDenom { get; private set; }
    public decimal GasPriceOffset { get; private set; }
    public decimal? CurrentBaseGasPrice { get; private set; } = null;
    public int RefreshIntervalSeconds { get; private set; }

    private bool _started;
    private readonly object _startupLock = new object();
    private readonly PeriodicTimer _refreshTimer;

    public EIP1159MempoolGasFeeProvider(Configuration configuration, ITxFeesModule txFeesModule)
    {
        _txFeesModule = txFeesModule;

        BaseGasFeeDenom = configuration.FeeDenom;
        GasPriceOffset = configuration.GasPriceOffset;
        RefreshIntervalSeconds = configuration.RefreshIntervalSeconds;

        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(RefreshIntervalSeconds));
    }

    private async Task RunGasPriceUpdaterAsync()
    {
        while(await _refreshTimer.WaitForNextTickAsync())
        {
            try
            {
                CurrentBaseGasPrice = await GetCurrentBaseFeeAsync();
            }
            catch
            {
            }
        }
    }

    private async Task<decimal> GetCurrentBaseFeeAsync()
    {
        decimal gasPricee18 = Decimal.Parse((await _txFeesModule.GetEipBaseFeeAsync()).BaseFee);
        return gasPricee18 / GasPriceDenom;
    }

    public async ValueTask<Coin> GetFeeForGasAsync(ulong gasWanted)
    {
        if(CurrentBaseGasPrice.HasValue)
        {
            return new Coin(BaseGasFeeDenom, (ulong) Math.Ceiling(gasWanted * (CurrentBaseGasPrice.Value + GasPriceOffset)));
        }

        lock(_startupLock)
        {
            if(!_started)
            {
                _started = true;
                _ = Task.Run(RunGasPriceUpdaterAsync);
            }
        }

        decimal gasPrice = await GetCurrentBaseFeeAsync();
        return new Coin(BaseGasFeeDenom, (ulong) Math.Ceiling(gasWanted * (gasPrice + GasPriceOffset)));
    }
}

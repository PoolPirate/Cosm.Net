using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IChainDataProvider
{
    public Task<string> GetChainIdAsync();
    public Task<AccountData> GetAccountDataAsync(string address);
}

public interface IChainDataProvider<TConfiguration> : IChainDataProvider;
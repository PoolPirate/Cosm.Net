using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IChainDataProvider
{
    public Task<AccountData> GetAccountDataAsync(string address);
}

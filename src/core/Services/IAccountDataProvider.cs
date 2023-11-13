using Cosm.Net.Models;

namespace Cosm.Net.Services;
public interface IAccountDataProvider
{
    public Task<AccountData> GetAccountDataAsync();
}

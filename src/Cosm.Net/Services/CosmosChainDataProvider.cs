using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosmos.Auth.V1Beta1;

namespace Cosm.Net.Services;
public class CosmosChainDataProvider : IChainDataProvider
{
    private readonly IAuthModuleAdapter _authModule;
    private readonly ITendermintModuleAdapter _tendermintModule;
    private readonly IOfflineSigner _signer;

    public CosmosChainDataProvider(IAuthModuleAdapter authModule, ITendermintModuleAdapter tendermintModule,
       IOfflineSigner signer)
    {
        _authModule = authModule;
        _tendermintModule = tendermintModule;
        _signer = signer;
    }

    public async Task<AccountData> GetAccountDataAsync(string address)
    {
        var accountData = await _authModule.AccountAsync(address);
        var account = BaseAccount.Parser.ParseFrom(accountData);

        return new AccountData(account.AccountNumber, account.Sequence);
    }

    public Task<string> GetChainIdAsync() 
        => _tendermintModule.GetChainId();
}

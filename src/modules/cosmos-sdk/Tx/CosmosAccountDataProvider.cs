using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosmos.Auth.V1Beta1;

namespace Cosm.Net.CosmosSdk.Tx;
public class CosmosAccountDataProvider : IAccountDataProvider
{
    private readonly IAuthModule _authModule;
    private readonly ITxChainConfiguration _chainConfiguration;
    private readonly IOfflineSigner _signer;

    public CosmosAccountDataProvider(IAuthModule authModule, ITxChainConfiguration chainConfiguration, IOfflineSigner signer)
    {
        _authModule = authModule;
        _chainConfiguration = chainConfiguration;
        _signer = signer;
    }

    public async Task<AccountData> GetAccountDataAsync()
    {
        string accountAddress = _signer.GetAddress(_chainConfiguration.Prefix);
        var accountResponse = await _authModule.AccountAsync(accountAddress);

        var account = BaseAccount.Parser.ParseFrom(accountResponse.Account.Value);

        return new AccountData(account.AccountNumber, account.Sequence);
    }
}

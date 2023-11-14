using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosmos.Auth.V1Beta1;

namespace Cosm.Net.CosmosSdk.Tx;
public class CosmosChainDataProvider : IChainDataProvider
{
    private readonly IAuthModule _authModule;
    private readonly ITendermintService _tendermintService;
    private readonly IOfflineSigner _signer;

    public CosmosChainDataProvider(IAuthModule authModule, ITendermintService tendermintService,
       IOfflineSigner signer)
    {
        _authModule = authModule;
        _tendermintService = tendermintService;
        _signer = signer;
    }

    public async Task<AccountData> GetAccountDataAsync(string address)
    {
        var accountResponse = await _authModule.AccountAsync(address);
        var account = BaseAccount.Parser.ParseFrom(accountResponse.Account.Value);

        return new AccountData(account.AccountNumber, account.Sequence);
    }

    public async Task<string> GetChainIdAsync()
    {
        var nodeInfo = await _tendermintService.GetNodeInfoAsync();
        return nodeInfo.DefaultNodeInfo.Network;
    }
}

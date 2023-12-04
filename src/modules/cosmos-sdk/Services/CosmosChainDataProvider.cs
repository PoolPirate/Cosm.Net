using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosmos.Auth.V1Beta1;

namespace Cosm.Net.CosmosSdk.Services;
public class CosmosChainDataProvider : IChainDataProvider
{
    private readonly IAuthModule _authModule;
    private readonly ITendermintModule _tendermintModule;
    private readonly IOfflineSigner _signer;

    public CosmosChainDataProvider(IAuthModule authModule, ITendermintModule tendermintModule,
       IOfflineSigner signer)
    {
        _authModule = authModule;
        _tendermintModule = tendermintModule;
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
        var nodeInfo = await _tendermintModule.GetNodeInfoAsync();
        return nodeInfo.DefaultNodeInfo.Network;
    }
}

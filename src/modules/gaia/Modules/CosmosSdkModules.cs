using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Google.Protobuf;
using Grpc.Core;

namespace Cosm.Net.Gaia.Modules;
//Module not exposed on this chain
//internal partial class AccountModule : IModule<AccountModule, Cosmos.Accounts.V1.Query.QueryClient> { }
internal partial class AuthModule : IModule<AuthModule, Cosmos.Auth.V1Beta1.Query.QueryClient>, IAuthModuleAdapter
{
    async Task<AccountData> IAuthModuleAdapter.GetAccountAsync(string address, Metadata? headers,
        DateTime? deadline, CancellationToken cancellationToken)
    {
        var accountData = await AccountAsync(address, headers, deadline, cancellationToken);
        var account = Cosmos.Auth.V1Beta1.BaseAccount.Parser.ParseFrom(accountData.Account.Value);
        return new AccountData(account.AccountNumber, account.Sequence);
    }
}
internal partial class AuthzModule : IModule<AuthzModule, Cosmos.Authz.V1Beta1.Query.QueryClient> { }
internal partial class BankModule : IModule<BankModule, Cosmos.Bank.V1Beta1.Query.QueryClient> { }
//Module not exposed on this chain
//internal partial class CircuitModule : IModule<CircuitModule, Cosmos.Circuit.V1.Query.QueryClient> { }
internal partial class ConsensusModule : IModule<ConsensusModule, Cosmos.Consensus.V1.Query.QueryClient> { }
internal partial class DistributionModule : IModule<DistributionModule, Cosmos.Staking.V1Beta1.Query.QueryClient> { }
internal partial class EvidenceModule : IModule<EvidenceModule, Cosmos.Evidence.V1Beta1.Query.QueryClient> { }
internal partial class FeeGrantModule : IModule<FeeGrantModule, Cosmos.Feegrant.V1Beta1.Query.QueryClient> { }
internal partial class GovModule : IModule<GovModule, Cosmos.Gov.V1Beta1.Query.QueryClient> { }
internal partial class MintModule : IModule<MintModule, Cosmos.Mint.V1Beta1.Query.QueryClient> { }
internal partial class NftModule : IModule<NftModule, Cosmos.Nft.V1Beta1.Query.QueryClient> { }
internal partial class ParamsModule : IModule<ParamsModule, Cosmos.Params.V1Beta1.Query.QueryClient> { }
//Module not exposed on this chain
//internal partial class ProtocolPoolModule : IModule<ProtocolPoolModule, Cosmos.Protocolpool.V1.Query.QueryClient> { }
internal partial class SlashingModule : IModule<SlashingModule, Cosmos.Slashing.V1Beta1.Query.QueryClient> { }
internal partial class StakingModule : IModule<StakingModule, Cosmos.Staking.V1Beta1.Query.QueryClient> { }
internal partial class TendermintModule : IModule<TendermintModule, Cosmos.Base.Tendermint.V1Beta1.Service.ServiceClient>, ITendermintModuleAdapter
{
    async Task<string> ITendermintModuleAdapter.GetChainId(Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
    {
        var nodeInfo = await GetNodeInfoAsync(headers, deadline, cancellationToken);
        return nodeInfo.DefaultNodeInfo.Network;
    }
}
internal partial class TxModule : IModule<TxModule, Cosmos.Tx.V1Beta1.Service.ServiceClient>, ITxModuleAdapter
{
    async Task<TxSubmission> ITxModuleAdapter.BroadcastTxAsync(ByteString txBytes, BroadcastMode mode, Metadata? headers,
        DateTime? deadline, CancellationToken cancellationToken)
    {
        var signMode = mode switch
        {
            BroadcastMode.Unspecified => Cosmos.Tx.V1Beta1.BroadcastMode.Unspecified,
            BroadcastMode.Sync => Cosmos.Tx.V1Beta1.BroadcastMode.Sync,
            BroadcastMode.Async => Cosmos.Tx.V1Beta1.BroadcastMode.Async,
            _ => throw new InvalidOperationException("Unsupported BroadcastMode")
        };

        var response = await BroadcastTxAsync(txBytes, signMode, headers, deadline, cancellationToken);
        return new TxSubmission(response.TxResponse.Code, response.TxResponse.Txhash, response.TxResponse.RawLog);
    }
    async Task<TxSimulation> ITxModuleAdapter.SimulateAsync(ByteString txBytes, Metadata? headers,
        DateTime? deadline, CancellationToken cancellationToken)
    {
        var response = await SimulateAsync(txBytes, headers, deadline, cancellationToken);

        return new TxSimulation(
            response.GasInfo.GasUsed,
            response.Result.Events
                .Select(x => new TxEvent(
                    x.Type, x.Attributes.Select(y => new TxEventAttribute(y.Key, y.Value)).ToArray()))
                .ToArray()
        );
    }
}
internal partial class UpgradeModule : IModule<UpgradeModule, Cosmos.Upgrade.V1Beta1.Query.QueryClient> { }
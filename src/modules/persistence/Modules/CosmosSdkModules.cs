using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Google.Protobuf;
using Grpc.Core;

namespace Cosm.Net.Modules;
//internal partial class AccountModule : IModule<AccountModule, global::Cosmos.Accounts.V1.Query.QueryClient> { }
internal partial class AuthModule : IModule<AuthModule, global::Cosmos.Auth.V1Beta1.Query.QueryClient>, IAuthModuleAdapter
{
    async Task<AccountData> IAuthModuleAdapter.GetAccountAsync(string address, Metadata? headers,
        DateTime? deadline, CancellationToken cancellationToken)
    {
        var accountData = await AccountAsync(address, headers, deadline, cancellationToken);
        var account = Cosmos.Auth.V1Beta1.BaseAccount.Parser.ParseFrom(accountData.Account.Value);
        return new AccountData(account.AccountNumber, account.Sequence);
    }
}
internal partial class AuthzModule : IModule<AuthzModule, global::Cosmos.Authz.V1Beta1.Query.QueryClient> { }
internal partial class BankModule : IModule<BankModule, global::Cosmos.Bank.V1Beta1.Query.QueryClient> { }
//internal partial class CircuitModule : IModule<CircuitModule, global::Cosmos.Circuit.V1.Query.QueryClient> { }
//internal partial class ConsensusModule : IModule<ConsensusModule, global::Cosmos.Consensus.V1.Query.QueryClient> { }
internal partial class DistributionModule : IModule<DistributionModule, global::Cosmos.Staking.V1Beta1.Query.QueryClient> { }
internal partial class EvidenceModule : IModule<EvidenceModule, global::Cosmos.Evidence.V1Beta1.Query.QueryClient> { }
internal partial class FeeGrantModule : IModule<FeeGrantModule, global::Cosmos.Feegrant.V1Beta1.Query.QueryClient> { }
internal partial class GovModule : IModule<GovModule, global::Cosmos.Gov.V1Beta1.Query.QueryClient> { }
internal partial class MintModule : IModule<MintModule, global::Cosmos.Mint.V1Beta1.Query.QueryClient> { }
//internal partial class NftModule : IModule<NftModule, global::Cosmos.Nft.V1Beta1.Query.QueryClient> { }
internal partial class ParamsModule : IModule<ParamsModule, global::Cosmos.Params.V1Beta1.Query.QueryClient> { }
//internal partial class ProtocolPoolModule : IModule<ProtocolPoolModule, global::Cosmos.Protocolpool.V1.Query.QueryClient> { }
internal partial class SlashingModule : IModule<SlashingModule, global::Cosmos.Slashing.V1Beta1.Query.QueryClient> { }
internal partial class StakingModule : IModule<StakingModule, global::Cosmos.Staking.V1Beta1.Query.QueryClient> { }
internal partial class TendermintModule : IModule<TendermintModule, global::Cosmos.Base.Tendermint.V1Beta1.Service.ServiceClient>, ITendermintModuleAdapter
{
    async Task<string> ITendermintModuleAdapter.GetChainId(Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
    {
        var nodeInfo = await GetNodeInfoAsync(headers, deadline, cancellationToken);
        return nodeInfo.DefaultNodeInfo.Network;
    }
}
internal partial class TxModule : IModule<TxModule, global::Cosmos.Tx.V1Beta1.Service.ServiceClient>, ITxModuleAdapter
{
    async Task<TxSubmission> ITxModuleAdapter.BroadcastTxAsync(ByteString txBytes, BroadcastMode mode, Metadata? headers,
        DateTime? deadline, CancellationToken cancellationToken)
    {
        var signMode = mode switch
        {
            BroadcastMode.Unspecified => global::Cosmos.Tx.V1Beta1.BroadcastMode.Unspecified,
            BroadcastMode.Sync => global::Cosmos.Tx.V1Beta1.BroadcastMode.Sync,
            BroadcastMode.Async => global::Cosmos.Tx.V1Beta1.BroadcastMode.Async,
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
                    null, x.Type, x.Attributes.Select(y => new TxEventAttribute(y.Key, y.Value)).ToArray()))
                .ToArray()
        );
    }

    async Task<TxExecution> ITxModuleAdapter.GetTxByHashAsync(string txHash,
        Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
    {
        var tx = await GetTxAsync(txHash, headers, deadline, cancellationToken);
        var events = tx.TxResponse.Logs.Count < 2
            ? tx.TxResponse.Events.Select(e =>
            {
                var msgIndexRaw = e.Attributes.LastOrDefault(x => x.Key == "msg_index")?.Value;
                return new TxEvent(msgIndexRaw is null ? null : int.Parse(msgIndexRaw), e.Type,
                    e.Attributes.Select(a => new TxEventAttribute(a.Key, a.Value)).ToList().AsReadOnly());
            })
            : tx.TxResponse.Logs.SelectMany(
                x => x.Events.Select(
                    e => new TxEvent((int) x.MsgIndex, e.Type, e.Attributes.Select(
                        a => new TxEventAttribute(a.Key, a.Value)).ToList().AsReadOnly())));

        return new TxExecution(tx.TxResponse.Code == 0, txHash, tx.TxResponse.Height, tx.Tx.Body.Memo, events.ToArray());
    }
}
internal partial class UpgradeModule : IModule<UpgradeModule, global::Cosmos.Upgrade.V1Beta1.Query.QueryClient> { }

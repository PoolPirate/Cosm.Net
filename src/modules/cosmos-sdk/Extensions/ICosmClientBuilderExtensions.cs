using Cosm.Net.Client;
using Cosm.Net.CosmosSdk.Tx;
using Cosm.Net.Modules;

namespace Cosm.Net.CosmosSdk.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddCosmosSdk(this CosmClientBuilder builder)
    {
        builder.RegisterModule<IABCI, ABCI>();
        builder.RegisterModule<IAccountModule, AccountModule>();
        builder.RegisterModule<IAuthModule, AuthModule>();
        builder.RegisterModule<IAuthzModule, AuthzModule>();
        builder.RegisterModule<IBankModule, BankModule>();
        builder.RegisterModule<ICircuitModule, CircuitModule>();
        builder.RegisterModule<IConsensusModule, ConsensusModule>();
        builder.RegisterModule<IDistributionModule, DistributionModule>();
        builder.RegisterModule<IEvidenceModule, EvidenceModule>();
        builder.RegisterModule<IFeeGrantModule, FeeGrantModule>();
        builder.RegisterModule<IGovModule, GovModule>();
        builder.RegisterModule<IMintModule, MintModule>();
        builder.RegisterModule<INftModule, NftModule>();
        builder.RegisterModule<IParamsModule, ParamsModule>();
        builder.RegisterModule<IProtocolPoolModule, ProtocolPoolModule>();
        builder.RegisterModule<ISlashingModule, SlashingModule>();
        builder.RegisterModule<IStakingModule, StakingModule>();
        builder.RegisterModule<ITxModule, TxModule>();
        builder.RegisterModule<IUpgradeModule, UpgradeModule>();

        return builder;
    }

    public static CosmTxClientBuilder UseCosmosTxStructure(this CosmTxClientBuilder builder)
    {
        builder.WithTxEncoder<CosmosTxEncoder>();
        builder.WithTxPublisher<TxModulePublisher>();
        builder.WithAccountDataProvider<CosmosAccountDataProvider>();

        return builder;
    }
}

using Cosm.Net.Client;
using Cosm.Net.Modules;

namespace Cosm.Net.CosmosSdk.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddCosmosSdk(this CosmClientBuilder builder)
    {
        builder.RegisterModule<AccountModule>();
        builder.RegisterModule<AuthModule>();
        builder.RegisterModule<AuthzModule>();
        builder.RegisterModule<BankModule>();
        builder.RegisterModule<CircuitModule>();
        builder.RegisterModule<ConsensusModule>();
        builder.RegisterModule<DistributionModule>();
        builder.RegisterModule<EvidenceModule>();
        builder.RegisterModule<FeeGrantModule>();
        builder.RegisterModule<GovModule>();
        builder.RegisterModule<MintModule>();
        builder.RegisterModule<NftModule>();
        builder.RegisterModule<ParamsModule>();
        builder.RegisterModule<ProtocolPoolModule>();
        builder.RegisterModule<SlashingModule>();
        builder.RegisterModule<StakingModule>();
        builder.RegisterModule<TxModule>();
        builder.RegisterModule<UpgradeModule>();

        return builder;
    }
}

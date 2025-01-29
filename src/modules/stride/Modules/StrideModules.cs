namespace Cosm.Net.Modules;

internal partial class AirdropModule : IModule<AirdropModule, Stride.Airdrop.Query.QueryClient> { }
internal partial class AutopilotModule : IModule<AutopilotModule, Stride.Autopilot.Query.QueryClient> { }
internal partial class ClaimModule : IModule<ClaimModule, Stride.Claim.Query.QueryClient> { }
internal partial class EpochsModule : IModule<EpochsModule, Stride.Epochs.Query.QueryClient> { }
internal partial class IACallbacksModule : IModule<IACallbacksModule, Stride.Icacallbacks.Query.QueryClient> { }
internal partial class IAOracleModule : IModule<IAOracleModule, Stride.Icaoracle.Query.QueryClient> { }
internal partial class MintModule : IModule<MintModule, Stride.Mint.V1Beta1.Query.QueryClient> { }
internal partial class RecordsModule : IModule<RecordsModule, Stride.Records.Query.QueryClient> { }
internal partial class StakeDymcModule : IModule<StakeDymcModule, Stride.Stakedym.Query.QueryClient> { }
internal partial class StakeIbcModule : IModule<StakeIbcModule, Stride.Stakeibc.Query.QueryClient> { }
internal partial class StakeTiaModule : IModule<StakeTiaModule, Stride.Staketia.Query.QueryClient> { }
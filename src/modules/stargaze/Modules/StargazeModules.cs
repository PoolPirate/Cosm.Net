namespace Cosm.Net.Modules;

internal partial class TokenFactoryModule : IModule<TokenFactoryModule, Osmosis.Tokenfactory.V1Beta1.Query.QueryClient> { }
internal partial class MintModule : IModule<MintModule, Publicawesome.Stargaze.Mint.V1Beta1.Query.QueryClient> { }
internal partial class GlobalFeeModule : IModule<GlobalFeeModule, Publicawesome.Stargaze.Globalfee.V1.Query.QueryClient> { }
internal partial class CronModule : IModule<CronModule, Publicawesome.Stargaze.Cron.V1.Query.QueryClient> { }
internal partial class AllocModule : IModule<AllocModule, Publicawesome.Stargaze.Alloc.V1Beta1.Query.QueryClient> { }
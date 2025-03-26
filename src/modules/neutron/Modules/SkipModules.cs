namespace Cosm.Net.Modules;
internal partial class AuctionModule : IModule<AuctionModule, Sdk.Auction.V1.Query.QueryClient> { }
internal partial class MempoolModule : IModule<MempoolModule, Sdk.Mempool.V1.Service.ServiceClient> { }
internal partial class FeeMarketModule : IModule<FeeMarketModule, Feemarket.Feemarket.V1.Query.QueryClient> { }
internal partial class SkipConnectMarketmapModule : IModule<SkipConnectMarketmapModule, Slinky.Marketmap.V1.Query.QueryClient> { }
internal partial class SkipConnectOracleModule : IModule<SkipConnectOracleModule, Slinky.Oracle.V1.Query.QueryClient> { }
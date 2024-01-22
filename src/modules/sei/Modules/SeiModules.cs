using Cosm.Net.Modules;

namespace Cosm.Net.Sei.Modules;
internal partial class DexModule : IModule<DexModule, Seiprotocol.Seichain.Dex.Query.QueryClient> { }
internal partial class EpochModule : IModule<EpochModule, Seiprotocol.Seichain.Epoch.Query.QueryClient> { }
internal partial class MintModule : IModule<MintModule, Seiprotocol.Seichain.Mint.Query.QueryClient> { }
internal partial class OracleModule : IModule<OracleModule, Seiprotocol.Seichain.Oracle.Query.QueryClient> { }
internal partial class TokenFactoryModule : IModule<TokenFactoryModule, Seiprotocol.Seichain.Tokenfactory.Query.QueryClient> { }
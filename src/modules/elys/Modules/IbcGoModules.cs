namespace Cosm.Net.Modules;
internal partial class IbcFeeModule : IModule<IbcFeeModule,Ibc.Applications.Fee.V1.Query.QueryClient> { }
internal partial class IbcInterchainAccountsControllerModule
    : IModule<IbcInterchainAccountsControllerModule,Ibc.Applications.InterchainAccounts.Controller.V1.Query.QueryClient> { }
internal partial class IbcInterchainAccountsHostModule
    : IModule<IbcInterchainAccountsHostModule,Ibc.Applications.InterchainAccounts.Host.V1.Query.QueryClient> { }
internal partial class IbcTransferModule : IModule<IbcTransferModule,Ibc.Applications.Transfer.V1.Query.QueryClient> { }
internal partial class IbcChannelModule : IModule<IbcChannelModule,Ibc.Core.Channel.V1.Query.QueryClient> { }
internal partial class IbcClientModule : IModule<IbcClientModule,Ibc.Core.Client.V1.Query.QueryClient> { }
internal partial class IbcConnectionModule : IModule<IbcConnectionModule,Ibc.Core.Connection.V1.Query.QueryClient> { }

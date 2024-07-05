namespace Cosm.Net.Modules;
internal partial class ContractManagerModule : IModule<ContractManagerModule,Neutron.Contractmanager.Query.QueryClient> { }
internal partial class CronModule : IModule<CronModule,Neutron.Cron.Query.QueryClient> { }
internal partial class DexModule : IModule<DexModule,Neutron.Dex.Query.QueryClient> { }
internal partial class FeeBurnerModule : IModule<FeeBurnerModule,Neutron.Feeburner.Query.QueryClient> { }
internal partial class FeeRefunderModule : IModule<FeeRefunderModule,Neutron.Feerefunder.Query.QueryClient> { }
internal partial class InterchainQueriesModule : IModule<InterchainQueriesModule,Neutron.Interchainqueries.Query.QueryClient> { }
internal partial class InterchainTxsModule : IModule<InterchainTxsModule,Neutron.Interchaintxs.V1.Query.QueryClient> { }
internal partial class TransferModule : IModule<TransferModule,Neutron.Transfer.Query.QueryClient> { }
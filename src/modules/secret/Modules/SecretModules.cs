using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Secret.Modules;
internal partial class SecretModules
{
    internal partial class ComputeModule : IModule<ComputeModule, global::Secret.Compute.V1Beta1.Query.QueryClient>, IWasmAdapater
    {
        ITxMessage IWasmAdapater.EncodeContractCall(string contractAddress, ByteString encodedRequest, IEnumerable<Coin> funds, string? txSender)
        {
            
            return null!;
        }
        Task<ByteString> IWasmAdapater.SmartContractStateAsync(string contractAddress, ByteString queryData) 
            => throw new NotImplementedException();
    }
    internal partial class EmergencyButtonModule : IModule<EmergencyButtonModule, global::Secret.Emergencybutton.V1Beta1.Query.QueryClient> { }
    internal partial class InterTxModule : IModule<InterTxModule, global::Secret.Intertx.V1Beta1.Query.QueryClient> { }
    internal partial class RegistrationModule : IModule<RegistrationModule, global::Secret.Registration.V1Beta1.Query.QueryClient> { }
}

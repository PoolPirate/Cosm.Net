using Cosm.Net.Modules;
using Google.Protobuf;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface IAuthModuleAdapter : IModule
{
    public Task<ByteString> AccountAsync(string address, Metadata? headers = default,
        DateTime? deadline = default, CancellationToken cancellationToken = default);
}

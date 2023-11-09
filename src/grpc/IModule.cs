using Grpc.Net.Client;

namespace Cosm.Net.Client;
public interface IModule<TModule>
{
    internal static abstract TModule FromGrpcChannel(GrpcChannel channel);
}

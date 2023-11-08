using Grpc.Net.Client;

namespace Cosm.Net.Client;
public interface IModule<TModule>
{
    internal abstract static TModule FromGrpcChannel(GrpcChannel channel);
}

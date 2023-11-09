using Grpc.Core;
using Grpc.Net.Client;

namespace Cosm.Net.Base;
public interface IModule<TModule, TService> : ICosmModule<TService>, IModule<TModule>
        where TService : ClientBase
{
}

public interface IModule<TModule>
{
    public static abstract TModule FromGrpcChannel(GrpcChannel channel);
}
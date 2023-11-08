using Grpc.Core;

namespace Cosm.Net.Client;

public interface ICosmModule<TService>
    where TService : ClientBase
{
}

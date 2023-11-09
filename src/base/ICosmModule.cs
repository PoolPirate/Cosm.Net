using Grpc.Core;

namespace Cosm.Net.Base;

public interface ICosmModule<TService>
    where TService : ClientBase
{
}

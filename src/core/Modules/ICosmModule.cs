using Grpc.Core;

namespace Cosm.Net.Modules;

/// <summary>
/// Used by source generator to identify modules
/// </summary>
/// <typeparam name="TService">The grpc query service stub</typeparam>
public interface ICosmModule<TService>
    where TService : ClientBase
{
}

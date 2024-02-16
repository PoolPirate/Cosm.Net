using Cosm.Net.Client.Internal;
using Cosm.Net.Modules;
using Cosm.Net.Tx;

namespace Cosm.Net.Client;
public interface ICosmClient
{
    /// <summary>
    /// Information about the chain that this client is connected to.
    /// </summary>
    public IChainConfiguration Chain { get; }

    /// <summary>
    /// Initializes the client. Must be called before using any other methods.
    /// </summary>
    /// <returns></returns>
    public Task InitializeAsync();

    /// <summary>
    /// Creates an instance of an on-chain module binding.
    /// </summary>
    /// <typeparam name="TModule">The type of module to create</typeparam>
    /// <returns></returns>
    public TModule Module<TModule>() where TModule : IModule;

    /// <summary>
    /// Converts to internal client for accessing unsupported APIs.
    /// </summary>
    /// <returns></returns>
    public IInternalCosmClient AsInternal();
}

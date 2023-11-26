using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client.Internal;
public interface IInternalCosmClientBuilder
{
    public IServiceCollection ServiceCollection { get; }
}

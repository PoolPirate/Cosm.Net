using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public interface IInternalCosmClientBuilder
{
    public IServiceCollection ServiceCollection { get; }
}

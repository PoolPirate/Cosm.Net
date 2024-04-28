namespace Cosm.Net.Services;
public interface IInitializeableService
{
    public ValueTask InitializeAsync(CancellationToken cancellationToken = default);
}

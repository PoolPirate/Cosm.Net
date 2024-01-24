using Cosm.Net.Models;
using Cosm.Net.Modules;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface IAuthModuleAdapter : IModule
{
    public Task<AccountData> GetAccountAsync(string address, Metadata? headers = default,
        DateTime? deadline = default, CancellationToken cancellationToken = default);
}

namespace Cosm.Net.Generators.Proto.Adapters.Internal;
public static class AuthModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Models;
        using Cosm.Net.Modules;
        using Cosm.Net.Services;
        using Grpc.Core;

        namespace Cosm.Net.Adapters.Internal;

        internal class AuthModuleAdapter(AccountParser accountParser, IAuthModule authModule) : IInternalAuthAdapter
        {
            public async Task<AccountData> GetAccountAsync(string address, Metadata? headers,
                DateTime? deadline, CancellationToken cancellationToken)
            {
                var accountData = await authModule.AccountAsync(address, headers, deadline, cancellationToken);
                return accountParser.ParseAccount(accountData.Account);
            }
        }
        """;
}
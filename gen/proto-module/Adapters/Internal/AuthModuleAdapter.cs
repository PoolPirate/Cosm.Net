namespace Cosm.Net.Generators.Proto.Adapters.Internal;
public static class AuthModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Models;
        using Cosm.Net.Modules;
        using Grpc.Core;

        namespace Cosm.Net.Adapters.Internal;

        internal class AuthModuleAdapter(IAuthModule authModule) : IInternalAuthAdapter
        {
            public async Task<AccountData> GetAccountAsync(string address, Metadata? headers,
                DateTime? deadline, CancellationToken cancellationToken)
            {
                var accountData = await authModule.AccountAsync(address, headers, deadline, cancellationToken);
                return !accountData.Account.TryUnpack<Cosmos.Auth.V1Beta1.BaseAccount>(out var account)
                    ? throw new InvalidOperationException($"Cannot parse account type: {accountData.Account.TypeUrl}")
                    : new AccountData(account.AccountNumber, account.Sequence);
            }
        }
        """;
}
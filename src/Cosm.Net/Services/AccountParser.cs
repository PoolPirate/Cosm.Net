using Cosm.Net.Models;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;

namespace Cosm.Net.Services;
public sealed class AccountParser
{
    private readonly Dictionary<MessageDescriptor, Func<Any, AccountData>> _customAccountTypes = [];

    public void RegisterAccountType<T>(MessageDescriptor descriptor, Func<T, AccountData> handler)
        where T : IMessage<T>
    {
        if (descriptor.ClrType != typeof(T))
        {
            throw new InvalidOperationException("Descriptor does not match generic parameter");
        }

        _customAccountTypes.Add(descriptor, (rawAccount) =>
        {
            var account = descriptor.Parser.ParseFrom(rawAccount.Value);
            return handler((T) account);
        });
    }

    public AccountData ParseAccount(Any account)
    {
        foreach(var (descriptor, handler) in _customAccountTypes)
        {
            if(!account.Is(descriptor))
            {
                continue;
            }

            return handler(account);
        }

        return account.TryUnpack<Cosmos.Auth.V1Beta1.BaseAccount>(out var baseAccount)
            ? new AccountData(baseAccount.AccountNumber, baseAccount.Sequence)
            : throw new InvalidOperationException($"Cannot parse account type: {account.TypeUrl}");
    }
}

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
        if(descriptor.ClrType != typeof(T))
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

        if(account.TryUnpack<Cosmos.Auth.V1Beta1.BaseAccount>(out var baseAccount))
        {
            return new AccountData(baseAccount.AccountNumber, baseAccount.Sequence);
        }
        else if(account.TryUnpack<Cosmos.Vesting.V1Beta1.ContinuousVestingAccount>(out var continousVestingAccount))
        {
            return new AccountData(continousVestingAccount.BaseVestingAccount.BaseAccount.AccountNumber, continousVestingAccount.BaseVestingAccount.BaseAccount.Sequence);
        }
        else if(account.TryUnpack<Cosmos.Vesting.V1Beta1.DelayedVestingAccount>(out var delayedVestingAccount))
        {
            return new AccountData(delayedVestingAccount.BaseVestingAccount.BaseAccount.AccountNumber, delayedVestingAccount.BaseVestingAccount.BaseAccount.Sequence);
        }
        else if(account.TryUnpack<Cosmos.Vesting.V1Beta1.BaseVestingAccount>(out var baseVestingAccount))
        {
            return new AccountData(baseVestingAccount.BaseAccount.AccountNumber, baseVestingAccount.BaseAccount.Sequence);
        }
        else if(account.TryUnpack<Cosmos.Vesting.V1Beta1.PermanentLockedAccount>(out var permanentLockedAccount))
        {
            return new AccountData(permanentLockedAccount.BaseVestingAccount.BaseAccount.AccountNumber, permanentLockedAccount.BaseVestingAccount.BaseAccount.Sequence);
        }
        else if(account.TryUnpack<Cosmos.Vesting.V1Beta1.PeriodicVestingAccount>(out var periodicVestingAccount))
        {
            return new AccountData(periodicVestingAccount.BaseVestingAccount.BaseAccount.AccountNumber, periodicVestingAccount.BaseVestingAccount.BaseAccount.Sequence);
        }
        //
        throw new InvalidOperationException($"Cannot parse account type: {account.TypeUrl}");
    }
}

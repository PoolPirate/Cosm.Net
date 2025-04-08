namespace Cosm.Net.Generators.Proto.Adapters;
public static class BankModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Grpc.Core;
        using System.Numerics;
        using Cosm.Net.Modules;
        using Cosm.Net.Models;
        using Cosm.Net.Tx.Msg;

        namespace Cosm.Net.Adapters;

        internal class BankAdapter(IBankModule bankModule) : IBankAdapter
        {
            public async Task<BigInteger> BalanceAsync(string address, string denom,
                Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var balance = await bankModule.BalanceAsync(address, denom, headers, deadline, cancellationToken);
                return BigInteger.Parse(balance.Balance.Amount);
            }

            public async Task<BigInteger> BalanceAsync(string address, string denom, CallOptions options)
            {
                var balance = await bankModule.BalanceAsync(address, denom, options);
                return BigInteger.Parse(balance.Balance.Amount);
            }

            public ITxMessage Send(string fromAddress, string toAddress, string denominator, BigInteger amount)
                => bankModule.Send(fromAddress, toAddress, [new Coin(denominator, amount)]);
                            
            public ITxMessage Send(string fromAddress, string toAddress, params Coin[] amount)
                => bankModule.Send(fromAddress, toAddress, amount);
        }
        """;
}

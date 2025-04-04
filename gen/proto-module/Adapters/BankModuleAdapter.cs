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

        internal class BankAdapater(IBankModule bankModule) : IBankAdapter
        {
            private readonly IBankModule _bankModule = bankModule;

            public async Task<BigInteger> BalanceAsync(string address, string denom,
                Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var balance = await _bankModule.BalanceAsync(address, denom, metadata, deadline, cancellationToken);
                return BigInteger.Parse(balance.Balance.Amount);
            }

            public async Task<BigInteger> BalanceAsync(string address, string denom, CallOptions options)
            {
                var balance = await _bankModule.BalanceAsync(address, denom, options);
                return BigInteger.Parse(balance.Balance.Amount);
            }

            public ITxMessage Send(string fromAddress, string toAddress, string denominator, BigInteger amount)
                => _bankModule.Send(fromAddress, toAddress, [new Coin(denominator, amount)]);
                            
            public ITxMessage Send(string fromAddress, string toAddress, params Coin[] amount)
                => _bankModule.Send(fromAddress, toAddress, amount);
        }
        """;
}

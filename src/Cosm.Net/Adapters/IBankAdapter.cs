using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;
using Grpc.Core;
using System.Numerics;

namespace Cosm.Net.Adapters;
public interface IBankAdapter : IModule
{
    public Task<BigInteger> BalanceAsync(string address, string denom,
        Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<BigInteger> BalanceAsync(string address, string denom, CallOptions options);

    public ITxMessage Send(string fromAddress, string toAddress, string denominator, BigInteger amount);
    public ITxMessage Send(string fromAddress, string toAddress, params Coin[] amount);
}
namespace Cosm.Net.Generators.Proto.Adapters;
public static class TxModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Modules;
        using Cosm.Net.Models;
        using Google.Protobuf;
        using Grpc.Core;
        using System.Numerics;

        namespace Cosm.Net.Adapters;

        internal class TxModuleAdapter(ITxModule txModule) : ITxModuleAdapter
        {
            async Task<TxSubmission> ITxModuleAdapter.BroadcastTxAsync(ByteString txBytes, BroadcastMode mode, Metadata? headers,
                DateTime? deadline, CancellationToken cancellationToken)
            {
                var signMode = mode switch
                {
                    BroadcastMode.Unspecified => Cosmos.Tx.V1Beta1.BroadcastMode.Unspecified,
                    BroadcastMode.Sync => Cosmos.Tx.V1Beta1.BroadcastMode.Sync,
                    BroadcastMode.Async => Cosmos.Tx.V1Beta1.BroadcastMode.Async,
                    _ => throw new InvalidOperationException("Unsupported BroadcastMode")
                };

                var response = await txModule.BroadcastTxAsync(txBytes, signMode, headers, deadline, cancellationToken);
                return new TxSubmission(response.TxResponse.Code, response.TxResponse.Txhash, response.TxResponse.RawLog);
            }
            async Task<TxSimulation> ITxModuleAdapter.SimulateAsync(ByteString txBytes, Metadata? headers,
                DateTime? deadline, CancellationToken cancellationToken)
            {
                var response = await txModule.SimulateAsync(txBytes, headers, deadline, cancellationToken);

                return new TxSimulation(
                    response.GasInfo.GasUsed,
                    [.. response.Result.Events
                        .Select(x => new TxEvent(
                            null, 
                            x.Type, 
                            [.. x.Attributes.Select(y => new TxEventAttribute(y.Key.ToStringUtf8(), y.Value.ToStringUtf8()))]
                        ))
                    ]
                );
            }

            async Task<TxExecution> ITxModuleAdapter.GetTxByHashAsync(string txHash,
                Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
            {
                var tx = await txModule.GetTxAsync(txHash, headers, deadline, cancellationToken);
                var events = tx.TxResponse.Logs.Count < 2
                    ? tx.TxResponse.Events.Select(e =>
                    {
                        var msgIndexRaw = e.Attributes.LastOrDefault(x => x.Key.ToStringUtf8() == "msg_index")?.Value.ToStringUtf8();
                        return new TxEvent(msgIndexRaw is null ? null : int.Parse(msgIndexRaw), e.Type,
                            [.. e.Attributes.Select(a => new TxEventAttribute(a.Key.ToStringUtf8(), a.Value.ToStringUtf8()))]);
                    })
                    : tx.TxResponse.Logs.SelectMany(
                        x => x.Events.Select(
                            e => new TxEvent((int) x.MsgIndex, e.Type, [.. e.Attributes.Select(
                                a => new TxEventAttribute(a.Key, a.Value))])));

                return new TxExecution(
                    tx.TxResponse.Code,
                    txHash,
                    tx.TxResponse.Height,
                    tx.TxResponse.RawLog,
                    tx.Tx.Body.Memo,
                    tx.Tx.AuthInfo.Fee.GasLimit,
                    [.. tx.Tx.AuthInfo.Fee.Amount.Select(x => new Coin(x.Denom, BigInteger.Parse(x.Amount)))],
                    [.. events]
                );
            }
        }
        """;
}

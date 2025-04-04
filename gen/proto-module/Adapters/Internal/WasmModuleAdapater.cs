namespace Cosm.Net.Generators.Proto.Adapters.Internal;
public static class WasmModuleAdapater
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Json;
        using Cosm.Net.Models;
        using Cosm.Net.Modules;
        using Cosm.Net.Signer;
        using Cosm.Net.Tx;
        using Cosm.Net.Tx.Msg;
        using Google.Protobuf;
        using Microsoft.Extensions.DependencyInjection;
        using System.Text.Json.Nodes;

        namespace Cosm.Net.Adapters.Internal;

        internal class WasmModuleAdapter(IWasmModule wasmModule, IChainConfiguration chain, IServiceProvider provider) : IInternalWasmAdapter
        {
            private readonly IWasmModule _wasmModule = wasmModule;
            private readonly IChainConfiguration _chain = chain;
            private readonly ICosmSigner? _signer = provider.GetService<ICosmSigner>();

            public IWasmTxMessage EncodeContractCall(IContract contract, JsonObject requestBody, IEnumerable<Coin> funds, string? txSender)
            {
                if(_signer is null)
                {
                    throw new InvalidOperationException("Transactions not supported in ReadClient");
                }

                var requestJson = requestBody.ToJsonString(CosmWasmJsonUtils.SerializerOptions);
                var msg = new Cosmwasm.Wasm.V1.MsgExecuteContract()
                {
                    Contract = contract.ContractAddress,
                    Msg = ByteString.CopyFrom(System.Text.Encoding.UTF8.GetBytes(requestJson)),
                    Sender = txSender ?? _signer.GetAddress(_chain.Bech32Prefix),
                };

                foreach(var coin in funds)
                {
                    msg.Funds.Add(new Cosmos.Base.V1Beta1.Coin()
                    {
                        Amount = coin.Amount.ToString(),
                        Denom = coin.Denom
                    });
                }

                return new WasmTxMessage<Cosmwasm.Wasm.V1.MsgExecuteContract>(msg, requestJson);
            }

            public async Task<ByteString> SmartContractStateAsync(IContract contract, ByteString queryData, CancellationToken cancellationToken)
            {
                var response = await _wasmModule.SmartContractStateAsync(contract.ContractAddress, queryData, cancellationToken: cancellationToken);
                return response.Data;
            }
        }
        """;
}

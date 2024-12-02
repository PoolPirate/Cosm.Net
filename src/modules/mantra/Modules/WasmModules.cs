using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Json;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Nodes;

namespace Cosm.Net.Modules;
internal partial class WasmModule : IModule<WasmModule, Cosmwasm.Wasm.V1.Query.QueryClient>, IWasmAdapater
{
    private readonly IChainConfiguration _chain;
    private readonly IOfflineSigner? _signer;

    public WasmModule(CallInvoker callInvoker, IChainConfiguration chain, IServiceProvider provider)
    {
        _client = new Cosmwasm.Wasm.V1.Query.QueryClient(callInvoker);
        _chain = chain;
        _signer = provider.GetService<IOfflineSigner>();
    }

    IWasmTxMessage IWasmAdapater.EncodeContractCall(IContract contract, JsonObject requestBody, IEnumerable<Coin> funds, string? txSender)
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

    async Task<ByteString> IWasmAdapater.SmartContractStateAsync(IContract contract, ByteString queryData, CancellationToken cancellationToken)
    {
        var response = await SmartContractStateAsync(contract.ContractAddress, queryData, cancellationToken: cancellationToken);
        return response.Data;
    }
}
using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;
using Grpc.Net.Client;

namespace Cosm.Net.Modules;
internal partial class WasmModule : IModule<WasmModule, Cosmwasm.Wasm.V1.Query.QueryClient>, IWasmAdapater
{
    private readonly IChainConfiguration _chain;
    private readonly IOfflineSigner? _signer;

    public WasmModule(GrpcChannel channel, IChainConfiguration chain, IServiceProvider provider)
    {
        _client = new Cosmwasm.Wasm.V1.Query.QueryClient(channel);
        _chain = chain;
        _signer = provider.GetService<IOfflineSigner>();
    }

    ITxMessage IWasmAdapater.EncodeContractCall(IContract contract, ByteString encodedRequest, IEnumerable<Coin> funds, string? txSender)
    {
        if(_signer is null)
        {
            throw new InvalidOperationException("Transactions not supported in ReadClient");
        }
        var msg = new Cosmwasm.Wasm.V1.MsgExecuteContract()
        {
            Contract = contract.ContractAddress,
            Msg = encodedRequest,
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

        return new TxMessage<Cosmwasm.Wasm.V1.MsgExecuteContract>(msg);
    }

    async Task<ByteString> IWasmAdapater.SmartContractStateAsync(IContract contract, ByteString queryData)
    {
        var response = await SmartContractStateAsync(contract.ContractAddress, queryData, default, default, default);
        return response.Data;
    }
}
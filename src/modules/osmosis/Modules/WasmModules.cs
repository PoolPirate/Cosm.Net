namespace Cosm.Net.Modules;
internal partial class WasmModule : IModule<WasmModule, Cosmwasm.Wasm.V1.Query.QueryClient>, IWasmAdapater
{
    private readonly IOfflineSigner _signer;
    private readonly IChainConfiguration _chain;

    public WasmModule(GrpcChannel channel, IOfflineSigner signer, IChainConfiguration chain)
    {
        _client = new Cosmwasm.Wasm.V1.Query.QueryClient(channel);
        _signer = signer;
        _chain = chain;
    }

    ITxMessage IWasmAdapater.EncodeContractCall(IContract contract, ByteString encodedRequest, IEnumerable<Coin> funds, string? txSender)
    {
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
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;

namespace Cosm.Net.Adapters;
public interface IWasmAdapater : IModule
{
    public Task<ByteString> SmartContractStateAsync(string contractAddress, ByteString queryData);
    public ITxMessage EncodeContractCall(string contractAddress, ByteString encodedRequest, IEnumerable<Coin> funds, string? txSender);
}
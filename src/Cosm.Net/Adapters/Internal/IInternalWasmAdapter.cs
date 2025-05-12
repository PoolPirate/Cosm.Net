﻿using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;
using System.Text.Json.Nodes;

namespace Cosm.Net.Adapters.Internal;
public interface IInternalWasmAdapter : IModule
{
    public Task<ByteString> SmartContractStateAsync(IWasmContract contract, ByteString queryData, CancellationToken cancellationToken = default);
    public IWasmTxMessage EncodeContractCall(IWasmContract contract, JsonObject requestBody, IEnumerable<Coin> funds, string? txSender);
}
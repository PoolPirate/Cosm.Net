using Cosm.Net.Encoding;
using Cosm.Net.Encoding.Json;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Cosm.Net.Wallet;
using Google.Protobuf;
using Grpc.Core;
using System.Text.Json.Nodes;

namespace Cosm.Net.Adapters.Internal;
internal class ComputeModuleAdapter(ICosmSigner? signer, IChainConfiguration chain, SecretEncryptionProvider encryptor, IComputeModule computeModule) : IInternalWasmAdapter
{
    private readonly ICosmSigner? _signer = signer;
    private readonly IChainConfiguration _chain = chain;
    private readonly SecretEncryptionProvider _encryptor = encryptor;
    private readonly IComputeModule _computeModule = computeModule;

    public IWasmTxMessage EncodeContractCall(IContract contract, JsonObject requestBody, IEnumerable<Coin> funds, string? txSender)
    {
        if(_signer is null)
        {
            throw new InvalidOperationException("Transactions not supported in ReadClient");
        }
        if(contract.CodeHash is null)
        {
            throw new InvalidOperationException("Missing CodeHash. Secret Network contracts have to be created with a codeHash set!");
        }

        var requestJson = requestBody.ToJsonString(CosmWasmJsonUtils.SerializerOptions);
        var (encryptedMessage, context, decryptor) = EncryptMessage(
            contract.CodeHash, ByteString.CopyFrom(System.Text.Encoding.UTF8.GetBytes(requestJson)));
        decryptor.Dispose();

        var msg = new Secret.Compute.V1Beta1.MsgExecuteContract()
        {
            Contract = ByteString.CopyFrom(Bech32.DecodeAddress(contract.ContractAddress)),
            Msg = encryptedMessage,
            Sender = ByteString.CopyFrom(Bech32.DecodeAddress(txSender ?? _signer.GetAddress(_chain.Bech32Prefix))),
            CallbackCodeHash = "",
            CallbackSig = ByteString.Empty
        };
        msg.SentFunds.AddRange(funds.Select(x => new Cosmos.Base.V1Beta1.Coin()
        {
            Amount = x.Amount.ToString(),
            Denom = x.Denom
        }));

        return new SecretTxMessage<global::Secret.Compute.V1Beta1.MsgExecuteContract>(msg, requestJson, context);
    }
    public async Task<ByteString> SmartContractStateAsync(IContract contract, ByteString queryData, CancellationToken cancellationToken)
    {
        if(contract.CodeHash is null)
        {
            throw new InvalidOperationException("Missing CodeHash. Secret Network contracts have to be created with a codeHash set!");
        }

        var (encryptedMessage, _, decryptor) = EncryptMessage(contract.CodeHash, queryData);

        try
        {
            var queryResponse = await _computeModule.QuerySecretContractAsync(contract.ContractAddress, encryptedMessage);
            var encodedResponse = decryptor.DecryptMessage(queryResponse.Data.Span);
            return ByteString.CopyFrom(Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(encodedResponse)));
        }
        catch(RpcException ex)
        {
            if(ex.StatusCode != StatusCode.Unknown || !ex.Status.Detail.StartsWith("encrypted: "))
            {
                throw;
            }

            var errorParts = ex.Status.Detail.Split(":");

            if(errorParts.Length != 3)
            {
                throw;
            }

            var encryptedErrorMessage = errorParts[1].Trim();
            var errorMessage = decryptor.DecryptMessage(
                Convert.FromBase64String(encryptedErrorMessage));

            throw new RpcException(ex.Status, ex.Trailers, $"{errorParts[2]}: {System.Text.Encoding.UTF8.GetString(errorMessage)}");
        }
        finally
        {
            decryptor.Dispose();
        }
    }

    private (ByteString Message, SecretEncryptionContext Context, SecretMessageDecryptor Decryptor) EncryptMessage(string codeHash, ByteString message)
    {
        int codeHashBytes = System.Text.Encoding.UTF8.GetByteCount(codeHash);
        Span<byte> buffer = stackalloc byte[codeHashBytes + message.Length];

        System.Text.Encoding.UTF8.GetBytes(codeHash, buffer[..codeHashBytes]);
        message.Span.CopyTo(buffer[codeHashBytes..]);

        var encryptedMessage = _encryptor.EncryptMessage(buffer, out var context, out var decryptor);
        return (ByteString.CopyFrom(encryptedMessage), context, decryptor);
    }
}

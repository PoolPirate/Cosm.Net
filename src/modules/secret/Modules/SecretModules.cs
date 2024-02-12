using Cosm.Net.Adapters;
using Cosm.Net.Encoding;
using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Cosmos.Tx.V1Beta1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Modules;
internal partial class ComputeModule : IModule<ComputeModule, global::Secret.Compute.V1Beta1.Query.QueryClient>, IWasmAdapater
{
    private readonly IOfflineSigner? _signer;
    private readonly IChainConfiguration _chain;
    private readonly SecretMessageEncryptor _encryptor;

    public ComputeModule(GrpcChannel channel, IChainConfiguration chain, SecretMessageEncryptor encryptor, IServiceProvider provider)
    {
        _client = new global::Secret.Compute.V1Beta1.Query.QueryClient(channel);
        _chain = chain;
        _encryptor = encryptor;
        _signer = provider.GetService<IOfflineSigner>();
    }

    ITxMessage IWasmAdapater.EncodeContractCall(IContract contract, ByteString encodedRequest, IEnumerable<Coin> funds, string? txSender)
    {
        if(_signer is null)
        {
            throw new InvalidOperationException("Transactions not supported in ReadClient");
        }
        if(contract.CodeHash is null)
        {
            throw new InvalidOperationException("Missing CodeHash. Secret Network contracts have to be created with a codeHash set!");
        }

        var (encryptedMessage, _) = EncryptMessage(contract.CodeHash, encodedRequest);

        var msg = new global::Secret.Compute.V1Beta1.MsgExecuteContract()
        {
            Contract = ByteString.CopyFrom(Bech32.DecodeAddress(contract.ContractAddress)),
            Msg = ByteString.CopyFrom(encryptedMessage),
            Sender = ByteString.CopyFrom(Bech32.DecodeAddress(txSender ?? _signer.GetAddress(_chain.Bech32Prefix))),
            CallbackCodeHash = "",
            CallbackSig = ByteString.Empty
        };
        msg.SentFunds.AddRange(funds.Select(x => new Cosmos.Base.V1Beta1.Coin()
        {
            Amount = x.Amount.ToString(),
            Denom = x.Denom
        }));

        return new TxMessage<global::Secret.Compute.V1Beta1.MsgExecuteContract>(msg);
    }
    async Task<ByteString> IWasmAdapater.SmartContractStateAsync(IContract contract, ByteString queryData)
    {
        if(contract.CodeHash is null)
        {
            throw new InvalidOperationException("Missing CodeHash. Secret Network contracts have to be created with a codeHash set!");
        }

        var (encryptedMessage, nonce) = EncryptMessage(contract.CodeHash, queryData);

        try
        {
            var queryResponse = await QuerySecretContractAsync(contract.ContractAddress, ByteString.CopyFrom(encryptedMessage));
            var encodedResponse = _encryptor.DecryptMessage(queryResponse.Data.Span, nonce);
            return ByteString.CopyFrom(Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(encodedResponse)));
        }
        catch(RpcException ex)
        {
            if (ex.StatusCode != StatusCode.Unknown || !ex.Status.Detail.StartsWith("encrypted: "))
            {
                throw;
            }

            var errorParts = ex.Status.Detail.Split(":");

            if (errorParts.Length != 3)
            {
                throw;
            }

            var encryptedErrorMessage = errorParts[1].Trim();
            var errorMessage = _encryptor.DecryptMessage(
                Convert.FromBase64String(encryptedErrorMessage), nonce);

            throw new RpcException(ex.Status, ex.Trailers, $"{errorParts[2]}: {System.Text.Encoding.UTF8.GetString(errorMessage)}");
        }
    }

    private (byte[] EncryptedMessage, byte[] Nonce) EncryptMessage(string codeHash, ByteString message)
    {
        var codeHashBytes = System.Text.Encoding.UTF8.GetBytes(codeHash);
        Span<byte> buffer = stackalloc byte[codeHashBytes.Length + message.Length];

        codeHashBytes.CopyTo(buffer);
        message.Span.CopyTo(buffer[codeHashBytes.Length..]);

        var encryptedMessage = _encryptor.EncryptMessage(buffer, out var nonce);
        return (encryptedMessage, nonce);
    }
}
internal partial class EmergencyButtonModule : IModule<EmergencyButtonModule, global::Secret.Emergencybutton.V1Beta1.Query.QueryClient> { }
internal partial class InterTxModule : IModule<InterTxModule, global::Secret.Intertx.V1Beta1.Query.QueryClient> { }
internal partial class RegistrationModule : IModule<RegistrationModule, global::Secret.Registration.V1Beta1.Query.QueryClient> { }

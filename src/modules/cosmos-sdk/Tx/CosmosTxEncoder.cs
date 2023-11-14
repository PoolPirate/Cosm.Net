using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Cosmos.Crypto.Secp256K1;
using Cosmos.Tx.V1Beta1;
using Google.Protobuf;

namespace Cosm.Net.CosmosSdk.Tx;
public class CosmosTxEncoder : ITxEncoder
{
    private readonly IOfflineSigner _signer;
    private readonly ITxChainConfiguration _chainConfig;

    public CosmosTxEncoder(IOfflineSigner signer, ITxChainConfiguration chainConfig)
    {
        _signer = signer;
        _chainConfig = chainConfig;
    }

    public byte[] GetSignSignDoc(ICosmTx tx, ulong accountNumber, ulong sequence, 
        ulong gasWanted, string feeDenom, ulong feeAmount) 
        => new SignDoc()
        {
            AccountNumber = accountNumber,
            AuthInfoBytes = MakeAuthInfo(sequence, gasWanted, feeDenom, feeAmount).ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages).ToByteString(),
            ChainId = _chainConfig.ChainId
        }.ToByteArray();

    public ByteString EncodeTx(ICosmTx tx, ulong sequence)
    {
        var txRaw = new TxRaw()
        {
            AuthInfoBytes = MakeAuthInfo(sequence, 0, _chainConfig.FeeDenom, 0)
            .ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages)
            .ToByteString()
        };

        txRaw.Signatures.Add(ByteString.Empty);

        return txRaw.ToByteString();
    }

    public ByteString EncodeTx(ISignedCosmTx tx)
    {
        var authInfoBytes = MakeAuthInfo(tx.Sequence, tx.GasWanted, tx.FeeDenom, tx.FeeAmount)
            .ToByteString();
        var bodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages)
            .ToByteString();

        var txRaw = new TxRaw()
        {
            AuthInfoBytes = authInfoBytes,
            BodyBytes = bodyBytes
        };

        txRaw.Signatures.Add(tx.Signature);

        return txRaw.ToByteString();
    }

    private AuthInfo MakeAuthInfo(ulong sequence, ulong gasWanted, string feeDenom, ulong feeAmount)
    {
        var authInfo = new AuthInfo()
        {
            Fee = new Fee()
            {
                GasLimit = gasWanted,
            }
        };

        authInfo.Fee.Amount.Add(new Cosmos.Base.V1Beta1.Coin()
        {
            Amount = $"{feeAmount}",
            Denom = feeDenom
        });

        authInfo.SignerInfos.Add(new SignerInfo()
        {
            ModeInfo = new ModeInfo()
            {
                Single = new ModeInfo.Types.Single()
                {
                    Mode = Cosmos.Tx.Signing.V1Beta1.SignMode.Direct
                },
            },
            Sequence = sequence,
            PublicKey = new Google.Protobuf.WellKnownTypes.Any()
            {
                TypeUrl = $"/{PubKey.Descriptor.FullName}",
                Value = new PubKey()
                {
                    Key = ByteString.CopyFrom(_signer.PublicKey)
                }.ToByteString(),
            }
        });

        return authInfo;
    }

    private static TxBody MakeTxBody(string memo, ulong timeoutHeight, IEnumerable<ITxMessage> txMessages)
    {
        var body = new TxBody()
        {
            Memo = memo,
            TimeoutHeight = timeoutHeight
        };

        foreach(var txMessage in txMessages)
        {
            body.Messages.Add(new Google.Protobuf.WellKnownTypes.Any()
            {
                TypeUrl = txMessage.GetTypeUrl(),
                Value = txMessage.ToByteString()
            });
        }

        return body;
    }
}

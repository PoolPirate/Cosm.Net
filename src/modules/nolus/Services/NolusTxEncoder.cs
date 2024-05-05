using Cosm.Net.Models;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;
using Cosmos.Crypto.Secp256K1;
using Cosmos.Tx.V1Beta1;

namespace Cosm.Net.Services;
public class NolusTxEncoder : ITxEncoder
{
    private readonly IOfflineSigner _signer;
    private readonly IChainConfiguration _chainConfig;

    public NolusTxEncoder(IOfflineSigner signer, IChainConfiguration chainConfig)
    {
        _signer = signer;
        _chainConfig = chainConfig;
    }

    public virtual byte[] GetSignSignDoc(ICosmTx tx, GasFeeAmount gasFee, ulong accountNumber, ulong sequence)
        => new SignDoc()
        {
            AccountNumber = accountNumber,
            AuthInfoBytes = MakeAuthInfo(sequence, gasFee.GasWanted, gasFee.FeeDenom, gasFee.FeeAmount).ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages).ToByteString(),
            ChainId = _chainConfig.ChainId
        }.ToByteArray();

    public virtual ByteString EncodeTx(ICosmTx tx, ulong sequence, string feeDenom)
    {
        var txRaw = new TxRaw()
        {
            AuthInfoBytes = MakeAuthInfo(sequence, 5000000, feeDenom, 100000)
            .ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages)
            .ToByteString()
        };

        txRaw.Signatures.Add(ByteString.Empty);

        return txRaw.ToByteString();
    }

    public virtual ByteString EncodeTx(ISignedCosmTx tx)
    {
        var authInfoBytes = MakeAuthInfo(tx.Sequence, tx.GasFee.GasWanted, tx.GasFee.FeeDenom, tx.GasFee.FeeAmount)
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

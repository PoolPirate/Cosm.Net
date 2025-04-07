using Cosm.Net.Models;
using Cosm.Net.Tx;
using Cosm.Net.Tx.Msg;
using Cosmos.Crypto.Secp256K1;
using Cosmos.Tx.V1Beta1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Cosm.Net.Services;
public class NolusTxEncoder(IChainConfiguration chainConfig) : ITxEncoder
{
    private readonly IChainConfiguration _chainConfig = chainConfig;

    public virtual IMessage GetSignSignDoc(ICosmTx tx, ByteString publicKey, ulong gasWanted, IEnumerable<Coin> txFees, ulong accountNumber, ulong sequence)
        => new SignDoc()
        {
            AccountNumber = accountNumber,
            AuthInfoBytes = MakeAuthInfo(publicKey, sequence, gasWanted, txFees).ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages).ToByteString(),
            ChainId = _chainConfig.ChainId
        };

    public virtual ByteString EncodeTx(ICosmTx tx, ByteString publicKey, ulong sequence, string feeDenom)
        => new TxRaw()
        {
            AuthInfoBytes = MakeAuthInfo(publicKey, sequence, 0, [new Coin(feeDenom, 0)]).ToByteString(),
            BodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages).ToByteString(),
            Signatures = { ByteString.Empty }
        }.ToByteString();

    public virtual ByteString EncodeTx(ISignedCosmTx tx)
    {
        var authInfoBytes = MakeAuthInfo(tx.PublicKey, tx.Sequence, tx.GasWanted, tx.TxFees).ToByteString();
        var bodyBytes = MakeTxBody(tx.Memo, tx.TimeoutHeight, tx.Messages).ToByteString();

        return new TxRaw()
        {
            AuthInfoBytes = authInfoBytes,
            BodyBytes = bodyBytes,
            Signatures = { tx.Signature }
        }.ToByteString();
    }

    private static AuthInfo MakeAuthInfo(ByteString publicKey, ulong sequence, ulong gasWanted, IEnumerable<Coin> txFees)
    {
        var authInfo = new AuthInfo()
        {
            Fee = new Fee()
            {
                GasLimit = gasWanted,
            },
            SignerInfos =
            {
                new SignerInfo()
                {
                    ModeInfo = new ModeInfo()
                    {
                        Single = new ModeInfo.Types.Single()
                        {
                            Mode = Cosmos.Tx.Signing.V1Beta1.SignMode.Direct
                        },
                    },
                    Sequence = sequence,
                    PublicKey = Any.Pack(
                        new PubKey() { Key = publicKey },
                        "/"
                    )
                }
            }
        };

        foreach(var feeCoin in txFees)
        {
            authInfo.Fee.Amount.Add(new Cosmos.Base.V1Beta1.Coin()
            {
                Amount = $"{feeCoin.Amount}",
                Denom = feeCoin.Denom
            });
        }

        return authInfo;
    }

    private static TxBody MakeTxBody(string memo, long timeoutHeight, IEnumerable<ITxMessage> txMessages)
    {
        var body = new TxBody()
        {
            Memo = memo,
            TimeoutHeight = (ulong) timeoutHeight,
        };

        foreach(var txMessage in txMessages)
        {
            body.Messages.Add(new Any()
            {
                TypeUrl = txMessage.GetTypeUrl(),
                Value = txMessage.ToByteString()
            });
        }

        return body;
    }
}

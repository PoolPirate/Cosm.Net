//using Cosm.Net.Wallet;
//using Google.Protobuf;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Cosm.Net.Core.Msg;
using Google.Protobuf;

namespace Cosm.Net.Tx;

public class CosmTx : ICosmTx
{
    private readonly ITxMessage[] _messages;

    public CosmTx(ITxMessage[] messages)
    {
        _messages = messages;
    }

    ByteString ICosmTx.Encode() 
        => throw new NotImplementedException();
}

//namespace Cosm.Net.Tx;
//public class CosmTx
//{
//    private static readonly Fee ZeroFee = new Fee()
//    {
//        GasLimit = 10000000
//    };

//    public ulong AccountNumber { get; }
//    public ulong SequenceNumber { get; }
//    public string ChainId { get; }

//    private readonly byte[] InnerPublicKey;
//    public ReadOnlyMemory<byte> PublicKey
//        => InnerPublicKey.AsMemory();

//    public string Memo { get; }
//    public IReadOnlyList<IMessage> Messages { get; }

//    public ulong GasLimit { get; private set; }
//    public string? FeeDenom { get; private set; }
//    public ulong? FeeAmount { get; private set; }

//    private byte[]? InnerSignature;
//    public ReadOnlyMemory<byte>? Signature
//        => InnerSignature?.AsMemory();

//    public CosmTx(ulong accountNumber, ulong sequenceNumber, string chainId, ReadOnlySpan<byte> publicKey,
//        string memo, params IMessage[] messages)
//    {
//        AccountNumber = accountNumber;
//        SequenceNumber = sequenceNumber;
//        ChainId = chainId;
//        InnerPublicKey = publicKey.ToArray();
//        Memo = memo;
//        Messages = messages.ToList();

//        GasLimit = ZeroFee.GasLimit;
//        FeeDenom = null;
//        FeeAmount = null;
//    }

//    public void Sign(IOfflineSigner signer)
//    {
//        var doc = GetSignDoc(SignMode.Direct);
//        InnerSignature = new byte[64];
//        signer.SignMessage(InnerSignature, doc.ToByteArray());
//    }

//    public void ClearSignature()
//    {
//        InnerSignature = null;
//    }

//    public void SetGas(ulong gasLimit, decimal gasCost, string feeDenom)
//    {
//        ClearSignature();
//        FeeDenom = feeDenom;
//        GasLimit = gasLimit;
//        FeeAmount = (ulong) Math.Ceiling(gasCost * gasLimit);
//    }

//    private AuthInfo GetAuthInfo(SignMode signMode)
//    {
//        var fee = new Fee()
//        {
//            GasLimit = GasLimit,
//        };
//        if(FeeDenom is not null && FeeAmount.HasValue)
//        {
//            fee.Amount.Add(new Cosmos.Base.V1Beta1.Coin()
//            {
//                Denom = FeeDenom,
//                Amount = $"{FeeAmount}"
//            });
//        }

//        var authInfo = new AuthInfo()
//        {
//            Fee = fee
//        };
//        authInfo.SignerInfos.Add(new SignerInfo()
//        {
//            ModeInfo = new ModeInfo()
//            {
//                Single = new ModeInfo.Types.Single()
//                {
//                    Mode = signMode
//                }
//            },
//            PublicKey = new Any()
//            {
//                TypeUrl = $"/{PubKey.Descriptor.FullName}",
//                Value = new PubKey()
//                {
//                    Key = ByteString.CopyFrom(PublicKey.ToArray())
//                }.ToByteString()
//            },
//            Sequence = SequenceNumber
//        });

//        return authInfo;
//    }
//    private TxBody GetBody()
//    {
//        var body = new TxBody()
//        {
//            Memo = Memo,
//            TimeoutHeight = 0,
//        };
//        body.Messages.AddRange(Messages.Select(x => new Any()
//        {
//            TypeUrl = $"/{x.Descriptor.FullName}",
//            Value = x.ToByteString()
//        }));
//        return body;
//    }

//    private SignDoc GetSignDoc(SignMode signMode)
//    {
//        return new SignDoc()
//        {
//            AccountNumber = AccountNumber,
//            ChainId = ChainId,
//            AuthInfoBytes = GetAuthInfo(signMode).ToByteString(),
//            BodyBytes = GetBody().ToByteString(),
//        };
//    }

//    public ByteString GetTxBytes()
//    {
//        var tx = GetTx();
//        return tx.ToByteString();
//    }

//    public Tx GetTx()
//    {
//        var tx = new Tx()
//        {
//            AuthInfo = GetAuthInfo(InnerSignature is null ? SignMode.Unspecified : SignMode.Direct),
//            Body = GetBody()
//        };

//        tx.Signatures.Add(
//            InnerSignature is null
//                ? ByteString.Empty
//                : ByteString.CopyFrom(InnerSignature)
//        );

//        return tx;
//    }
//}

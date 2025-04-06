using Google.Protobuf;

namespace Cosm.Net.Tx.Msg;

/// <summary>
/// Represents a Cosmos SDK transaction message. Multiple ITxMessage can be combined to create a transaction.
/// </summary>
/// <typeparam name="TMsg">The internal generated protobuf type of the message.</typeparam>
public interface ITxMessage<TMsg> : ITxMessage
    where TMsg : IMessage, IMessage<TMsg>
{
}

/// <summary>
/// Represents a Cosmos SDK transaction message. Multiple ITxMessage can be combined to create a transaction.
/// </summary>
public interface ITxMessage
{
    /// <summary>
    /// Gets the protobuf typeurl of the tx message.
    /// </summary>
    /// <returns></returns>
    public string GetTypeUrl();

    /// <summary>
    /// Protobuf encodes the transaction.
    /// </summary>
    /// <returns></returns>
    public ByteString ToByteString();
}
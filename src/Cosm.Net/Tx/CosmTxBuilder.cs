using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Tx;
/// <summary>
/// A transaction builder for combining messages and configuring various options for Cosmos SDK transactions.
/// </summary>
public class CosmTxBuilder
{
    private readonly List<ITxMessage> _messages;
    private string _memo = String.Empty;
    private long _timeoutHeight;

    public CosmTxBuilder()
    {
        _messages = [];
    }

    /// <summary>
    /// Adds a message to the transaction.
    /// </summary>
    /// <param name="msg">The message to add</param>
    /// <returns></returns>
    public CosmTxBuilder AddMessage(ITxMessage msg)
    {
        _messages.Add(msg);
        return this;
    }

    /// <summary>
    /// Adds multiple messages to the transaction.
    /// </summary>
    /// <param name="msgs">Enumerable of messages to add</param>
    /// <returns></returns>
    public CosmTxBuilder AddMessages(IEnumerable<ITxMessage> msgs)
    {
        _messages.AddRange(msgs);
        return this;
    }

    /// <summary>
    /// Sets the transaction memo.
    /// </summary>
    /// <param name="memo"></param>
    /// <returns></returns>
    public CosmTxBuilder WithMemo(string memo)
    {
        _memo = memo;
        return this;
    }

    /// <summary>
    /// Sets the transaction timeout height.
    /// </summary>
    /// <param name="timeoutHeight"></param>
    /// <returns></returns>
    public CosmTxBuilder WithTimeoutHeight(long timeoutHeight)
    {
        _timeoutHeight = timeoutHeight;
        return this;
    }

    /// <summary>
    /// Builds the transaction.
    /// </summary>
    /// <returns></returns>
    public ICosmTx Build()
        => new CosmTx(_memo, _timeoutHeight, _messages.ToArray());
}

namespace Cosm.Net.Models;
public class TxEvent
{
    public int? MsgIndex { get; }
    public string Type { get; }
    public IReadOnlyList<TxEventAttribute> Attributes { get; }

    public TxEvent(int? msgIndex, string type, IReadOnlyList<TxEventAttribute> attributes)
    {
        MsgIndex = msgIndex;
        Type = type;
        Attributes = attributes;
    }
}

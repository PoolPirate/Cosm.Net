namespace Cosm.Net.Models;
public class TxEvent
{
    public int? MsgIndex { get; }
    public string Type { get; }
    public IReadOnlyCollection<TxEventAttribute> Attributes { get; }

    public TxEvent(int? msgIndex, string type, IReadOnlyCollection<TxEventAttribute> attributes)
    {
        MsgIndex = msgIndex;
        Type = type;
        Attributes = attributes;
    }
}

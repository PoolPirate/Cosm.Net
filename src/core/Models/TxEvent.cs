namespace Cosm.Net.Models;
public class TxEvent
{
    public string Type { get; }
    public IReadOnlyCollection<TxEventAttribute> Attributes { get; }

    public TxEvent(string type, IReadOnlyCollection<TxEventAttribute> attributes)
    {
        Type = type;
        Attributes = attributes;
    }
}

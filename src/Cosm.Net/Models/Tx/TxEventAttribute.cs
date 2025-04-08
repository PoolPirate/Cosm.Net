namespace Cosm.Net.Models.Tx;
public class TxEventAttribute
{
    public string Key { get; }
    public string Value { get; }

    public TxEventAttribute(string key, string value)
    {
        Key = key;
        Value = value;
    }
}

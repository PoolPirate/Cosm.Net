namespace Cosm.Net.Models.Tx;
public class TxEstimation
{
    public IReadOnlyList<TxEvent> Events { get; }
    public ulong GasWanted { get; }
    public Coin TxFee { get; }

    internal TxEstimation(IReadOnlyList<TxEvent> events, ulong gasWanted, Coin txFee)
    {
        Events = events;
        GasWanted = gasWanted;
        TxFee = txFee;
    }
}

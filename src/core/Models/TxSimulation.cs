namespace Cosm.Net.Models;
public class TxSimulation
{
    public ulong GasUsed { get; }
    public IReadOnlyList<TxEvent> Events { get; }

    public TxSimulation(ulong gasUsed, IReadOnlyList<TxEvent> events)
    {
        GasUsed = gasUsed;
        Events = events;
    }
}

namespace Cosm.Net.Models;
public class TxSimulation
{
    public ulong GasUsed { get; }
    public IReadOnlyCollection<TxEvent> Events { get; }

    public TxSimulation(ulong gasUsed, IReadOnlyCollection<TxEvent> events)
    {
        GasUsed = gasUsed;
        Events = events;
    }
}

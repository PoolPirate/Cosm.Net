namespace Cosm.Net.Models.Tx;
public class TxSimulation(ulong gasUsed, IReadOnlyList<TxEvent> events)
{
    public ulong GasUsed { get; } = gasUsed;
    public IReadOnlyList<TxEvent> Events { get; } = events;
}

namespace Cosm.Net.Models;
public class TxSimulation
{
    public ulong GasUsed { get; }

    public TxSimulation(ulong gasUsed)
    {
        GasUsed = gasUsed;
    }
}

namespace Cosm.Net.Configuration;
internal class GasBufferConfiguration : IGasBufferConfiguration
{
    public double GasMultiplier { get; }
    public ulong GasOffset { get; }

    public GasBufferConfiguration(double gasMultiplier, ulong gasOffset)
    {
        GasMultiplier = gasMultiplier;
        GasOffset = gasOffset;
    }
}
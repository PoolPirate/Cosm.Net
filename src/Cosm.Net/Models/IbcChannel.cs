namespace Cosm.Net.Models;
public record IbcChannel(
    string PortId,
    string ChannelId,
    string CounterpartyPortId,
    string CounterpartyChannelId,
    IReadOnlyList<string> ConnectionHops,
    byte State,
    byte Ordering,
    string Version
);

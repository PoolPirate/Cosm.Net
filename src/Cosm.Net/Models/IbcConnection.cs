namespace Cosm.Net.Models;
public record IbcConnection(
    string ConnectionId,
    string ClientId,
    string CounterpartyConnectionId,
    string CounterpartyClientId,
    ulong DelayPeriod,
    byte State
);
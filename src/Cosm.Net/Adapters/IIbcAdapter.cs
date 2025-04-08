using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface IIbcAdapter : IModule
{
    public Task<Height> GetLatestClientHeightAsync(
        string clientId, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
    );
    public Task<Height> GetLatestClientHeightAsync(string clientId, CallOptions options);

    public Task<ulong> NextSequenceReceiveAsync(
        string portId, string channelId, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
    );
    public Task<ulong> NextSequenceReceiveAsync(
        string portId, string channelId, CallOptions options
    );

    public Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
        string portId, string channelId, IEnumerable<ulong> packetAckSequences,
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
    );
    public Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
        string portId, string channelId, IEnumerable<ulong> packetAckSequences, CallOptions options
    );

    public Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
        string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences,
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
    );
    public Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
        string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences, CallOptions options
    );

    public Task<IbcChannel> ChannelAsync(string portId, string channelId, 
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<IbcChannel> ChannelAsync(string portId, string channelId, CallOptions options);

    public Task<IbcConnection> ConnectionAsync(string connectionId, 
        Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<IbcConnection> ConnectionAsync(string connectionId, CallOptions options);

    public ITxMessage UpdateClient(string clientId, Any clientMessage, string signer);

    public ITxMessage RecvPacket(
        string sourcePort,
        string sourceChannel,
        string destinationPort,
        string destinationChannel,
        ulong sequence,
        ByteString packetData,
        Height timeoutHeight,
        ulong timeoutTimestamp,
        ByteString proof,
        Height proofHeight,
        string signer
    );

    public ITxMessage TimeoutPacket(
        string sourcePort,
        string sourceChannel,
        string destinationPort,
        string destinationChannel,
        ulong sequence,
        ByteString packetData,
        Height timeoutHeight,
        ulong timeoutTimestamp,
        ByteString proofUnreceived,
        Height proofHeight,
        ulong nextSequenceRecv,
        string signer
    );
}

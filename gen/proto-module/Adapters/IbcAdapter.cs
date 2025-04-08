namespace Cosm.Net.Generators.Proto.Adapters;
public static class IbcAdapter
{
    public static string Code(string clientModuleName, string channelModuleName)
        => $$$"""
        #nullable enable

        using Cosm.Net.Models;
        using Cosm.Net.Modules;
        using Cosm.Net.Tx.Msg;
        using Google.Protobuf;
        using Google.Protobuf.WellKnownTypes;
        using Grpc.Core;
        using Ibc.Core.Channel.V1;
        using Ibc.Lightclients.Tendermint.V1;

        namespace Cosm.Net.Adapters;
        internal class IbcAdapter(I{{{clientModuleName}}} ibcClientModule, I{{{channelModuleName}}} ibcChannelModule) : IIbcAdapter
        {
            public async Task<Height> GetLatestClientHeightAsync(
                string clientId, Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            )
            {
                var response = await ibcClientModule.ClientStateAsync(clientId, metadata, deadline, cancellationToken);
                var clientState = response.ClientState.Unpack<ClientState>();
                return new Height((long) clientState.LatestHeight.RevisionNumber, (long) clientState.LatestHeight.RevisionHeight);
            }
            public async Task<Height> GetLatestClientHeightAsync(string clientId, CallOptions options)
            {
                var response = await ibcClientModule.ClientStateAsync(clientId, options);
                var clientState = response.ClientState.Unpack<ClientState>();
                return new Height((long) clientState.LatestHeight.RevisionNumber, (long) clientState.LatestHeight.RevisionHeight);
            }

            public async Task<ulong> NextSequenceReceiveAsync(
                string portId, string channelId, Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.NextSequenceReceiveAsync(portId, channelId, metadata, deadline, cancellationToken)).NextSequenceReceive;
            public async Task<ulong> NextSequenceReceiveAsync(
                string portId, string channelId, CallOptions options
            ) => (await ibcChannelModule.NextSequenceReceiveAsync(portId, channelId, options)).NextSequenceReceive;

            public async Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
                string portId, string channelId, IEnumerable<ulong> packetAckSequences,
                Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.UnreceivedAcksAsync(portId, channelId, packetAckSequences, metadata, deadline, cancellationToken)).Sequences;
            public async Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
                string portId, string channelId, IEnumerable<ulong> packetAckSequences, CallOptions options
            ) => (await ibcChannelModule.UnreceivedAcksAsync(portId, channelId, packetAckSequences, options)).Sequences;
        
            public async Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
                string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences,
                Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.UnreceivedPacketsAsync(portId, channelId, packetCommitmentSequences, metadata, deadline, cancellationToken)).Sequences;
            public async Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
                string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences, CallOptions options
            ) => (await ibcChannelModule.UnreceivedPacketsAsync(portId, channelId, packetCommitmentSequences, options)).Sequences;

            public ITxMessage UpdateClient(string clientId, Any clientMessage, string signer)
                => ibcClientModule.UpdateClient(clientId, clientMessage, signer);

            public ITxMessage RecvPacket(
                string sourcePort,
                string sourceChannel,
                string destinationPort,
                string destinationChannel,
                ulong sequence,
                ByteString packetData,
                Height timeoutHeight,
                DateTimeOffset timeoutTimestamp,
                ByteString proof,
                Height proofHeight,
                string signer
            )
                => ibcChannelModule.RecvPacket(new Packet()
                    {
                        SourcePort = sourcePort,
                        SourceChannel = sourceChannel,
                        DestinationPort = destinationPort,
                        DestinationChannel = destinationChannel,
                        Sequence = sequence,
                        Data = packetData,
                        TimeoutHeight = new Ibc.Core.Client.V1.Height() 
                        {
                            RevisionNumber = (ulong) timeoutHeight.RevisionNumber, 
                            RevisionHeight = (ulong) timeoutHeight.RevisionHeight
                        },
                        TimeoutTimestamp = 1000ul * 1000ul * (ulong) timeoutTimestamp.ToUnixTimeMilliseconds()
                    }, 
                    proof, 
                    proofHeight,
                    signer
                );

                public ITxMessage TimeoutPacket(
                    string sourcePort,
                    string sourceChannel,
                    string destinationPort,
                    string destinationChannel, 
                    ulong sequence,
                    ByteString packetData,
                    Height timeoutHeight,
                    DateTimeOffset timeoutTimestamp,
                    ByteString proofUnreceived,
                    Height proofHeight,
                    ulong nextSequenceRecv,
                    string signer
                )
                    => ibcChannelModule.Timeout(
                        new Packet()
                        {
                            SourcePort = sourcePort,
                            SourceChannel = sourceChannel,
                            DestinationPort = destinationPort,
                            DestinationChannel = destinationChannel,
                            Sequence = sequence,
                            Data = packetData,
                            TimeoutHeight = new Ibc.Core.Client.V1.Height() 
                            {
                                RevisionNumber = (ulong) timeoutHeight.RevisionNumber, 
                                RevisionHeight = (ulong) timeoutHeight.RevisionHeight
                            },
                            TimeoutTimestamp = 1000ul * 1000ul * (ulong) timeoutTimestamp.ToUnixTimeMilliseconds()
                        },
                        proofUnreceived,
                        proofHeight,
                        nextSequenceRecv,
                        signer
                    );
        }
        
        """;
}

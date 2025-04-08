namespace Cosm.Net.Generators.Proto.Adapters;
public static class IbcAdapter
{
    public static string Code(string clientModuleName, string channelModuleName, string transferModuleName, string connectionModuleName)
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
        internal class IbcAdapter(
            I{{{clientModuleName}}} ibcClientModule, 
            I{{{channelModuleName}}} ibcChannelModule, 
            I{{{transferModuleName}}} ibcTransferModule,
            I{{{connectionModuleName}}} ibcConnectionModule
        ) : IIbcAdapter
        {
            public async Task<Height> GetLatestClientHeightAsync(
                string clientId, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            )
            {
                var response = await ibcClientModule.ClientStateAsync(clientId, headers, deadline, cancellationToken);
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
                string portId, string channelId, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.NextSequenceReceiveAsync(portId, channelId, headers, deadline, cancellationToken)).NextSequenceReceive;
            public async Task<ulong> NextSequenceReceiveAsync(
                string portId, string channelId, CallOptions options
            ) => (await ibcChannelModule.NextSequenceReceiveAsync(portId, channelId, options)).NextSequenceReceive;

            public async Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
                string portId, string channelId, IEnumerable<ulong> packetAckSequences,
                Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.UnreceivedAcksAsync(portId, channelId, packetAckSequences, headers, deadline, cancellationToken)).Sequences;
            public async Task<IReadOnlyList<ulong>> UnreceivedAcksAsync(
                string portId, string channelId, IEnumerable<ulong> packetAckSequences, CallOptions options
            ) => (await ibcChannelModule.UnreceivedAcksAsync(portId, channelId, packetAckSequences, options)).Sequences;
        
            public async Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
                string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences,
                Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default
            ) => (await ibcChannelModule.UnreceivedPacketsAsync(portId, channelId, packetCommitmentSequences, headers, deadline, cancellationToken)).Sequences;
            public async Task<IReadOnlyList<ulong>> UnreceivedPacketsAsync(
                string portId, string channelId, IEnumerable<ulong> packetCommitmentSequences, CallOptions options
            ) => (await ibcChannelModule.UnreceivedPacketsAsync(portId, channelId, packetCommitmentSequences, options)).Sequences;

            public async Task<IbcChannel> ChannelAsync(
                string portId, string channelId, Metadata? headers, DateTime? deadline, CancellationToken cancellationToken = default
            )
            {
                var response = await ibcChannelModule.ChannelAsync(portId, channelId, headers, deadline, cancellationToken);
                return new IbcChannel(            
                    portId,
                    channelId,
                    response.Channel.Counterparty.PortId,
                    response.Channel.Counterparty.ChannelId,
                    response.Channel.ConnectionHops,
                    (byte) response.Channel.State,
                    (byte) response.Channel.Ordering,
                    response.Channel.Version
                );
            }
            public async Task<IbcChannel> ChannelAsync(string portId, string channelId, CallOptions options)
            {
                var response = await ibcChannelModule.ChannelAsync(portId, channelId, options);
                return new IbcChannel(
                    portId,
                    channelId,
                    response.Channel.Counterparty.PortId,
                    response.Channel.Counterparty.ChannelId,
                    response.Channel.ConnectionHops,
                    (byte) response.Channel.State,
                    (byte) response.Channel.Ordering,
                    response.Channel.Version
                );
            }

            public async Task<IbcConnection> ConnectionAsync(
                string connectionId, Metadata? headers, DateTime? deadline, CancellationToken cancellationToken = default
            )
            {
                var response = await ibcConnectionModule.ConnectionAsync(connectionId, headers, deadline, cancellationToken);
                return new IbcConnection(
                    connectionId,
                    response.Connection.ClientId,
                    response.Connection.Counterparty.ConnectionId,
                    response.Connection.Counterparty.ClientId,
                    response.Connection.DelayPeriod,
                    (byte) response.Connection.State
                );
            }
            public async Task<IbcConnection> ConnectionAsync(string connectionId, CallOptions options)
            {
                var response = await ibcConnectionModule.ConnectionAsync(connectionId, options);
                return new IbcConnection(
                    connectionId,
                    response.Connection.ClientId,
                    response.Connection.Counterparty.ConnectionId,
                    response.Connection.Counterparty.ClientId,
                    response.Connection.DelayPeriod,
                    (byte) response.Connection.State
                );
            }

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
                ulong timeoutTimestamp,
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
                        TimeoutTimestamp = timeoutTimestamp
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
                    ulong timeoutTimestamp,
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
                            TimeoutTimestamp = timeoutTimestamp
                        },
                        proofUnreceived,
                        proofHeight,
                        nextSequenceRecv,
                        signer
                    );
        }
        
        """;
}

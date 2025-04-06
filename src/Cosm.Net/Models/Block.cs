using Google.Protobuf;

namespace Cosm.Net.Models;
public sealed class Block(
    ulong height, DateTimeOffset timestamp, ByteString proposerAddress, 
    IReadOnlyList<ByteString> txs,
    IMessage header, IMessage lastCommit)
{
    public ulong Height { get; } = height;
    public DateTimeOffset Timestamp { get; } = timestamp;
    public ByteString ProposerAddress { get; } = proposerAddress;

    public IReadOnlyList<ByteString> Txs { get; } = txs;

    public IMessage Header { get; } = header;
    public IMessage LastCommit { get; } = lastCommit;
}

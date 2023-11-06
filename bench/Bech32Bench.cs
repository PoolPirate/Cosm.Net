using BenchmarkDotNet.Attributes;
using Cosm.Net.Encoding;

namespace Cosm.Net.Bench;
[MemoryDiagnoser]
public class Bech32Bench
{
    private readonly string Address = "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56sy";
    private readonly byte[] Buffer = new byte[20];

    [Benchmark]
    public byte[] DecodeAddress()
    {
        _ = Bech32.TryDecodeAddress(Address, Buffer);
        return Buffer;
    }

    [Benchmark]
    public byte[] DecodeAddressNano()
    {
        Bech32Encoder.Decode(Address, out _, out byte[]? data);
        return data!;
    }
}

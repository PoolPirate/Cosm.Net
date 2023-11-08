using BenchmarkDotNet.Attributes;
using Cosm.Net.Encoding;
using Nano.Bech32;

namespace Cosm.Net.Bench;
[MemoryDiagnoser]
public class Bech32Bench
{
    private readonly string Address = "osmo1hku9an4jd2ut8amcrxwwn8yw8fzy0cygls56sy";
    private readonly byte[] AddressData = new byte[20];

    private readonly byte[] Buffer = new byte[500];

    public Bech32Bench()
    {
        _ = Bech32.TryDecodeAddress(Address, AddressData);
    }

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

    [Benchmark]
    public string? EncodeAddress() => Bech32.EncodeAddress("osmo", AddressData);

    [Benchmark]
    public string? EncodeAddressNano() => Bech32Encoder.Encode("osmo", AddressData);
}

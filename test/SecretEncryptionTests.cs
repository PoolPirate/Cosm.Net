using Cosm.Net.Services;

namespace Cosm.Net.Test;
public class SecretEncryptionTests
{
    private static readonly byte[] ConsensosIoPubKey = Convert.FromHexString("EFDFBEE583877E6D12C219695030A5BFB72E0A3ABDC416655AA4A30C95A4446F");
    private static readonly byte[] EncryptionSeed = Convert.FromHexString("3B1C19E173CCB5261F0756A14D4FAEC3FF4A2310B66671D6610E1FC639773AC0");
    private static readonly byte[] MessageInput = Convert.FromHexString("E25D07ABB0F0F30F17839B2CA99EBED0818FC8D4155AE0D56B4FA6FFED9E54D8");
    private static readonly byte[] EncryptedOutput = Convert.FromHexString("11C4B3B75399626F669185308DA3B793F9DD24F0EA742D39C4A2BC2D9A6BABA0054819125682F00D042AF36B9F4C6055ED97E7F8CC398C63E8363AE0D67F5C504EDB3826B31EB7110B37DCFE27B2E814F1A024250B4A4C1DF25B30BFEB1BADA665AFF09FC1278B2FED2C1E5A7A29B097");

    private readonly SecretEncryptionProvider _encryptionProvider;

    public SecretEncryptionTests()
    {
        _encryptionProvider = new SecretEncryptionProvider(ConsensosIoPubKey, EncryptionSeed);
    }

    [Fact]
    public void Should_Encrypt_Known_Output()
    {
        var nonce = Convert.FromHexString("11C4B3B75399626F669185308DA3B793F9DD24F0EA742D39C4A2BC2D9A6BABA0");
        var encryptedMessage = _encryptionProvider.Encrypt(MessageInput, nonce, out _, out var decryptor);
        decryptor.Dispose();

        Assert.True(EncryptedOutput.SequenceEqual(encryptedMessage));
    }

    private static readonly byte[] EncryptedInput = Convert.FromHexString("AFDAFC21FF3103B2F9779B15EA16773AE68ED57CB1D361D22DAD695A2D02B8E30A8AA05C416D50EB98F650794B2B39BECA237C9E06A2A06AF2FB0EBF0AA13A9DB27B0DB3886A6EAA73766C43115F8678179EE593CE4183B53E39E2131220E8DF3B8FFBBF1A496BB5B1ED8640D31A2ABD5C8BE0A582C7D790C7BAB7C0588F5964B95AD9BF1905C566FF730EC2579E8FFEC6FA9E1CBEA483F80F528AB1F32F6EB1AE2C836B06B665A60B40CB789775826B0A4DF03D916D1BDA4FBC56EC8F066A8D92D559E4A98D45B6CB2EE8EC7E6AE814DBC664D1FBA0FCBE5C22D4690DC3FF456CCA4FBA0AD4C9DC034634CE357E8825B6638B944D10181F0D29D4939F6C4966F227D1A2F3930DE024061F799B96783D");
    private static readonly byte[] DecryptedOutput = System.Text.Encoding.UTF8.GetBytes("""
        {"get_config":{"admin_auth":{"address":"secret1hcz23784w6znz3cmqml7ha8g4x6s7qq9v93mtl","code_hash":"6666d046c049b04197326e6386b3e65dbe5dd9ae24266c62b333876ce57adaa8"},"airdrop_address":null}}
        """);

    [Fact]
    public void Should_Decrypt_Known_Output()
    {
        byte[] nonce = Convert.FromHexString("F8D3A8A9BAF733DA64CA1BEA7E6597AC0766011F29FB45F607A4348F185E6828");
        using var decryptor = new SecretMessageDecryptor(_encryptionProvider.CalculateTxEncryptionKey(nonce));

        var decryptedMessage = decryptor.DecryptMessage(EncryptedInput);
        var plainMessage = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(decryptedMessage));
        Assert.True(DecryptedOutput.SequenceEqual(plainMessage));
    }
}

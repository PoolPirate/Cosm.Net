﻿namespace Cosm.Net.Tx;
internal class ChainInfo
{
    public string Bech32Prefix { get; }

    public ChainInfo(string bech32Prefix)
    {
        Bech32Prefix = bech32Prefix;
    }
}

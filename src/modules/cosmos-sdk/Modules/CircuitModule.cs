﻿using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Circuit.V1;

namespace Cosm.Net.CosmosSdk;
internal partial class CircuitModule : IModule<CircuitModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public CircuitModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}

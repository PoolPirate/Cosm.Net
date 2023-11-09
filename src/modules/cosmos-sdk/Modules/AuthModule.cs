﻿using Cosm.Net.Base;
using Cosmos.Auth.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class AuthModule : IModule<AuthModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private AuthModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AuthModule IModule<AuthModule>.FromGrpcChannel(GrpcChannel channel)
        => new AuthModule(channel);
}
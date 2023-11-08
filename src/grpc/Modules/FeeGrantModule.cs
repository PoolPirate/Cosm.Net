using Cosmos.Feegrant.V1Beta1;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosm.Net.Client.Modules;
public partial class FeeGrantModule : ICosmModule<Query.QueryClient>, IModule<FeeGrantModule>
{
    private readonly Query.QueryClient Service;

    private FeeGrantModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static FeeGrantModule IModule<FeeGrantModule>.FromGrpcChannel(GrpcChannel channel)
        => new FeeGrantModule(channel);
}

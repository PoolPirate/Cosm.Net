﻿using Grpc.Core;

namespace Cosm.Net.Modules;
public interface IModule<TModule, TService> : ICosmModule<TService>, IModule
        where TService : ClientBase
{
}

public interface IModule
{
}
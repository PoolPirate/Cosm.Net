﻿using Cosm.Net.Client.Internal;
using Cosm.Net.Modules;

namespace Cosm.Net.Client;
public interface ICosmClient
{
    public Task InitializeAsync();

    public TModule Module<TModule>() where TModule : IModule;
    public IInternalCosmClient AsInternal();
}

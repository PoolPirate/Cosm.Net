namespace Cosm.Net.Base;
public interface ICosmClientBuilder<TCosmClientBuilder>
    where TCosmClientBuilder : class
{
    public TCosmClientBuilder RegisterModule<TModule>()
        where TModule : class, IModule<TModule>;
}

using System.Reflection;

namespace Cosm.Net.Services;
public interface IContractFactory
{
    public TContract Create<TContract>(string address, string? codeHash);
    public void AddContractTypesFromAssembly(Assembly assembly);
}

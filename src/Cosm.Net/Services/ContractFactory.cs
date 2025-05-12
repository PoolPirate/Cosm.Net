using Cosm.Net.Adapters.Internal;
using Cosm.Net.Models;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cosm.Net.Services;
internal class ContractFactory(IInternalWasmAdapter wasmAdapter) : IWasmContractFactory
{
    private readonly Lock _lock = new Lock();
    private readonly IInternalWasmAdapter _wasmAdapter = wasmAdapter;
    private readonly Dictionary<Type, Func<string, string?, IWasmContract>> _factoryDelegates = [];

    public TContract Create<TContract>(string address, string? codeHash)
    {
        if(!_factoryDelegates.TryGetValue(typeof(TContract), out var factoryDelegate))
        {
            factoryDelegate = GetContractFactoryDelegate(typeof(TContract));
            lock(_lock)
            {
                _factoryDelegates.TryAdd(typeof(TContract), factoryDelegate);
            }
        }

        return (TContract) factoryDelegate(address, codeHash);
    }

    public void AddContractTypesFromAssembly(Assembly assembly)
    {
        var factoryDelegates = assembly.GetTypes()
            .Where(x => x.GetInterface(nameof(IWasmContract)) is not null)
            .Where(x => x.IsInterface)
            .Select(x => (x, GetContractFactoryDelegate(x)));

        lock(_lock)
        {
            foreach(var (interfaceType, factoryDelegate) in factoryDelegates)
            {
                _factoryDelegates.TryAdd(interfaceType, factoryDelegate);
            }
        }
    }

    private Func<string, string?, IWasmContract> GetContractFactoryDelegate(Type contractInterfaceType)
    {
        var assembly = contractInterfaceType.Assembly;
        var contractType = assembly.GetTypes()
            .Where(x => x.Name == $"{contractInterfaceType.Name}_Generated_Implementation")
            .SingleOrDefault() ?? throw new NotSupportedException($"Could not find implementation for contract interface {contractInterfaceType}");

        if(RuntimeFeature.IsDynamicCodeCompiled)
        {
            var ctor = contractType.GetConstructor([typeof(IInternalWasmAdapter), typeof(string), typeof(string)])
                ?? throw new NotSupportedException("Constructor not found.");

            var contractAddressParam = Expression.Parameter(typeof(string), "address");
            var codeHashParam = Expression.Parameter(typeof(string), "codeHash");

            var newExpr = Expression.New(
                ctor,
                Expression.Constant(_wasmAdapter),
                contractAddressParam,
                codeHashParam
            );

            return Expression.Lambda<Func<string, string?, IWasmContract>>(newExpr, contractAddressParam, codeHashParam).Compile();
        }
        else
        {
            return (contractAddress, codeHash) => (IWasmContract) (Activator.CreateInstance(contractType, _wasmAdapter, contractAddress, codeHash)
                ?? throw new NotSupportedException());
        }
    }
}

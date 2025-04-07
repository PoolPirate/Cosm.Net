using Cosm.Net.Modules;
using Cosm.Net.Tx.Msg;

namespace Cosm.Net.Adapters;
public interface IAuthzAdapter : IModule
{
    public ITxMessage Exec(string grantee, params IEnumerable<ITxMessage> messages);
}

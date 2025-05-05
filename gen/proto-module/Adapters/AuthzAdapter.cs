namespace Cosm.Net.Generators.Proto.Adapters;
public static class AuthzAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Tx.Msg;
        using Google.Protobuf.WellKnownTypes;
        using Cosm.Net.Modules;

        namespace Cosm.Net.Adapters;

        internal class AuthzAdapter(IAuthzModule authzModule) : IAuthzAdapter
        {
            public ITxMessage Exec(string grantee, params IEnumerable<ITxMessage> messages)
                => authzModule.Exec(
                    grantee,
                    messages.Select(x => new Any()
                    {
                        TypeUrl = x.GetTypeUrl(),
                        Value = x.ToByteString()
                    })
                );
        }
        """;
}

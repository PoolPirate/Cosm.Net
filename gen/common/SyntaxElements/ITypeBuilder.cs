namespace Cosm.Net.Generators.Common.SyntaxElements;
public interface ITypeBuilder : ISyntaxBuilder
{
    public string TypeName { get; }

    public string Build();
    public SyntaxId GetContentId();
}

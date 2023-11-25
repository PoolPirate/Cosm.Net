namespace Cosm.Net.Generators.Common.SyntaxElements;
public interface ITypeBuilder : ISyntaxBuilder
{
    public string Build();
    public SyntaxId GetContentId();
}

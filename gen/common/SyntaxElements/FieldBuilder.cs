namespace Cosm.Net.Generators.Common.SyntaxElements;
public enum FieldVisibility
{
    Public,
    Private,
    Internal
}
public class FieldBuilder
{
    public string Type { get; }
    public string Name { get; }

    private FieldVisibility _visibility = FieldVisibility.Private;
    private bool _isReadonly = true;

    public FieldBuilder(string type, string name)
    {
        Type = type;
        Name = name;
    }

    public FieldBuilder WithVisibility(FieldVisibility visibility)
    {
        _visibility = visibility;
        return this;
    }

    public FieldBuilder WithIsReadonly(bool isReadonly)
    {
        _isReadonly = isReadonly;
        return this;
    }

    public string Build()
        => $$"""
            {{_visibility.ToString().ToLower()}} {{(_isReadonly ? "readonly" : "")}} {{Type}} {{Name}};
            """;
}

namespace Cosm.Net.Generators;
public static class NameUtils
{
    public static string Uncapitalize(string name) 
        => name[0].ToString().ToLower() + name.Substring(1);
}

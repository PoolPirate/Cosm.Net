using Cosm.Net.Generators.Common.SyntaxElements;
using Cosm.Net.Generators.Common.Util;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Cosm.Net.Generators.CosmWasm;

public static class CosmWasmGenerator
{
    public static async Task<string> GenerateForFileAsync(string filePath)
    {
        string text = await File.ReadAllTextAsync(filePath);
        var contractSchema = JsonSerializer.Deserialize<ContractSchema>(text)!;

        string code = await contractSchema.GenerateCSharpCodeFileAsync();
        return code;
    }
}

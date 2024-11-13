using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Cosm.Net.Generators.Proto;

[Generator]
public class ModuleQueryClientGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var moduleClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                IsModuleClassCandidate, 
                (ctx, _) => ctx.SemanticModel.GetDeclaredSymbol((ClassDeclarationSyntax) ctx.Node)
            )
            .Where(moduleSymbol => 
                moduleSymbol is not null && 
                moduleSymbol.AllInterfaces.Any(static x => x.Name == "ICosmModule")
            )
            .Select((x, _) => x!);

        var msgClassesProvider = context.SyntaxProvider
            .CreateSyntaxProvider(IsMsgClassCandidate, TransformToMsgClassInfo)
            .Where(msgInfo => msgInfo.HasValue)
            .Select((x, _) => x!.Value)
            .Collect()
            .Select((msgClasses, _) => MatchMsgAndResponsePairs(msgClasses));

        var compilationAndClasses = context.CompilationProvider
            .Combine(moduleClassesProvider.Collect())
            .Combine(msgClassesProvider);

        context.RegisterSourceOutput(compilationAndClasses, (spc, combined) =>
        {
            var ((compilation, moduleClasses), matchedMsgClasses) = combined;
            GenerateSource(spc, compilation, moduleClasses, matchedMsgClasses);
        });
    }

    private static bool IsModuleClassCandidate(SyntaxNode node, CancellationToken _)
        => node is ClassDeclarationSyntax classDecl &&
            classDecl.BaseList is not null &&
            classDecl.Modifiers.Any(SyntaxKind.PartialKeyword);

    private static bool IsMsgClassCandidate(SyntaxNode node, CancellationToken _) 
        => node is ClassDeclarationSyntax classDecl &&
               classDecl.Identifier.Text.StartsWith("Msg");

    private static (string Name, INamedTypeSymbol Symbol)? TransformToMsgClassInfo(GeneratorSyntaxContext context, CancellationToken _)
    {
        var classDeclaration = (ClassDeclarationSyntax) context.Node;

        if(context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }
        //
        return (classSymbol.Name, classSymbol);
    }

    private static ImmutableArray<INamedTypeSymbol> MatchMsgAndResponsePairs(ImmutableArray<(string Name, INamedTypeSymbol Symbol)> msgClasses)
    {
        var msgTypeCandidates = new List<INamedTypeSymbol>();
        var msgResponseCandidates = new List<INamedTypeSymbol>();
        var matchedMsgTypes = new List<INamedTypeSymbol>();

        foreach(var (name, symbol) in msgClasses)
        {
            if(IsMsgCandidate(name))
            {
                var matchingResponse = msgResponseCandidates
                    .Find(response => AreMatchingCandidates(name, response));
                if(matchingResponse != null)
                {
                    msgResponseCandidates.Remove(matchingResponse);
                    matchedMsgTypes.Add(symbol); 
                }
                else
                {
                    msgTypeCandidates.Add(symbol);
                }
            }
            else if(IsMsgResponseCandidate(name))
            {
                var matchingMsg = msgTypeCandidates
                    .Find(msg => AreMatchingCandidates(msg, name));
                if(matchingMsg != null)
                {
                    msgTypeCandidates.Remove(matchingMsg);
                    matchedMsgTypes.Add(matchingMsg);
                }
                else
                {
                    msgResponseCandidates.Add(symbol);
                }
            }
        }

        return [.. matchedMsgTypes];
    }

    private static bool IsMsgCandidate(string name)
        => name.StartsWith("Msg") && !name.EndsWith("Response");

    private static bool IsMsgResponseCandidate(string name)
        => name.StartsWith("Msg") && name.EndsWith("Response");

    private static bool AreMatchingCandidates(INamedTypeSymbol msgCandidate, string msgResponseName)
        => msgResponseName == msgCandidate.Name + "Response";

    private static bool AreMatchingCandidates(string msgName, INamedTypeSymbol msgResponseCandidate)
        => msgResponseCandidate.Name == msgName + "Response";

    private static void GenerateSource(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<INamedTypeSymbol> moduleClasses,
        ImmutableArray<INamedTypeSymbol> msgClasses)
    {
        foreach(var moduleSymbol in moduleClasses)
        {
            if(moduleSymbol.DeclaredAccessibility != Accessibility.Internal)
            {
                ReportDiagnostic(context, GeneratorDiagnostics.ModuleClassShouldBeInternal, moduleSymbol);
            }

            var queryClientType = QueryModuleProcessor.GetQueryClientType(moduleSymbol);
            var msgTypeSymbols = msgClasses
                .Where(msgTypeSymbol => msgTypeSymbol.ContainingNamespace.Equals(queryClientType.ContainingNamespace, SymbolEqualityComparer.Default))
                .ToImmutableArray();

            var moduleCode = QueryModuleProcessor.GetQueryModuleGeneratedCode(moduleSymbol, queryClientType, msgTypeSymbols);

            context.AddSource($"{moduleSymbol.Name}.generated.cs", SourceText.From(moduleCode, Encoding.UTF8));
        }
    }

    private static void ReportDiagnostic(SourceProductionContext context, DiagnosticDescriptor descriptor, ISymbol symbol, Exception? ex = null)
    {
        var diagnostic = Diagnostic.Create(descriptor, symbol.Locations.FirstOrDefault(), ex?.Message ?? string.Empty);
        context.ReportDiagnostic(diagnostic);
    }
}

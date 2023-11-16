using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Cosm.Net.Generators.Common.SourceGeneratorKit;
public class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<IMethodSymbol> Methods { get; } = [];
    public List<IFieldSymbol> Fields { get; } = [];
    public List<IPropertySymbol> Properties { get; } = [];
    public List<INamedTypeSymbol> Classes { get; } = [];

    public virtual bool CollectMethodSymbol { get; } = false;
    public virtual bool CollectFieldSymbol { get; } = false;
    public virtual bool CollectPropertySymbol { get; } = false;
    public virtual bool CollectClassSymbol { get; } = false;

    /// <inheritdoc/>
    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        switch(context.Node)
        {
            case MethodDeclarationSyntax methodDeclarationSyntax:
                OnVisitMethodDeclaration(methodDeclarationSyntax, context.SemanticModel);
                break;
            case PropertyDeclarationSyntax propertyDeclarationSyntax:
                OnVisitPropertyDeclaration(propertyDeclarationSyntax, context.SemanticModel);
                break;
            case FieldDeclarationSyntax fieldDeclarationSyntax:
                OnVisitFieldDeclaration(fieldDeclarationSyntax, context.SemanticModel);
                break;
            case ClassDeclarationSyntax classDeclarationSyntax:
                OnVisitClassDeclaration(classDeclarationSyntax, context.SemanticModel);
                break;
        }
    }

    protected virtual void OnVisitMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax, SemanticModel model)
    {
        if(!CollectMethodSymbol)
        {
            return;
        }

        if(!ShouldCollectMethodDeclaration(methodDeclarationSyntax))
        {
            return;
        }

        if(model.GetDeclaredSymbol(methodDeclarationSyntax) is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if(!ShouldCollectMethodSymbol(methodSymbol))
        {
            return;
        }

        Methods.Add(methodSymbol);
    }

    protected virtual bool ShouldCollectMethodDeclaration(MethodDeclarationSyntax methodDeclarationSyntax)
        => true;

    protected virtual bool ShouldCollectMethodSymbol(IMethodSymbol methodSymbol)
        => true;

    protected virtual void OnVisitFieldDeclaration(FieldDeclarationSyntax fieldDeclarationSyntax, SemanticModel model)
    {
        if(!CollectFieldSymbol)
        {
            return;
        }

        if(!ShouldCollectFieldDeclaration(fieldDeclarationSyntax))
        {
            return;
        }

        if(model.GetDeclaredSymbol(fieldDeclarationSyntax) is not IFieldSymbol fieldSymbol)
        {
            return;
        }

        if(!ShouldCollectFieldSymbol(fieldSymbol))
        {
            return;
        }

        Fields.Add(fieldSymbol);
    }

    protected virtual bool ShouldCollectFieldDeclaration(FieldDeclarationSyntax fieldDeclarationSyntax)
        => true;

    protected virtual bool ShouldCollectFieldSymbol(IFieldSymbol fieldSymbol)
        => true;

    protected virtual void OnVisitPropertyDeclaration(PropertyDeclarationSyntax propertyDeclarationSyntax, SemanticModel model)
    {
        if(!CollectPropertySymbol)
        {
            return;
        }

        if(!ShouldCollectPropertyDeclaration(propertyDeclarationSyntax))
        {
            return;
        }

        if(model.GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol propertySymbol)
        {
            return;
        }

        if(!ShouldCollectPropertySymbol(propertySymbol))
        {
            return;
        }

        Properties.Add(propertySymbol);
    }

    protected virtual bool ShouldCollectPropertyDeclaration(PropertyDeclarationSyntax propertyDeclarationSyntax)
        => true;

    protected virtual bool ShouldCollectPropertySymbol(IPropertySymbol propertySymbol)
        => true;

    protected virtual void OnVisitClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax, SemanticModel model)
    {
        if(!CollectClassSymbol)
        {
            return;
        }

        if(!ShouldCollectClassDeclaration(classDeclarationSyntax))
        {
            return;
        }

        if(model.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
        {
            return;
        }

        if(!ShouldCollectClassSymbol(classSymbol))
        {
            return;
        }

        Classes.Add(classSymbol);
    }

    protected virtual bool ShouldCollectClassDeclaration(ClassDeclarationSyntax classDeclarationSyntax)
        => true;

    protected virtual bool ShouldCollectClassSymbol(INamedTypeSymbol classSymbol)
        => true;
}
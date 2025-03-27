namespace PdArchEcsCorePersistenceGenerator.State;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistence;
using PdArchEcsCorePersistenceGenerator.Utils;

[Generator]
public class EntityStateGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InterfaceDeclarationSyntax classDecl &&
                                        classDecl.AttributeLists.Count > 0,
                transform: (context, _) => (context.Node as InterfaceDeclarationSyntax, context.SemanticModel))
            .Where(pair => Utilities.HasAttribute(nameof(EntityStateAttribute), pair.Item1, pair.Item2))
            .Collect();

        context.RegisterSourceOutput(structDeclarations, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context,
        ImmutableArray<(InterfaceDeclarationSyntax, SemanticModel)> structs)
    {
        foreach (var (ctx, semanticModel) in structs)
        {
            var entityStateInterfaceSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");
            var entityName = entityStateInterfaceSymbol.Name[1..].Replace("State", "");
            var namespaces = new HashSet<string> { };
            var entityStateClassCode = EntityStateTemplate.Generate(ctx, semanticModel, ns => namespaces.Add(ns));

            var code = $$"""
namespace {{entityStateInterfaceSymbol.ContainingNamespace.ToDisplayString()}};
using PdArchEcsCorePersistence;
using ByteFormatter;
{{entityStateClassCode}}

""";

            var formattedCode = code.FormatCode();
            context.AddSource($"EcsCodeGen.Persistence.States/{entityName}Persistence.g.cs", formattedCode);
        }
    }

}

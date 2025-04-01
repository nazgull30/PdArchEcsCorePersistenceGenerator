namespace PdArchEcsCorePersistenceGenerator.EntityState;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCore.Entities;
using PdArchEcsCorePersistence;
using PdArchEcsCorePersistenceGenerator.Utils;

[Generator]
public class EntityGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider
               .CreateSyntaxProvider(
                   predicate: (node, _) => node is StructDeclarationSyntax,
                   transform: (context, _) => (context.Node as StructDeclarationSyntax, context.SemanticModel)
               )
               .Where(pair => Utilities.HasAttribute(nameof(ComponentAttribute), pair.Item1, pair.Item2) && Utilities.HasAttribute(nameof(PersistentAttribute), pair.Item1, pair.Item2))
               .Collect();

        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InterfaceDeclarationSyntax,
                transform: (context, _) => (context.Node as InterfaceDeclarationSyntax, context.SemanticModel)
            )
            .Where(pair => Utilities.HasAttribute(nameof(EntityStateAttribute), pair.Item1, pair.Item2))
            .Collect();

        var combinedProvider = structDeclarations.Combine(interfaceDeclarations);

        context.RegisterSourceOutput(combinedProvider, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context,
    (ImmutableArray<(StructDeclarationSyntax structDeclaration, SemanticModel SemanticModel)> structs,
    ImmutableArray<(InterfaceDeclarationSyntax interfaceDeclaration, SemanticModel SemanticModel)> interfaces) tuple)
    {
        var components = new List<INamedTypeSymbol>();
        foreach (var (componentDeclaration, semanticModel) in tuple.structs)
        {
            var properties = PropertyUtils.GetProperties(componentDeclaration, semanticModel);
            if (properties.Count != 1)
                continue;
            var componentSymbol = semanticModel.GetDeclaredSymbol(componentDeclaration) ?? throw new ArgumentException("structSymbol is null");
            components.Add(componentSymbol as INamedTypeSymbol);
        }

        foreach (var (stateDeclaration, semanticModel) in tuple.interfaces)
        {
            var entityStateInterfaceSymbol = semanticModel.GetDeclaredSymbol(stateDeclaration) ?? throw new ArgumentException("stateSymbol is null");
            var entityName = entityStateInterfaceSymbol.Name[1..].Replace("State", "");
            var namespaces = new HashSet<string> { };

            var entityStateClassCode = EntityStateTemplate.Generate(stateDeclaration, semanticModel, components, ns => namespaces.Add(ns));
            var entityPropertyAccessClassCode = EntityPropertyAccessTemplate.Generate(stateDeclaration, semanticModel, components, ns => namespaces.Add(ns));


            var namespacesBuilder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                namespacesBuilder.AppendLine($"using {ns};");
            }

            var code = $$"""
                namespace PdArchEcsCorePersistence;

                {{namespacesBuilder}}
                using PdArchEcsCorePersistence.Components;
                using {{entityStateInterfaceSymbol.ContainingNamespace.ToDisplayString()}};
                using ByteFormatter;
                using VContainer;

                {{entityStateClassCode}}
                {{entityPropertyAccessClassCode}}

            """;

            var formattedCode = code.FormatCode();
            context.AddSource($"EcsCodeGen.Persistence.States/{entityName}Persistence.g.cs", formattedCode);
        }

    }
}

namespace PdArchEcsCorePersistenceGenerator.Bytes;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCore.Entities;
using PdArchEcsCorePersistenceGenerator.Components;
using PdArchEcsCorePersistenceGenerator.Utils;

[Generator]
public class ComponentPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is StructDeclarationSyntax classDecl &&
                                        classDecl.AttributeLists.Count > 0,
                transform: (context, _) => (context.Node as StructDeclarationSyntax, context.SemanticModel))
            .Where(pair => Utilities.HasAttribute(nameof(ComponentAttribute), pair.Item1, pair.Item2))
            .Collect();

        context.RegisterSourceOutput(structDeclarations, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context,
       ImmutableArray<(StructDeclarationSyntax, SemanticModel)> structs)
    {
        foreach (var (ctx, semanticModel) in structs)
        {
            var namespaces = new HashSet<string> { };
            var structSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");
            var propertyInterface = ComponentPropertyTemplate.Generate(ctx, semanticModel, ns => namespaces.Add(ns));
            if (string.IsNullOrEmpty(propertyInterface))
                continue;

            var propertyAccessClass = ComponentPropertyAccessTemplate.Generate(ctx, semanticModel, ns => namespaces.Add(ns));
            if (string.IsNullOrEmpty(propertyAccessClass))
                continue;

            var namespacesBuilder = new StringBuilder();
            foreach (var ns in namespaces)
            {
                namespacesBuilder.AppendLine($"using {ns};");
            }

            var code = $$"""
namespace PdArchEcsCorePersistence.Components;

using Arch.Core;
using Arch.Core.Extensions;
using PdArchEcsCorePersistence;
using PdArchEcsCore.Components;
{{namespacesBuilder}}

{{propertyInterface}}

{{propertyAccessClass}}

""";
            var formattedCode = code.FormatCode();
            context.AddSource($"EcsCodeGen.Persistence/{structSymbol.Name}PersistenceProperties.g.cs", formattedCode);

        }
    }
}

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
public class AccessInstallManagerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var accessDeclarations = context.SyntaxProvider
        .CreateSyntaxProvider(
            predicate: (node, _) => node is ClassDeclarationSyntax,
            transform: (context, _) => (context.Node as ClassDeclarationSyntax, context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken: _) as ITypeSymbol, context.SemanticModel)
        )
        .Where(pair => Utilities.InheritsFrom(pair.Item2, "AMultipleEntityAccess") || Utilities.InheritsFrom(pair.Item2, "ASingleEntityAccess"))
        .Collect();

        context.RegisterSourceOutput(accessDeclarations, GenerateCode);
    }

    private void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<(ClassDeclarationSyntax classDeclaration, ITypeSymbol typeSymbol, SemanticModel SemanticModel)> interfaces)
    {
        var accessSymbols = interfaces.Select(i => i.typeSymbol).ToList();

        var namespaces = new HashSet<string> { };

        var bindings = new StringBuilder();

        foreach (var accessSymbol in accessSymbols)
        {
            bindings.AppendLine($"builder.Register<{accessSymbol.Name}>(Lifetime.Singleton).AsImplementedInterfaces();");
            bindings.AppendLine($"builder.Register<{accessSymbol.Name.Replace("Access", "")}PropertyAccess>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();");
            namespaces.Add(accessSymbol.ContainingNamespace.ToDisplayString());
        }

        var namespacesBuilder = new StringBuilder();
        foreach (var ns in namespaces)
        {
            namespacesBuilder.AppendLine($"using {ns};");
        }

        var code = $$"""

namespace PdArchEcsCorePersistence;

using VContainer;
using VContainer.Godot;
using VContainer.Pools;
{{namespacesBuilder}}

public static class AccessInstallManager
{
    public static void Install(IContainerBuilder builder)
    {
        builder.Register<GeneralStateAccess>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<GeneralStateByteConverter>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.RegisterPool<GeneralState, GeneralStatePool, IPool<GeneralState>>();

        {{bindings}}
    }
}


""";

        var formattedCode = code.FormatCode();
        context.AddSource($"EcsCodeGen.Persistence/AccessInstallManager.g.cs", formattedCode);
    }
}

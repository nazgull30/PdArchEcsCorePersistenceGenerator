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
public class AccessInstallerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InterfaceDeclarationSyntax,
                transform: (context, _) => (context.Node as InterfaceDeclarationSyntax, context.SemanticModel)
            )
            .Where(pair => Utilities.HasAttribute(nameof(EntityStateAttribute), pair.Item1, pair.Item2))
            .Collect();

        context.RegisterSourceOutput(interfaceDeclarations, GenerateCode);
    }

    private void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<(InterfaceDeclarationSyntax interfaceDeclaration, SemanticModel SemanticModel)> interfaces)
    {

        // foreach (var (stateDeclaration, semanticModel) in interfaces)
        // {
        //     var entityStateInterfaceSymbol = semanticModel.GetDeclaredSymbol(stateDeclaration) ?? throw new ArgumentException("stateSymbol is null");
        //     var entityName = entityStateInterfaceSymbol.Name[1..].Replace("State", "");
        //     var namespaces = new HashSet<string> { };

        //     var entityStateClassCode = EntityStateTemplate.Generate(stateDeclaration, semanticModel, components, ns => namespaces.Add(ns));
        //     var entityPropertyAccessClassCode = EntityPropertyAccessTemplate.Generate(stateDeclaration, semanticModel, components, ns => namespaces.Add(ns));


        //     var namespacesBuilder = new StringBuilder();
        //     foreach (var ns in namespaces)
        //     {
        //         namespacesBuilder.AppendLine($"using {ns};");
        //     }
        // }

        var code = $$"""

namespace Tavern.Src.Ecs.Persistence;

using PdArchEcsCorePersistence;
using VContainer;
using VContainer.Godot;
using VContainer.Pools;
using VContainer.Pools.Impls;

public partial class AccessInstaller : MonoInstaller
{
    public override void Install(IContainerBuilder builder)
    {
        builder.Register<GeneralStateAccess>(Lifetime.Singleton).AsImplementedInterfaces();

        builder.Register<GeneralStateByteConverter>(Lifetime.Singleton).AsImplementedInterfaces();

        builder.Register<InventoryAccess>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<InventoryPropertyAccess>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

        builder.Register<ContainerItemAccess>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<ContainerItemPropertyAccess>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

        BindStatePool<InventoryState, IInventoryState>(builder);

        builder.RegisterPool<GeneralState, GeneralStatePool, IPool<GeneralState>>();
        builder.RegisterPool<ContainerItemState, StatePool<ContainerItemState, IContainerItemState>, IPool<ContainerItemState>>();
    }

    protected void BindStatePool<TState, TAccess>(IContainerBuilder builder)
        where TState : TAccess, new()
         where TAccess : IProperty
    {
        builder.RegisterPool<TState, SimpleMemoryPool<TState>, IPool<TState>>();
    }
}


""";

        var formattedCode = code.FormatCode();
        // context.AddSource($"EcsCodeGen.Persistence/AccessInstallManager.g.cs", formattedCode);

    }
}

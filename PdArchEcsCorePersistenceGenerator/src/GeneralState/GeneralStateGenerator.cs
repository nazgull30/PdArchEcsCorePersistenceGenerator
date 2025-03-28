namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistence;
using PdArchEcsCorePersistenceGenerator.Utils;

[Generator]
public class GeneralStateGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var interfaceDeclarations = context.SyntaxProvider
          .CreateSyntaxProvider(
              predicate: (node, _) => node is InterfaceDeclarationSyntax,
              transform: (context, _) => (context.Node as InterfaceDeclarationSyntax, context.SemanticModel)
          )
          .Where(pair => Utilities.HasAttribute(nameof(EntityStateAttribute), pair.Item1, pair.SemanticModel))
          .Collect();

        context.RegisterSourceOutput(interfaceDeclarations, GenerateCode);
    }

    private void GenerateCode(SourceProductionContext context,
       ImmutableArray<(InterfaceDeclarationSyntax, SemanticModel)> interfaces)
    {

        var namespaces = new HashSet<string> { };
        var methodsDict = new Dictionary<string, string>();

        var entityStates = new List<EntityStateInfo>();
        foreach (var (ctx, semanticModel) in interfaces)
        {
            var entityStateSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("interface is null");
            var symbol = entityStateSymbol as INamedTypeSymbol;

            var attribute = Utilities.GetAttributes(nameof(EntityStateAttribute), symbol).First();
            var multipleObj = GetMultipleValue(attribute);
            var multiple = multipleObj != null && (bool)multipleObj;
            var info = new EntityStateInfo
            {
                Symbol = symbol,
                Multiple = multiple
            };
            entityStates.Add(info);
        }

        var generateStateCode = GeneralStateTemplate.Generate(entityStates, ns => namespaces.Add(ns));
        var generateStatePoolCode = GeneralStatePoolTemplate.Generate(entityStates, ns => namespaces.Add(ns));
        var generateStateAccessCode = GeneralStateAccessTemplate.Generate(entityStates, ns => namespaces.Add(ns));
        var generalStateByteConverterCode = GeneralStateByteConverterTemplate.Generate(entityStates, ns => namespaces.Add(ns));

        var namespacesBuilder = new StringBuilder();
        foreach (var ns in namespaces)
        {
            namespacesBuilder.AppendLine($"using {ns};");
        }

        var code = $$"""
namespace PdArchEcsCorePersistence;

using ByteFormatter;
using System.Threading.Tasks;
using System.Collections.Generic;
using VContainer.Pools.Impls;
using VContainer.Pools;
using VContainer;
{{namespacesBuilder}}

{{generateStateCode}}

{{generateStatePoolCode}}

public interface IGeneralStateAccess : IAccess<GeneralState>
{
}

{{generateStateAccessCode}}

public interface IGeneralStateByteConverter
{
    GeneralState FromBytes(byte[] bytes);
    byte[] ToBytes(GeneralState gameState);
}

{{generalStateByteConverterCode}}

public interface IGeneralStateDao
{
    public Task Save(GeneralState generalState);
    public GeneralState Load();
}


""";
        var formattedCode = code.FormatCode();
        context.AddSource($"EcsCodeGen.Persistence/GeneralState.g.cs", formattedCode);
    }

    public static object GetMultipleValue(AttributeData attribute)
    {
        // Look for the field in NamedArguments
        foreach (var namedArg in attribute.NamedArguments)
        {
            if (namedArg.Key == "Multiple")
            {
                return namedArg.Value.Value; // Extract the field value
            }
        }

        if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0].Value is bool constructorValue)
        {
            return constructorValue;
        }


        return null; // Field not found
    }
}

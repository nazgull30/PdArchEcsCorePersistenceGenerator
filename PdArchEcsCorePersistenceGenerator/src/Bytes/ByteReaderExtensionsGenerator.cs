namespace PdArchEcsCorePersistenceGenerator.Bytes;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCore.Entities;
using PdArchEcsCorePersistenceGenerator.Utils;

[Generator]
public class ByteReaderExtensionsGenerator : IIncrementalGenerator
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

        var namespaces = new HashSet<string> { };
        var methodsDict = new Dictionary<string, string>();
        foreach (var (ctx, semanticModel) in structs)
        {
            var structSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");

            var properties = GetProperties(ctx, semanticModel);

            foreach (var property in properties)
            {
                if (methodsDict.ContainsKey(property.FieldType))
                    continue;

                var propertyMethods = ByteReaderPropertyMethodsTemplate.Generate(property);
                methodsDict.Add(property.FieldType, propertyMethods);
                namespaces.Add(property.Namespace);
            }

        }

        var namespacesBuilder = new StringBuilder();
        foreach (var ns in namespaces)
        {
            namespacesBuilder.AppendLine($"using {ns};");
        }

        var methodsBuilder = new StringBuilder();
        foreach (var method in methodsDict.Values)
        {
            methodsBuilder.AppendLine(method);
        }

        var code = $$"""
namespace PdArchEcsCorePersistence;

using ByteFormatter;
{{namespacesBuilder}}

public static class ByteReaderExtensions
{
    {{methodsBuilder}}
}

""";

        var formattedCode = code.FormatCode();
        context.AddSource($"EcsCodeGen.Persistence/ByteReaderExtensions.g.cs", formattedCode);
    }

    private static List<PropertyInfo> GetProperties(StructDeclarationSyntax stx, SemanticModel semanticModel)
    {
        var properties = new List<PropertyInfo>();
        foreach (var field in stx.Members.OfType<FieldDeclarationSyntax>())
        {
            foreach (var variable in field.Declaration.Variables)
            {
                var fieldTypeSyntax = field.Declaration.Type;
                var fieldTypeSymbol = semanticModel.GetTypeInfo(fieldTypeSyntax).Type;
                var fieldName = variable.Identifier.Text;
                var fieldType = fieldTypeSymbol.Name;

                if (fieldTypeSymbol.TypeKind != TypeKind.Enum && fieldType != "Uid")
                    continue;

                properties.Add(new PropertyInfo(fieldName, fieldType, fieldTypeSymbol.ContainingNamespace.ToDisplayString()));

            }
        }
        return properties;
    }

}

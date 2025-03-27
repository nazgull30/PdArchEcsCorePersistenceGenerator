namespace PdArchEcsCorePersistenceGenerator.Components;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class ComponentPropertyTemplate
{
    public static (string, string) Generate(StructDeclarationSyntax ctx, SemanticModel semanticModel)
    {
        var structSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");

        var properties = PropertyUtils.GetPropertiesEnumOrUid(ctx, semanticModel);
        if (properties.Count != 1)
            return (null, null);

        var property = properties[0];
        var statementSb = new StringBuilder();
        statementSb.Append("public ").Append($"{property.FieldType}").Append($" {property.FieldName} ").Append("  { get; set; }");

        var code = $$"""

        public interface I{{structSymbol.Name}}Property : IProperty
        {
            {{statementSb}}
        }
""";
        return (code, property.Namespace);
    }
}

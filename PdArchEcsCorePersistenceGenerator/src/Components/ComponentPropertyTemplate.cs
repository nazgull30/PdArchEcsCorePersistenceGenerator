namespace PdArchEcsCorePersistenceGenerator.Components;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class ComponentPropertyTemplate
{
    public static string Generate(StructDeclarationSyntax ctx, SemanticModel semanticModel, Action<string> addNs)
    {
        var structSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");

        var properties = PropertyUtils.GetProperties(ctx, semanticModel);
        if (properties.Count != 1)
            return null;

        var property = properties[0];
        addNs(property.Namespace);
        var statementSb = new StringBuilder();
        statementSb.Append("public ").Append($"{property.FieldType}?").Append($" {structSymbol.Name} ").Append("  { get; set; }");

        var code = $$"""

        public interface I{{structSymbol.Name}}Property : IProperty
        {
            {{statementSb}}
        }
""";
        return code;
    }
}

namespace PdArchEcsCorePersistenceGenerator.Components;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class ComponentPropertyAccessTemplate
{
    public static string Generate(StructDeclarationSyntax ctx, SemanticModel semanticModel, Action<string> addNs)
    {
        var structSymbol = semanticModel.GetDeclaredSymbol(ctx) ?? throw new ArgumentException("structSymbol is null");

        var properties = PropertyUtils.GetPropertiesEnumOrUid(ctx, semanticModel);
        if (properties.Count != 1)
            return null;

        var property = properties[0];
        addNs(property.Namespace);

        var className = structSymbol.Name[1..];

        var code = $$"""

        public class {{structSymbol.Name}}PropertyAccess : IPropertyAccess<I{{structSymbol.Name}}Property>
        {
            public void SetObjectValue(Entity entity, I{{structSymbol.Name}}Property property)
            {
                if (!property.{{structSymbol.Name}}.HasValue)
                    return;
                entity.Add{{structSymbol.Name}}(property.{{structSymbol.Name}}.Value);
            }

            public void SetPropertyValue(Entity entity, I{{structSymbol.Name}}Property property)
            {
                if (!entity.Has{{structSymbol.Name}}())
                    return;
                property.{{structSymbol.Name}} = entity.{{structSymbol.Name}}().Value;
            }

            public void Reset(I{{structSymbol.Name}}Property property)
            {
                property.{{structSymbol.Name}} = null;
            }
        }
""";
        return code;
    }
}

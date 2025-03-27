namespace PdArchEcsCorePersistenceGenerator.Utils;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class PropertyUtils
{
    public static List<PropertyInfo> GetProperties(StructDeclarationSyntax stx, SemanticModel semanticModel)
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

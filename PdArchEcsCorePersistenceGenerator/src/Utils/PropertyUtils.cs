namespace PdArchEcsCorePersistenceGenerator.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class PropertyUtils
{
    public static List<PropertyInfo> GetPropertiesEnumOrUid(StructDeclarationSyntax stx, SemanticModel semanticModel)
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

                if (fieldTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    fieldType += "?";
                }

                properties.Add(new PropertyInfo(fieldName, fieldType, fieldTypeSymbol.ContainingNamespace.ToDisplayString()));

            }
        }
        return properties;
    }

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
                var fieldType = fieldTypeSymbol.ToDisplayString();

                properties.Add(new PropertyInfo(fieldName, fieldType, fieldTypeSymbol.ContainingNamespace.ToDisplayString()));

            }
        }
        return properties;
    }

    public static PropertyInfo GetSingleProperties(StructDeclarationSyntax stx, SemanticModel semanticModel)
    {
        var structSymbol = semanticModel.GetDeclaredSymbol(stx) ?? throw new ArgumentException("structSymbol is null");
        var fields = stx.Members.OfType<FieldDeclarationSyntax>();
        if (fields.Count() != 1)
            throw new ArgumentException($"Single property for {structSymbol.Name}. Should be only one property.");

        foreach (var field in stx.Members.OfType<FieldDeclarationSyntax>())
        {
            foreach (var variable in field.Declaration.Variables)
            {
                var fieldTypeSyntax = field.Declaration.Type;
                var fieldTypeSymbol = semanticModel.GetTypeInfo(fieldTypeSyntax).Type;

                var fieldName = variable.Identifier.Text;
                var fieldType = fieldTypeSymbol.ToDisplayString();

                return new PropertyInfo(fieldName, fieldType, fieldTypeSymbol.ContainingNamespace.ToDisplayString());

            }
        }
        throw new ArgumentException($"Single property for {structSymbol.Name} not found");
    }

}

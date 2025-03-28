namespace PdArchEcsCorePersistenceGenerator.Utils;

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public static class Utilities
{
    public static bool HasAttribute(string attributeName, SyntaxNode declaration, SemanticModel semanticModel)
    {
        var symbol = semanticModel.GetDeclaredSymbol(declaration);
        if (symbol == null)
        {
            return false;
        }

        return symbol.GetAttributes().Any(a => a.AttributeClass?.Name == attributeName);
    }

    public static bool HasAttribute(ITypeSymbol typeSymbol, string attributeName)
    {
        var attributes = typeSymbol.GetAttributes();
        return attributes.Any(attr => attr.AttributeClass?.Name == attributeName);
    }

    public static IEnumerable<AttributeData> GetAttributes(string attributeName, ISymbol symbol)
    {
        return symbol.GetAttributes().Where(a => a.AttributeClass?.Name == attributeName);
    }

    public static bool ImplementsInterface(string interfaceFullName, TypeDeclarationSyntax ctx, SemanticModel semanticModel)
    {
        var classSymbol = semanticModel.GetDeclaredSymbol(ctx) as INamedTypeSymbol;

        while (classSymbol != null)
        {
            if (classSymbol.AllInterfaces.Any(i => i.ToDisplayString() == interfaceFullName))
            {
                return true;
            }
            classSymbol = classSymbol.BaseType;
        }
        return false;
    }

    public static List<IMethodSymbol> GetAllMethods(ITypeSymbol typeSymbol)
    {
        var allMethods = new List<IMethodSymbol>();

        while (typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Object)
        {
            allMethods.AddRange(typeSymbol.GetMembers().OfType<IMethodSymbol>());
            typeSymbol = typeSymbol.BaseType;
        }

        return allMethods;
    }

    public static List<IFieldSymbol> GetAllFields(ITypeSymbol typeSymbol)
    {
        var allFields = new List<IFieldSymbol>();

        // Traverse class and all parent classes
        while (typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Object)
        {
            allFields.AddRange(typeSymbol.GetMembers().OfType<IFieldSymbol>());
            typeSymbol = typeSymbol.BaseType;
        }

        return allFields;
    }

    public static List<IPropertySymbol> GetAllProperties(ITypeSymbol typeSymbol)
    {
        var allProperties = new List<IPropertySymbol>();

        // Traverse class and all parent classes
        while (typeSymbol != null && typeSymbol.SpecialType != SpecialType.System_Object)
        {
            allProperties.AddRange(typeSymbol.GetMembers().OfType<IPropertySymbol>());
            typeSymbol = typeSymbol.BaseType;
        }

        return allProperties;
    }

    public static bool InheritsFrom(ITypeSymbol typeSymbol, string baseClassName)
    {
        while (typeSymbol != null)
        {
            if (typeSymbol.Name == baseClassName)
                return true; // ✅ Found the base class

            typeSymbol = typeSymbol.BaseType; // Move up the inheritance chain
        }

        return false; // ❌ Not found
    }

    public static bool ImplementsIAccess(ITypeSymbol typeSymbol, string interfaceName)
    {
        while (typeSymbol != null)
        {
            // 🔹 Check all interfaces of the current class/base class
            foreach (var interfaceSymbol in typeSymbol.AllInterfaces)
            {
                if (interfaceSymbol.OriginalDefinition.ToDisplayString() == interfaceName)
                {
                    return true;
                }
            }

            // Move up the inheritance chain
            typeSymbol = typeSymbol.BaseType;
        }

        return false;
    }

}

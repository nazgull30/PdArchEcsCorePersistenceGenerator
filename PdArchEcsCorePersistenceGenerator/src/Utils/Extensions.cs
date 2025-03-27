namespace PdArchEcsCorePersistenceGenerator.Utils;

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class Extensions
{
    public static bool HasAttribute<T>(this MemberInfo type)
        where T : Attribute
        => type.GetCustomAttribute<T>() != null;

    public static string FirstCharToLower(this string source) =>
        string.Concat(source[..1].ToLowerInvariant(), source.AsSpan(1));

    public static string FirstCharToUpper(this string source) =>
        string.Concat(source[..1].ToUpperInvariant(), source.AsSpan(1));

    public static string FormatCode(this string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        var formattedRoot = root.NormalizeWhitespace();

        return formattedRoot.ToFullString();
    }

    public static string RemoveE(this string enumType)
    {
        if (enumType.StartsWith("E"))
        {
            return enumType[1..];
        }
        return enumType;
    }

    public static string GetTypeName(this IPropertySymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            var typeSymbol = type.Type as INamedTypeSymbol;
            var basicType = typeSymbol.TypeArguments[0];
            if (basicType.TypeKind == TypeKind.Enum)
                return basicType.Name.RemoveE();
            return basicType.Name;
        }

        return type.Type.ConvertToPrimitive();
    }

    public static string GetTypeName(this ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            var typeSymbol = type as INamedTypeSymbol;
            var basicType = typeSymbol.TypeArguments[0];
            if (basicType.TypeKind == TypeKind.Enum)
                return basicType.Name.RemoveE();
            return basicType.Name;
        }

        return type.ConvertToPrimitive();
    }


    public static bool IsPrimitiveType(this SpecialType specialType)
    {
        return specialType is >= SpecialType.System_Boolean and <= SpecialType.System_String;
    }

    public static bool IsPrimitiveType(this ITypeSymbol type) => type.SpecialType.IsPrimitiveType();

    public static string ConvertToPrimitive(this ITypeSymbol type) => _universalTypes[type.SpecialType];

    private static readonly Dictionary<SpecialType, string> _universalTypes = new()
        {
            {SpecialType.System_SByte, "sbyte"},
            {SpecialType.System_Byte, "byte"},
            {SpecialType.System_Int16, "short"},
            {SpecialType.System_UInt16, "ushort"},
            {SpecialType.System_Int32, "int"},
            {SpecialType.System_UInt32, "uint"},
            {SpecialType.System_Int64, "long"},
            {SpecialType.System_UInt64, "ulong"},
            {SpecialType.System_Single, "float"},
            {SpecialType.System_Double, "double"},
            {SpecialType.System_Boolean, "bool"},
            {SpecialType.System_Char, "char"},
            {SpecialType.System_String, "string"},
            {SpecialType.System_Object, "object"},
            {SpecialType.System_Void, "void"}
        };

}

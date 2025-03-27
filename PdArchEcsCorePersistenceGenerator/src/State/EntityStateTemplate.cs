namespace PdArchEcsCorePersistenceGenerator.State;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class EntityStateTemplate
{

    public static string Generate(
        InterfaceDeclarationSyntax stx,
        SemanticModel semanticModel,
        List<INamedTypeSymbol> components,
        Action<string> addNs)
    {
        var stateInterfaceSymbol = semanticModel.GetDeclaredSymbol(stx) as INamedTypeSymbol ?? throw new ArgumentException("interface is null");
        if (stateInterfaceSymbol == null)
            throw new ArgumentException("interface is null");


        var statePropertiesSb = new StringBuilder();
        var writeSb = new StringBuilder();
        var readSb = new StringBuilder();
        foreach (var propertyInterface in stateInterfaceSymbol.AllInterfaces)
        {
            var componentName = propertyInterface.Name[1..].Replace("Property", "");
            var componentSymbol = components.Find(c => c.Name == componentName);
            if (componentSymbol == null)
                continue;

            var field = componentSymbol.GetMembers().OfType<IFieldSymbol>().First();

            addNs(field.Type.ContainingNamespace.ToDisplayString());
            var typeName = field.Type.IsPrimitiveType() ? field.Type.ToDisplayString() : field.Type.Name;
            statePropertiesSb.Append("public ").Append($"{typeName}?").Append($" {componentName} ").Append("  { get; set; }\n");
            writeSb.AppendLine($"writer.Write({componentName});");
            var readStatement = CreateRead(field, componentName);
            readSb.AppendLine(readStatement);
        }

        var className = stateInterfaceSymbol.Name[1..];


        var code = $$"""

        public class {{className}} : {{stateInterfaceSymbol.Name}}, IByteConvertable
        {
            {{statePropertiesSb}}

                public void ToByte(ByteWriter writer)
                {
                    {{writeSb}}
                }

                public void FromByte(ByteReader reader)
                {
                    {{readSb}}
                }
        }
""";

        return code;
    }


    private static string CreateRead(IFieldSymbol fieldSymbol, string componentName)
    {

        if (fieldSymbol.Type.Name.Contains("Uid"))
        {
            return $"{componentName} = reader.ReadNullableUid();";
        }

        var typeName = fieldSymbol.Type.Name;
        if (fieldSymbol.Type.TypeKind == TypeKind.Enum)
        {
            typeName = fieldSymbol.Type.Name.RemoveE();
        }
        var code = $$"""
            {{componentName}} = reader.Read{{typeName}}Nullable();
""";
        return code;
    }

}

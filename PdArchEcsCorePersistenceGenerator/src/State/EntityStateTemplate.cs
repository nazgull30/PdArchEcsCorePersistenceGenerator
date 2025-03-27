namespace PdArchEcsCorePersistenceGenerator.State;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class EntityStateTemplate
{
    public static string Generate(InterfaceDeclarationSyntax stx, SemanticModel semanticModel, Action<string> addNs)
    {
        var stateInterfaceSymbol = semanticModel.GetDeclaredSymbol(stx) as INamedTypeSymbol ?? throw new ArgumentException("interface is null");
        if (stateInterfaceSymbol == null)
            throw new ArgumentException("interface is null");


        var statePropertiesSb = new StringBuilder();
        var writeSb = new StringBuilder();
        var readSb = new StringBuilder();
        foreach (var propertyInterface in stateInterfaceSymbol.AllInterfaces)
        {
            var properties = Utilities.GetAllProperties(propertyInterface);
            if (properties.Count == 0)
                continue;
            var property = properties[0];
            statePropertiesSb.Append("public ").Append($"{property.Type.ToDisplayString()}").Append($" {property.Name} ").Append("  { get; set; }\n");
            writeSb.AppendLine($"writer.Write({property.Name});");
            var readStatement = CreateRead(property);
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


    private static string CreateRead(IPropertySymbol propertySymbol)
    {

        if (propertySymbol.Type.ToDisplayString().Contains("Uid?"))
        {
            return $"{propertySymbol.Name} = reader.ReadNullableUid();";
        }

        var typeName = propertySymbol.GetTypeName();
        var code = $$"""
            {{propertySymbol.Name}} = reader.Read{{typeName}}Nullable();
""";
        return code;
    }
}

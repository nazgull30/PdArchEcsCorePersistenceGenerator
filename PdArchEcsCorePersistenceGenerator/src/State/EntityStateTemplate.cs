namespace PdArchEcsCorePersistenceGenerator.State;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class EntityStateTemplate
{

    public static string Generate(
        InterfaceDeclarationSyntax stx,
        SemanticModel semanticModel,
        Func<string, PropertyInfo> getComponentProperty,
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
            var property = getComponentProperty(componentName);

            statePropertiesSb.Append("public ").Append($"{property.FieldType}?").Append($" {componentName} ").Append("  { get; set; }\n");
            writeSb.AppendLine($"writer.Write({componentName});");
            var readStatement = CreateRead(property, componentName);
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


    private static string CreateRead(PropertyInfo propertyInfo, string componentName)
    {

        if (propertyInfo.FieldType.Contains("Uid"))
        {
            return $"{componentName} = reader.ReadNullableUid();";
        }

        var code = $$"""
            {{componentName}} = reader.Read{{propertyInfo.FieldType}}Nullable();
""";
        return code;
    }

}

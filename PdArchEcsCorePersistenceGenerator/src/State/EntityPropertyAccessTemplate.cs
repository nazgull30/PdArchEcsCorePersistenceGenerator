namespace PdArchEcsCorePersistenceGenerator.State;

using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class EntityPropertyAccessTemplate
{
    public static string Generate(InterfaceDeclarationSyntax stx, SemanticModel semanticModel, Action<string> addNs)
    {
        var stateInterfaceSymbol = semanticModel.GetDeclaredSymbol(stx) as INamedTypeSymbol ?? throw new ArgumentException("interface is null");
        if (stateInterfaceSymbol == null)
            throw new ArgumentException("interface is null");


        var statePropertiesSb = new StringBuilder();
        foreach (var propertyInterface in stateInterfaceSymbol.AllInterfaces)
        {
            var properties = Utilities.GetAllProperties(propertyInterface);
            if (properties.Count == 0)
                continue;
            var property = properties[0];
            statePropertiesSb.AppendLine($"AddOriginator<{property.Name}PropertyAccess>();");
        }

        var className = stateInterfaceSymbol.Name[1..].Replace("State", "");


        var code = $$"""

        [GenerateInjector]
        public class {{className}}PropertyAccess: ObjectPropertyAccess<{{stateInterfaceSymbol.Name}}>
        {
            public override void Initialize()
            {
                {{statePropertiesSb}}
            }
        }
""";

        return code;
    }
}

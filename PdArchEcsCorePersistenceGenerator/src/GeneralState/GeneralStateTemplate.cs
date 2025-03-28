namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Text;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class GeneralStateTemplate
{
    public static string Generate(List<EntityStateInfo> entityStates, Action<string> addNs)
    {
        var fields = new StringBuilder();
        var writers = new StringBuilder();
        var readers = new StringBuilder();

        foreach (var entityState in entityStates)
        {
            var fieldName = GetFieldName(entityState);
            var field = CreateField(entityState, fieldName);
            fields.AppendLine(field);
            addNs(entityState.Symbol.ContainingNamespace.ToDisplayString());
            writers.AppendLine($"writer.Write({fieldName});");
            var read = CreateRead(entityState, fieldName);
            readers.AppendLine(read);
        }

        var code = $$"""

        public class GeneralState
        {
            {{fields}}

            public void ToByte(ByteWriter writer)
            {
                {{writers}}
            }

            public void FromByte(ByteReader reader)
            {
                {{readers}}
            }
        }

""";
        return code;
    }

    private static string CreateField(EntityStateInfo entityState, string fieldName)
    {
        var typeName = entityState.Symbol.Name[1..];
        return entityState.Multiple ? $"public List<{typeName}> {fieldName} = [];" : $"public {typeName} {fieldName} = new();";
    }

    private static string CreateRead(EntityStateInfo entityState, string fieldName)
    {
        var typeName = entityState.Symbol.Name[1..];
        return entityState.Multiple ? $"{fieldName} = reader.ReadList<{typeName}>();" : $"{fieldName} = reader.Read<{typeName}>();";
    }

    private static string GetFieldName(EntityStateInfo entityState)
    {
        var removeState = entityState.Symbol.Name[1..].Replace("State", "");
        return entityState.Multiple ? removeState + "s" : removeState;
    }
}

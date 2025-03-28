namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Text;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class GeneralStatePoolTemplate
{
    public static string Generate(List<EntityStateInfo> entityStates, Action<string> addNs)
    {
        var onDespawned = new StringBuilder();

        foreach (var entityState in entityStates)
        {
            var fieldName = GetFieldName(entityState)[1..];
            addNs(entityState.Symbol.ContainingNamespace.ToDisplayString());
            if (entityState.Multiple)
            {
                onDespawned.AppendLine($"generalState.{fieldName}.Clear();");
            }
            else
            {
                var typeName = entityState.Symbol.Name[1..];
                onDespawned.AppendLine($"generalState.{fieldName} = new {typeName}();");
            }
        }

        var code = $$"""

        public class GeneralStatePool : SimpleMemoryPool<GeneralState>
        {
            protected override void OnDespawned(GeneralState generalState)
            {
                {{onDespawned}}
            }
        }


""";
        return code;
    }

    private static string GetFieldName(EntityStateInfo entityState)
    {
        var removeState = entityState.Symbol.Name.Replace("State", "");
        return entityState.Multiple ? removeState + "s" : removeState;
    }
}

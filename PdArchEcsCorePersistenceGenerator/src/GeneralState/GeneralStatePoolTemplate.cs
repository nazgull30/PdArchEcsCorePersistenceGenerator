namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Text;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class GeneralStatePoolTemplate
{
    public static string Generate(List<EntityStateInfo> entityStates, Action<string> addNs)
    {
        var onCreated = new StringBuilder();
        var onSpawned = new StringBuilder();

        foreach (var entityState in entityStates)
        {
            var fieldName = GetFieldName(entityState)[1..];
            addNs(entityState.Symbol.ContainingNamespace.ToDisplayString());
            if (entityState.Multiple)
            {
                onCreated.AppendLine($"generalState.{fieldName} = [];");
                onSpawned.AppendLine($"generalState.{fieldName}= [];");
            }
            else
            {
                var typeName = entityState.Symbol.Name[1..];
                onSpawned.AppendLine($"generalState.{fieldName} = new {typeName}();");
            }
        }

        var code = $$"""

        public class GeneralStatePool : SimpleMemoryPool<GeneralState>
        {
            protected override void OnCreated(GeneralState generalState)
            {
                {{onCreated}}
            }

            protected override void OnSpawned(GeneralState generalState)
            {
                {{onSpawned}}
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

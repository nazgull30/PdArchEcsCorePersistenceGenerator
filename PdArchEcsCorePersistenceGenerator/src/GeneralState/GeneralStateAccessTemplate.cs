namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Text;
using PdArchEcsCorePersistenceGenerator.Utils;

public static class GeneralStateAccessTemplate
{
    public static string Generate(List<EntityStateInfo> entityStates, Action<string> addNs)
    {
        var access = new StringBuilder();
        var read = new StringBuilder();
        var write = new StringBuilder();

        foreach (var entityState in entityStates)
        {
            var fieldName = entityState.Symbol.Name.Replace("State", "")[1..];
            addNs(entityState.Symbol.ContainingNamespace.ToDisplayString());
            access.AppendLine(CreateAccess(entityState, fieldName));
            read.AppendLine(CreateRead(entityState, fieldName));
            write.AppendLine(CreateWrite(entityState, fieldName));
        }
        if (access.Length > 1)
        {
            access.Remove(access.Length - 2, 1);
        }

        var code = $$"""

       public class GeneralStateAccess(
            {{access}}
        ) : IGeneralStateAccess
        {
            public void ReadState(GeneralState state)
            {
                {{read}}
            }

            public void WriteState(GeneralState state)
            {
                {{write}}
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

    private static string CreateAccess(EntityStateInfo entityState, string fieldName)
    {
        return entityState.Multiple ?
             $"IAccess<List<{fieldName}State>> {fieldName.FirstCharToLower()}Access,"
             : $"IAccess<{fieldName}State> {fieldName.FirstCharToLower()}Access,";
    }

    private static string CreateRead(EntityStateInfo entityState, string fieldName)
    {
        return entityState.Multiple ?
             $"{fieldName.FirstCharToLower()}Access.ReadState(state.{fieldName}s);"
             : $"{fieldName.FirstCharToLower()}Access.ReadState(state.{fieldName});";
    }

    private static string CreateWrite(EntityStateInfo entityState, string fieldName)
    {
        return entityState.Multiple ?
             $"{fieldName.FirstCharToLower()}Access.WriteState(state.{fieldName}s);"
             : $"{fieldName.FirstCharToLower()}Access.WriteState(state.{fieldName});";
    }

}

namespace PdArchEcsCorePersistenceGenerator.GeneralState;

using System;
using System.Collections.Generic;
using System.Text;
using PdArchEcsCorePersistenceGenerator.Utils;

public class GeneralStateByteConverterTemplate
{
    public static string Generate(List<EntityStateInfo> entityStates, Action<string> addNs)
    {
        var reads = new StringBuilder();
        var pools = new StringBuilder();

        foreach (var entityState in entityStates)
        {
            var fieldName = entityState.Symbol.Name.Replace("State", "")[1..];
            addNs(entityState.Symbol.ContainingNamespace.ToDisplayString());
            if (entityState.Multiple)
            {
                pools.AppendLine($"IPool<{fieldName}State> {fieldName.FirstCharToLower()}Pool");
                reads.AppendLine($"ReadList(generalState.{fieldName}s, {fieldName.FirstCharToLower()}Pool, reader);");
            }
            else
            {
                reads.AppendLine($"generalState.{fieldName}.FromByte(reader);");
            }
        }

        if (pools.Length > 1)
        {
            pools.Remove(pools.Length - 1, 1);
        }

        var code = $$"""

        public class GeneralStateByteConverter(
            {{pools}}
        ) : IGeneralStateByteConverter
        {
            public byte[] ToBytes(GeneralState gameState)
            {
                var writer = new ByteWriter(new byte[262144]);
                gameState.ToByte(writer);
                return writer.ToArray();
            }

            public GeneralState FromBytes(byte[] bytes)
            {
                var reader = new ByteReader(bytes);
                var generalState = new GeneralState();
                {{reads}}
                return generalState;
            }

            private static void ReadList<TStateItem>(
                    List<TStateItem> list,
                    IPool<TStateItem> pool,
                    ByteReader reader
                )
                    where TStateItem : IByteConvertable
            {
                var count = reader.ReadInt32();
                if (list.Capacity < count)
                    list.Capacity = count;

                for (var i = 0; i < count; i++)
                {
                    var convertable = pool.Spawn();
                    convertable.FromByte(reader);
                    list.Add(convertable);
                }
            }
        }

""";
        return code;
    }
}

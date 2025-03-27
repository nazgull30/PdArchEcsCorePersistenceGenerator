namespace PdArchEcsCorePersistenceGenerator.Bytes;

using System.Collections.Generic;
using PdArchEcsCorePersistenceGenerator.Utils;

public class ByteWriterPropertyMethodsTemplate
{
    public static string Generate(PropertyInfo property)
    {
        var namespaces = new HashSet<string> { };

        var write = CreateWrite(property);
        var writeNullable = CreateWriteNullable(property);

        var code = $$"""
                {{write}}

                {{writeNullable}}
""";
        return code;
    }

    private static string CreateWrite(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                     public static void Write(this ByteWriter writer, Uid uid) => writer.Write((uint)uid);
                     """;
        }

        var paramName = propertyInfo.FieldType.RemoveE().FirstCharToLower();
        var code = $$"""
                        public static void Write(this ByteWriter writer, {{propertyInfo.FieldType}} {{paramName}})
                            => writer.Write((byte){{paramName}});
                     """;
        return code;
    }

    private static string CreateWriteNullable(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                        public static void Write(this ByteWriter writer, Uid? uid)
                        {
                            if (uid.HasValue)
                            {
                                writer.Write(true);
                                writer.Write((uint)uid.Value);
                            }
                            else
                                writer.Write(false);
                        }
                     """;
        }
        var paramName = propertyInfo.FieldType.RemoveE().FirstCharToLower();
        var code = $$"""
                        public static void Write(this ByteWriter writer, {{propertyInfo.FieldType}}? {{paramName}})
                        {
                            if ({{paramName}}.HasValue)
                            {
                                writer.Write(true);
                                writer.Write((byte){{paramName}}.Value);
                            }
                            else
                                writer.Write(false);
                        }
                     """;
        return code;
    }

}

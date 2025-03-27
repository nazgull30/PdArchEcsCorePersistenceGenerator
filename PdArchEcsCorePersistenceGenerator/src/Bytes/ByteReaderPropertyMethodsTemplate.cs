namespace PdArchEcsCorePersistenceGenerator.Bytes;

using System.Collections.Generic;
using PdArchEcsCorePersistenceGenerator.Utils;

public class ByteReaderPropertyMethodsTemplate
{
    public static string Generate(PropertyInfo property)
    {
        var namespaces = new HashSet<string> { };

        var read = CreateRead(property);
        var skip = CreateSkip(property);
        var readNullable = CreateReadNullable(property);
        var skipNullable = CreateSkipNullable(property);

        var code = $$"""
                {{read}}

                {{skip}}

                {{readNullable}}

                {{skipNullable}}
""";
        return code;
    }

    private static string CreateRead(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                     public static Uid ReadUid(this ByteReader reader) => (Uid)reader.ReadUInt32();
                     """;
        }

        var code = $$"""
                         public static {{propertyInfo.FieldType}} Read{{propertyInfo.FieldType.RemoveE()}}(this ByteReader reader)
                             => ({{propertyInfo.FieldType}})reader.ReadUInt16();
                     """;
        return code;
    }

    private static string CreateSkip(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                     public static void SkipUid(this ByteReader reader) => reader.SkipInt32();
                     """;
        }
        var code = $$"""
                         public static void Skip{{propertyInfo.FieldType.RemoveE()}}(this ByteReader reader)
                             => reader.SkipUInt16();
                     """;
        return code;
    }

    private static string CreateReadNullable(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                        public static Uid? ReadNullableUid(this ByteReader reader)
                        {
                            var hasValue = reader.ReadBoolean();
                            return hasValue ? (Uid)reader.ReadUInt32() : null;
                        }
                     """;
        }
        var code = $$"""
                        public static {{propertyInfo.FieldType}}? Read{{propertyInfo.FieldType.RemoveE()}}Nullable(this ByteReader reader)
                            => reader.ReadBoolean() ? ({{propertyInfo.FieldType}})reader.ReadUInt16() : null;
                     """;
        return code;
    }

    private static string CreateSkipNullable(PropertyInfo propertyInfo)
    {
        if (propertyInfo.FieldType == "Uid")
        {
            return $$"""
                        public static void SkipNullableUid(this ByteReader reader)
                        {
                            if (reader.ReadBoolean())
                                reader.SkipInt32();
                        }
                     """;
        }
        var code = $$"""
                        public static void Skip{{propertyInfo.FieldType.RemoveE()}}Nullable(this ByteReader reader)
                        {
                            if (reader.ReadBoolean())
                                reader.SkipUInt16();
                        }
                     """;
        return code;
    }

}

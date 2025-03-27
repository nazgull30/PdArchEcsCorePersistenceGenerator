namespace PdArchEcsCorePersistenceGenerator.Utils;

using Microsoft.CodeAnalysis;

public struct PropertyInfo(string fieldName, string fieldType, string ns)
{
    public readonly string FieldName = fieldName;
    public readonly string FieldType = fieldType;
    public readonly string Namespace = ns;

    public ITypeSymbol FieldTypeSymbol;
}

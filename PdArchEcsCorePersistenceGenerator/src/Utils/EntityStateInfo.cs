namespace PdArchEcsCorePersistenceGenerator.Utils;

using Microsoft.CodeAnalysis;

public struct EntityStateInfo
{
    public INamedTypeSymbol Symbol;
    public bool Multiple;
}

namespace StoryblokSharp.Utilities;

/// <summary>
/// Extension methods for Type operations
/// </summary>
public static class TypeExtensions 
{
    private static readonly HashSet<Type> NumericTypes = new()
    {
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal)
    };

    /// <summary>
    /// Determines if a type represents a numeric value
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is numeric, false otherwise</returns>
    public static bool IsNumeric(this Type type)
    {
        // Handle nullable numeric types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = Nullable.GetUnderlyingType(type)!;
        }
        
        return NumericTypes.Contains(type);
    }
}
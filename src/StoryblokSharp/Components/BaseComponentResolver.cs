using System.Text.Json;
using StoryblokSharp.Utilities;

namespace StoryblokSharp.Components;

/// <summary>
/// Base class for component resolvers
/// </summary>
public abstract class BaseComponentResolver : IComponentResolver 
{
    private readonly Dictionary<string, Type> _componentTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly JsonSerializerOptions _jsonOptions;

    protected BaseComponentResolver()
    {
        _jsonOptions = JsonHelper.GetDefaultOptions();
    }

    public virtual string ResolveComponent(string componentType, IDictionary<string, object> props)
    {
        if (!SupportsComponent(componentType))
            return string.Empty;

        ValidateProps(componentType, props);
        return RenderComponent(componentType, props);
    }

    public virtual bool SupportsComponent(string componentType)
    {
        return _componentTypes.ContainsKey(componentType);
    }

    public virtual void ValidateProps(string componentType, IDictionary<string, object> props)
    {
        var type = GetComponentType(componentType);
        if (type == null)
            return;

        // Validate required props
        var requiredProps = type.GetProperties()
            .Where(p => Attribute.IsDefined(p, typeof(RequiredPropAttribute)));

        foreach (var prop in requiredProps)
        {
            if (!props.ContainsKey(prop.Name))
            {
                throw new ArgumentException(
                    $"Required prop '{prop.Name}' missing for component '{componentType}'");
            }
        }

        // Validate prop types
        foreach (var (key, value) in props)
        {
            var propInfo = type.GetProperty(key);
            if (propInfo == null)
                continue;

            var propType = propInfo.PropertyType;
            if (!IsValidPropType(value, propType))
            {
                throw new ArgumentException(
                    $"Invalid type for prop '{key}' in component '{componentType}'. " +
                    $"Expected {propType.Name}, got {value.GetType().Name ?? "null"}");
            }
        }
    }

    public void RegisterComponent(string componentType, Type componentClass)
    {
        if (string.IsNullOrEmpty(componentType))
            throw new ArgumentNullException(nameof(componentType));

        ArgumentNullException.ThrowIfNull(componentClass);

        _componentTypes[componentType] = componentClass;
    }

    public Type? GetComponentType(string componentType)
    {
        return _componentTypes.TryGetValue(componentType, out var type) ? type : null;
    }

    protected abstract string RenderComponent(string componentType, IDictionary<string, object> props);

    protected T? DeserializeProps<T>(IDictionary<string, object> props) where T : class
    {
        var json = JsonSerializer.Serialize(props, _jsonOptions);
        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }

    private static bool IsValidPropType(object? value, Type expectedType)
    {
        if (value == null)
            return !expectedType.IsValueType || Nullable.GetUnderlyingType(expectedType) != null;

        var valueType = value.GetType();
        
        // Handle nullable types
        if (Nullable.GetUnderlyingType(expectedType) is Type underlyingType)
            expectedType = underlyingType;

        // Handle numeric conversions
        if (expectedType.IsNumeric() && valueType.IsNumeric())
            return true;

        // Handle array/list types
        if (expectedType.IsArray && value is System.Collections.IEnumerable)
            return true;

        // Handle dictionary types
        if (expectedType.IsGenericType && 
            expectedType.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
            value is System.Collections.IDictionary)
            return true;

        return expectedType.IsAssignableFrom(valueType);
    }
}

/// <summary>
/// Attribute for marking required component properties
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredPropAttribute : Attribute
{
}

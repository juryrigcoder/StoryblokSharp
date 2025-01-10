
namespace StoryblokSharp.Components;

/// <summary>
/// Interface for component resolution
/// </summary>
public interface IComponentResolver
{
    /// <summary>
    /// Resolves a component to its HTML representation
    /// </summary>
    string ResolveComponent(string componentType, IDictionary<string, object> props);

    /// <summary>
    /// Checks if a component type is supported
    /// </summary>
    bool SupportsComponent(string componentType);

    /// <summary>
    /// Validates component properties
    /// </summary>
    void ValidateProps(string componentType, IDictionary<string, object> props);

    /// <summary>
    /// Registers a new component type
    /// </summary>
    void RegisterComponent(string componentType, Type componentClass);

    /// <summary>
    /// Gets the registered component type
    /// </summary>
    Type? GetComponentType(string componentType);
}
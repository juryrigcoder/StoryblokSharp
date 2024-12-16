using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using StoryblokSharp.Services.RichText;

/// <summary>
/// Resolves Storyblok components to Blazor components
/// </summary>
public class BlazorComponentResolver : IComponentResolver 
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _componentTypes;

    public BlazorComponentResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _componentTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
    }

    public string ResolveComponent(string componentType, IDictionary<string, object> props)
    {
        if (!SupportsComponent(componentType))
            return string.Empty;

        Type? type = GetComponentType(componentType);
        if (type == null)
            return string.Empty;

        // Create component parameters
        var parameters = new Dictionary<string, object>(props);
        
        // Render the component
        return RenderComponent(type, parameters);
    }

    public bool SupportsComponent(string componentType)
    {
        return _componentTypes.ContainsKey(componentType);
    }

    public void ValidateProps(string componentType, IDictionary<string, object> props)
    {
        // Blazor will handle parameter validation
    }

    public void RegisterComponent(string componentType, Type componentClass)
    {
        if (!typeof(IComponent).IsAssignableFrom(componentClass))
            throw new ArgumentException($"Component class must implement {nameof(IComponent)}", nameof(componentClass));

        _componentTypes[componentType] = componentClass;
    }

    public Type? GetComponentType(string componentType)
    {
        return _componentTypes.TryGetValue(componentType, out Type? type) ? type : null;
    }

    private string RenderComponent(Type componentType, IDictionary<string, object> parameters)
    {
        try
        {
            // Use Blazor's component renderer
            var renderer = _serviceProvider.GetRequiredService<IComponentRenderer>();
            return renderer.RenderComponent(componentType, parameters);
        }
        catch (Exception ex)
        {
            // Log error but don't throw
            System.Diagnostics.Debug.WriteLine($"Error rendering component: {ex.Message}");
            return string.Empty;
        }
    }
}
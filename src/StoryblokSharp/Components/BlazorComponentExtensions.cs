using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using StoryblokSharp.Services.RichText;

namespace StoryblokSharp.Components;

/// <summary>
/// Extension methods for registering Blazor components
/// </summary>
public static class BlazorComponentExtensions
{
    /// <summary>
    /// Adds the Blazor component resolver to the service collection
    /// </summary>
    public static IServiceCollection AddBlazorComponentResolver(this IServiceCollection services)
    {
        services.AddScoped<IComponentResolver, BlazorComponentResolver>();
        return services;
    }

    /// <summary>
    /// Registers a Blazor component for use with Storyblok
    /// </summary>
    /// <typeparam name="TComponent">The type of the Blazor component</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="componentType">The Storyblok component type name</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStoryblokComponent<TComponent>(
        this IServiceCollection services,
        string componentType) where TComponent : class, IComponent
    {
        // Register the component type
        services.AddTransient<TComponent>();
        
        // Get the resolver and register the component
        var resolver = services.BuildServiceProvider()
            .GetRequiredService<IComponentResolver>();
            
        resolver.RegisterComponent(componentType, typeof(TComponent));
        
        return services;
    }
}
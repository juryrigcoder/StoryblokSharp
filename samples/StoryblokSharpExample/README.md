# Storyblok Rich Text Content Processor

This console application processes rich text content from Storyblok's API, demonstrating how to handle nested components and render rich text content. It's built using C# and the Storyblok.Net SDK.

## Features

- Processes all rich text content in Storyblok components
- Handles nested components and content structures
- Renders rich text to HTML
- Supports multiple component types (Introduction, CustomRichText, etc.)
- Provides detailed content structure output
- Includes error handling and logging

## Prerequisites

- .NET 9.0 or later
- A Storyblok access token
- Basic understanding of Storyblok's content structure

## Getting Started

1. Clone the repository
2. Set up your Storyblok access token
3. Build and run the application

```bash
dotnet build
dotnet run -- your_access_token
```

## Program Structure

The program consists of several key components:

### 1. Main Entry Point

```csharp
static async Task Main(string[] args)
{
    if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
    {
        Console.WriteLine("Please provide your Storyblok API token as a command line argument.");
        return;
    }

    var accessToken = args[0];
    var client = await InitializeStoryblokClientAsync(accessToken);
    await DemonstrateFeaturesAsync(client);
}
```

### 2. Service Configuration

The `InitializeStoryblokClientAsync` method sets up all necessary services:

```csharp
// Register utility services
services.AddSingleton<StringBuilderCache>();
services.AddSingleton<IHtmlUtilities, HtmlUtilities>();
services.AddSingleton<IAttributeUtilities, AttributeUtilities>();
services.AddSingleton<IHtmlSanitizer, HtmlSanitizer>();
services.AddSingleton<IRichTextSchema, DefaultRichTextSchema>();

// Register node resolvers
services.AddScoped<MarkNodeResolver>();
services.AddScoped<ImageNodeResolver>();
services.AddScoped<TextNodeResolver>();
services.AddScoped<BlockNodeResolver>();
services.AddScoped<EmojiResolver>();
```

### 3. Rich Text Processing

The application processes rich text content in multiple steps:

1. **Component Discovery**: Recursively finds all components with rich text content
2. **Content Processing**: Extracts and processes the rich text content
3. **HTML Rendering**: Converts the rich text to HTML output

## Usage Example

```csharp
// Initialize the client
var client = await InitializeStoryblokClientAsync("your_access_token");

// Process a story
var parameters = new StoryQueryParameters
{
    Version = "published"
};

var story = await client.GetStoryAsync<StoryContent>("your_story_slug", parameters);

// Process components
foreach (var component in story.Story.Content.Body)
{
    ProcessComponent(component);
}
```

## Key Methods

### ProcessComponent

Processes individual components and their nested content:

```csharp
static void ProcessComponent(JsonElement component)
{
    var componentType = component.GetProperty("component").GetString();
    Console.WriteLine($"\nProcessing component: {componentType}");

    // Process rich text content
    if (component.TryGetProperty("wysiwyg", out var wysiwygElement))
    {
        // Process and render rich text
    }

    // Handle nested components
    if (component.TryGetProperty("items", out var items))
    {
        foreach (var item in items.EnumerateArray())
        {
            ProcessComponent(item);
        }
    }
}
```

### RenderRichTextContent

Renders rich text content to HTML:

```csharp
static string RenderRichTextContent(RichTextField richTextField)
{
    // Set up services
    var serviceCollection = new ServiceCollection();
    // ... configure services ...

    using var serviceProvider = serviceCollection.BuildServiceProvider();
    var renderer = serviceProvider.GetRequiredService<IRichTextRenderer>();

    return renderer.Render(richTextField.ToRichTextContent());
}
```

## Configuration

The application can be configured through the `RichTextOptions`:

```csharp
services.Configure<RichTextOptions>(options =>
{
    options.OptimizeImages = true;
    options.KeyedResolvers = true;
    options.InvalidNodeHandling = InvalidNodeStrategy.Remove;
    options.ImageOptions = new ImageOptimizationOptions
    {
        Loading = "lazy",
        Width = 800,
        Height = 600
    };
});
```

## Error Handling

The application includes comprehensive error handling:

- Component processing errors are caught and logged
- Rich text rendering errors are handled gracefully
- Invalid content is handled according to the InvalidNodeStrategy

## Output Format

The program provides two types of output for each rich text component:

1. **Content Structure**:
   ```
   Type: doc
   Children: 2
     Type: paragraph
     Text: Sample text
     Marks: bold, link
   ```

2. **Rendered HTML**:
   ```html
   <p><strong><a href="...">Sample text</a></strong></p>
   ```

## Troubleshooting

Common issues and solutions:

1. **Missing Content**: Ensure your story slug matches the content you want to process
2. **Rendering Errors**: Check the rich text content structure and mark definitions
3. **Service Errors**: Verify all required services are properly registered

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
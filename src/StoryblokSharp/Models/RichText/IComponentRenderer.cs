/// <summary>
/// Interface for component rendering
/// </summary>
public interface IComponentRenderer
{
    string RenderComponent(object component);

    string RenderComponent(object component, IDictionary<string, object> parameters);
}
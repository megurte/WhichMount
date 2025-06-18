namespace WhichMount.ComponentInjector;

// To safely dispose only plugin components
public interface IPluginComponent
{
    void Release();
}

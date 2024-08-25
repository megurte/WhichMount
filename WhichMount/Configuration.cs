using Dalamud.Configuration;
using System;
using Dalamud.Plugin;

namespace WhichMount;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [NonSerialized] private IDalamudPluginInterface _pluginInterface = null!;
    
    public int Version { get; set; } = 0;
    public bool ShowMountId { get; set; } = false;
    public bool ShowAvailability { get; set; } = false;
    public bool ShowSeats { get; set; } = false;
    public bool ShowHasActions { get; set; } = false;
    public bool ShowHasUniqueMusic { get; set; } = false;
    
    public Configuration(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }
    
    public void Save()
    {
        _pluginInterface.SavePluginConfig(this);
    }
}

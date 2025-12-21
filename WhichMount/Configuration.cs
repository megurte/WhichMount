using Dalamud.Configuration;
using System;
using Dalamud.Plugin;

namespace WhichMount;

[Serializable]
public class Configuration : IPluginConfiguration
{
    [NonSerialized] private IDalamudPluginInterface _pluginInterface = null!;
    
    public int Version { get; set; } = 1;
    
    // ▶ Context Menu Settings
    public bool EnableContextMenu { get; set; } = true;
    public bool ShowAvailability { get; set; } = false;
    public bool ShowMountId { get; set; } = false;
    public bool ShowSeats { get; set; } = false;
    public bool ShowHasActions { get; set; } = false;
    public bool ShowHasUniqueMusic { get; set; } = false;
    public bool ShowMBAvailable { get; set; } = false;
    public bool AddedInPatch { get; set; } = false;

    // ▶ Database Window
    public bool ShowDatabaseMountId { get; set; } = true;
    public bool ShowDatabaseSeats { get; set; } = true;
    public bool ShowDatabaseActions { get; set; } = true;
    public bool ShowDatabaseUniqueBGM { get; set; } = true;
    public bool ShowDatabaseMBAvailable { get; set; } = true;
    public bool ShowDatabasePatch { get; set; } = true;

    // ▶ Tooltip
    public bool ShowTooltip { get; set; } = true;
    public bool ShowUnlockedTooltip { get; set; } = true;
    public bool ShowObtainableTooltip { get; set; } = true;
    public bool ShowDatabaseUnlockStatus { get; set; } = true;

    public Configuration(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
    }
    
    public void Save()
    {
        _pluginInterface.SavePluginConfig(this);
    }
}

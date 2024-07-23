using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using WhichMount;

namespace SamplePlugin;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public void Save()
    {
        WhichMountMain.PluginInterface.SavePluginConfig(this);
    }
}

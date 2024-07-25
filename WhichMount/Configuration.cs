using Dalamud.Configuration;
using System;

namespace WhichMount;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public void Save()
    {
        Service.Interface.SavePluginConfig(this);
    }
}

using System;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace WhichMount;

#pragma warning disable CA1416

public class ConfigWindow : IDisposable
{
    private readonly WhichMountPlugin _whichMountPlugin;
    private readonly Configuration _configuration;
    private bool _showConfig;
    
    public ConfigWindow(IDalamudPluginInterface pluginInterface, WhichMountPlugin whichMount, Configuration configuration)
    {
        _whichMountPlugin = whichMount;
        _configuration = configuration;
        pluginInterface.UiBuilder.Draw += Draw;
        pluginInterface.UiBuilder.OpenConfigUi += () => _showConfig = true;
        Service.CommandManager.AddHandler("/mountsconfig", new CommandInfo((_, _) => _showConfig ^= true)
        {
            HelpMessage = "Open mount search configuration."
        });
    }
    
    private void Draw()
    {
        if (!_showConfig)
            return;

        ImGui.SetNextWindowSize(new Vector2(200, 100), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSizeConstraints(new Vector2(200, 100), new Vector2(250, 100));
        ImGui.Begin($"{_whichMountPlugin.Name} configuration", ref _showConfig, ImGuiWindowFlags.NoCollapse);
        
        var showAvailability = _configuration.ShowAvailability;
        if (ImGui.Checkbox("Show obtainable", ref showAvailability))
        {
            _configuration.ShowAvailability = showAvailability;
            _configuration.Save();
        }

        var showSeats = _configuration.ShowSeats;
        if (ImGui.Checkbox("Show number of seats", ref showSeats))
        {
            _configuration.ShowSeats = showSeats;
            _configuration.Save();
        }
        
        ImGui.End();
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler("/mountsconfig");
    }
}

#pragma warning restore CA1416

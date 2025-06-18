using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using WhichMount.ComponentInjector;

namespace WhichMount.UI;

#pragma warning disable CA1416

public class ConfigWindow : IPluginComponent, IInitializable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly WhichMountPlugin _whichMountPlugin;
    private readonly Configuration _configuration;
    private bool _showConfig;

    public ConfigWindow(IDalamudPluginInterface pluginInterface, WhichMountPlugin whichMount, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _whichMountPlugin = whichMount;
        _configuration = configuration;
    }
    
    public void Initialize()
    {
        _pluginInterface.UiBuilder.Draw += Draw;
    }

    public void Show() => _showConfig = true;
    
    private void Draw()
    {
        if (!_showConfig)
            return;

        ImGui.SetNextWindowSizeConstraints(new Vector2(250, 200), new Vector2(250, 200));
        ImGui.Begin($"{_whichMountPlugin.Name} configuration", ref _showConfig, ImGuiWindowFlags.NoCollapse);
        
        var showMountId = _configuration.ShowMountId;
        if (ImGui.Checkbox("Mount ID", ref showMountId))
        {
            _configuration.ShowMountId = showMountId;
            _configuration.Save();
        }
        
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
        
        var showHasActions = _configuration.ShowHasActions;
        if (ImGui.Checkbox("Has mounts unique actions or not", ref showHasActions))
        {
            _configuration.ShowHasActions = showHasActions;
            _configuration.Save();
        }
        
        var showHasUniqueMusic = _configuration.ShowHasUniqueMusic;
        if (ImGui.Checkbox("Has mounts unique BGM or not", ref showHasUniqueMusic))
        {
            _configuration.ShowHasUniqueMusic = showHasUniqueMusic;
            _configuration.Save();
        }
        
        var addedInPatch = _configuration.AddedInPatch;
        if (ImGui.Checkbox("Added in patch", ref addedInPatch))
        {
            _configuration.AddedInPatch = addedInPatch;
            _configuration.Save();
        }
        
        ImGui.End();
    }

    public void Release()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

#pragma warning restore CA1416

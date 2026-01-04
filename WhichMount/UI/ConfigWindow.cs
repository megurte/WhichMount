using System;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Bindings.ImGui;
using WhichMount.ComponentInjector;

namespace WhichMount.UI;

#pragma warning disable CA1416

[InjectFields]
public class ConfigWindow : DalamudWindow, IPluginComponent, IInitializable
{
    [Inject] private IDalamudPluginInterface _pluginInterface;
    [Inject] private WhichMountPlugin _whichMountPlugin;
    [Inject] private Configuration _configuration;
    private bool _showConfig;
    
    public void Initialize()
    {
        _pluginInterface.UiBuilder.Draw += Draw;
    }

    public void Show() => _showConfig = true;
    
    public override void Draw()
    {
        if (!_showConfig) return;
        DrawWindow();
    }

    private void DrawWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(294, 270), ImGuiCond.Always);
        if (!ImGui.Begin($"{_whichMountPlugin.Name} configuration", ref _showConfig, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize))
            return;

        if (ImGui.BeginTabBar("##WhichMountTabs"))
        {
            if (ImGui.BeginTabItem("Context Menu"))
            {
                DrawContextMenuTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Database Window"))
            {
                DrawDatabaseTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Tooltip"))
            {
                DrawTooltipTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    private void DrawContextMenuTab()
    {
        DrawCheckbox("Enable Context Menu",   _configuration.EnableContextMenu,    v => _configuration.EnableContextMenu = v);
        ImGui.Separator();
        DrawCheckbox("Show Mount ID",         _configuration.ShowMountId,          v => _configuration.ShowMountId = v);
        DrawCheckbox("Show Availability",     _configuration.ShowAvailability,     v => _configuration.ShowAvailability = v);
        DrawCheckbox("Show Seats",            _configuration.ShowSeats,            v => _configuration.ShowSeats = v);
        DrawCheckbox("Show Has Actions",      _configuration.ShowHasActions,       v => _configuration.ShowHasActions = v);
        DrawCheckbox("Show Unique BGM",       _configuration.ShowHasUniqueMusic,   v => _configuration.ShowHasUniqueMusic = v);
        DrawCheckbox("Show Available on MB",  _configuration.ShowMBAvailable,      v => _configuration.ShowMBAvailable = v);
        DrawCheckbox("Show Patch",            _configuration.AddedInPatch,         v => _configuration.AddedInPatch = v);
    }
    
    private void DrawDatabaseTab()
    {
        DrawCheckbox("Show Mount ID",         _configuration.ShowDatabaseMountId,     v => _configuration.ShowDatabaseMountId = v);
        DrawCheckbox("Show Seats",            _configuration.ShowDatabaseSeats,       v => _configuration.ShowDatabaseSeats = v);
        DrawCheckbox("Show Has Actions",      _configuration.ShowDatabaseActions,     v => _configuration.ShowDatabaseActions = v);
        DrawCheckbox("Show Unique BGM",       _configuration.ShowDatabaseUniqueBGM,   v => _configuration.ShowDatabaseUniqueBGM = v);
        DrawCheckbox("Show Available on MB",  _configuration.ShowDatabaseMBAvailable, v => _configuration.ShowDatabaseMBAvailable = v);
        DrawCheckbox("Show Patch",            _configuration.ShowDatabasePatch,       v => _configuration.ShowDatabasePatch = v);
        DrawCheckbox("Show Unlock",           _configuration.ShowDatabaseUnlockStatus,v => _configuration.ShowDatabaseUnlockStatus = v);
    }
    
    private void DrawTooltipTab()
    {
        DrawCheckbox("Enable Mount Tooltip on target",      _configuration.ShowTooltip,           v => _configuration.ShowTooltip = v);
        ImGui.Separator();
        DrawCheckbox("Show mount unlocked on character",    _configuration.ShowUnlockedTooltip,   v => _configuration.ShowUnlockedTooltip = v);
        DrawCheckbox("Show currently obtainable",           _configuration.ShowObtainableTooltip, v => _configuration.ShowObtainableTooltip = v);
    }
        
    private void DrawCheckbox(string label, bool currentValue, Action<bool> setValue)
    {
        var value = currentValue;
        if (ImGui.Checkbox(label, ref value))
        {
            setValue(value);
            _configuration.Save();
        }
    }

    public void Release()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

#pragma warning restore CA1416

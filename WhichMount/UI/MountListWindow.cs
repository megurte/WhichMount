using System;
using System.Collections.Generic;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using WhichMount.Models;

namespace WhichMount.UI;

// TODO search and sorting order

public class MountListWindow : IDisposable
{
    private List<MountModel> _mounts => _cashContainer.MountModels;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly CashContainer _cashContainer;
    private readonly ITextureProvider _textureProvider;
    private readonly IChatGui _chatGui;

    private bool _isOpen = false;

    private bool _showMountId = true;
    private bool _showSeats = true;
    private bool _showActions = true;
    private bool _showUniqueBgm = true;
    private bool _showPatch = true;
    
    public MountListWindow(IDalamudPluginInterface pluginInterface, CashContainer cashContainer, ITextureProvider textureProvider, IChatGui chatGui)
    {
        _pluginInterface = pluginInterface;
        _cashContainer = cashContainer;
        _textureProvider = textureProvider;
        _chatGui = chatGui;
        SortMounts();

        _pluginInterface.UiBuilder.Draw += Draw;
    }

    public void Show() => _isOpen = true;

    private void SortMounts()
    {
        _mounts.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    public void Draw()
    {
        if (!_isOpen)
            return;

        ImGui.SetNextWindowSizeConstraints(
            new Vector2(1400, 150),
            new Vector2(1400, float.MaxValue)
        );
        
        ImGui.SetNextWindowSize(new Vector2(1400, 620), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("WhichMount List", ref _isOpen, ImGuiWindowFlags.NoCollapse))
            return;
        
        ImGui.Text("Show columns:");
        ImGui.SameLine(); ImGui.Checkbox("Mount ID", ref _showMountId);
        ImGui.SameLine(); ImGui.Checkbox("Seats", ref _showSeats);
        ImGui.SameLine(); ImGui.Checkbox("Actions", ref _showActions);
        ImGui.SameLine(); ImGui.Checkbox("Unique BGM", ref _showUniqueBgm);
        ImGui.SameLine(); ImGui.Checkbox("Patch", ref _showPatch);
        
        var columnCount = 3;
        if (_showMountId) columnCount++;
        if (_showSeats) columnCount++;
        if (_showActions) columnCount++;
        if (_showUniqueBgm) columnCount++;
        if (_showPatch) columnCount++;
        
        DrawTable(columnCount);

        ImGui.End();
    }

    private void DrawTable(int columnCount)
    {
        var flags = ImGuiTableFlags.Borders
                    | ImGuiTableFlags.RowBg
                    | ImGuiTableFlags.ScrollY
                    | ImGuiTableFlags.Resizable;    
        
        var tableSize = ImGui.GetContentRegionAvail();

        if (ImGui.BeginTable("MountsTable", columnCount, flags, new Vector2(-1, tableSize.Y)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            SetupTableColumns();

            ImGui.TableHeadersRow();

            foreach (var mount in _mounts)
            {
                // Exclude mounts that doesn't exist in the game 
                if (mount.IconId == 0) continue;
                
                ImGui.TableNextRow();
                
                DrawMountIcon(mount);

                AddTextColumn(mount.Name);

                if (_showMountId)
                    AddTextColumn(mount.Id.ToString());

                if (_showSeats)
                    AddTextColumn(mount.NumberSeats.ToString());

                if (_showActions)
                    AddTextColumn(mount.HasActions ? "Yes" : "No");

                if (_showUniqueBgm)
                    AddTextColumn(mount.HasUniqueMusic ? "Yes" : "No");

                if (_showPatch)
                    AddTextColumn(_cashContainer.GetCachedData(mount.Id, TargetData.Patch));
                
                AddTextResizableColumn(_cashContainer.GetCachedData(mount.Id, TargetData.AcquiredBy));
            }

            ImGui.EndTable();
        }
    }

    private void SetupTableColumns()
    {
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 64);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 250f);
        if (_showMountId) ImGui.TableSetupColumn("Mount ID", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_showSeats) ImGui.TableSetupColumn("Seats", ImGuiTableColumnFlags.WidthFixed, 50);
        if (_showActions) ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_showUniqueBgm) ImGui.TableSetupColumn("Unique BGM", ImGuiTableColumnFlags.WidthFixed, 90);
        if (_showPatch) ImGui.TableSetupColumn("Patch", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableSetupColumn("Acquired By", ImGuiTableColumnFlags.WidthStretch, 1.0f);
    }

    private static void AddTextColumn(string msg)
    {
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(msg);
    }
    
    private static void AddTextResizableColumn(string msg)
    {
        ImGui.TableNextColumn();
        ImGui.TextWrapped(msg);
    }

    private void DrawMountIcon(MountModel mount)
    {
        var icon = GetIcon(mount.IconId).GetWrapOrEmpty();
        ImGui.TableNextColumn();
        ImGui.Image(icon.ImGuiHandle, new Vector2(64, 64));
    }

    private ISharedImmediateTexture GetIcon(uint id, bool hq = false) 
        => _textureProvider.GetFromGameIcon(new GameIconLookup(id, hq));

    public void Dispose()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

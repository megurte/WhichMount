using System;
using System.Collections.Generic;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Excel.Sheets;
using WhichMount.Models;

namespace WhichMount.UI;

public class MountListWindow : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IDataManager _dataManager;
    private readonly List<MountModel> _mounts = new();
    private readonly IChatGui _chatGui;
    private readonly CashContainer _cashContainer;
    
    private bool _isOpen = false;

    private bool _showMountId = true;
    private bool _showSeats = true;
    private bool _showActions = true;
    private bool _showUniqueBgm = true;
    private bool _showPatch = true;
    
    public MountListWindow(IDalamudPluginInterface pluginInterface, IDataManager dataManager, IChatGui chatGui, CashContainer cashContainer)
    {
        _pluginInterface = pluginInterface;
        _dataManager = dataManager;
        _chatGui = chatGui;
        _cashContainer = cashContainer;
        LoadMounts();

        _pluginInterface.UiBuilder.Draw += Draw;
    }

    public void Show()
    {
        _isOpen = true;
    }

    private void LoadMounts()
    {
        var sheet = _dataManager.GetExcelSheet<Mount>();
        
        foreach (var mount in sheet)
        {
            var model = new MountModel(_dataManager, _cashContainer, mount.RowId, "N/A");
            if (model.TryInitData())
            {
                _mounts.Add(model);
                _cashContainer.CacheTableData(model.Id);
            }
        }

        _mounts.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        _chatGui.Print("Inited window");
    }

    public void Draw()
    {
        if (!_isOpen)
            return;

        ImGui.SetNextWindowSize(new Vector2(1200, 620));

        if (!ImGui.Begin("WhichMount List", ref _isOpen, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
            return;
        
        ImGui.Text("Show columns:");
        ImGui.SameLine(); ImGui.Checkbox("Mount ID", ref _showMountId);
        ImGui.SameLine(); ImGui.Checkbox("Seats", ref _showSeats);
        ImGui.SameLine(); ImGui.Checkbox("Actions", ref _showActions);
        ImGui.SameLine(); ImGui.Checkbox("Unique BGM", ref _showUniqueBgm);
        ImGui.SameLine(); ImGui.Checkbox("Patch", ref _showPatch);
        
        var columnCount = 2;
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
        if (ImGui.BeginTable("MountsTable", columnCount,
                             ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY,
                             new Vector2(-1, 540)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 250f);
            if (_showMountId) ImGui.TableSetupColumn("Mount ID", ImGuiTableColumnFlags.WidthFixed, 70);
            if (_showSeats) ImGui.TableSetupColumn("Seats", ImGuiTableColumnFlags.WidthFixed, 50);
            if (_showActions) ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 70);
            if (_showUniqueBgm) ImGui.TableSetupColumn("Unique BGM", ImGuiTableColumnFlags.WidthFixed, 90);
            if (_showPatch) ImGui.TableSetupColumn("Patch", ImGuiTableColumnFlags.WidthFixed, 60);
            ImGui.TableSetupColumn("Acquired By", ImGuiTableColumnFlags.WidthStretch, 1.0f);

            ImGui.TableHeadersRow();

            foreach (var mount in _mounts)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(mount.Name);

                if (_showMountId)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{mount.Id}");
                }

                if (_showSeats)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"{mount.NumberSeats}");
                }

                if (_showActions)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(mount.HasActions ? "Yes" : "No");
                }

                if (_showUniqueBgm)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(mount.HasUniqueMusic ? "Yes" : "No");
                }

                if (_showPatch)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(_cashContainer.GetCachedData(mount.Id, TargetData.Patch));
                }
                
                ImGui.TableNextColumn(); 
                ImGui.TextWrapped(_cashContainer.GetCachedData(mount.Id, TargetData.AcquiredBy));
                
                ImGui.Dummy(new Vector2(1, 10)); 
            }

            ImGui.EndTable();
        }
    }

    public void Dispose()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

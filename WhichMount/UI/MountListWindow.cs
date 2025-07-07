using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Text;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount.UI;

public class MountListWindow : IPluginComponent, IInitializable
{
    private enum SortType
    {
        Alphabet,
        Id,
        Patch,
        Unlocked,
        Locked
    }
    
    private List<MountModel> Mounts => _cashContainer.MountModels;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly CashContainer _cashContainer;
    private readonly ITextureProvider _textureProvider;
    private readonly Configuration _configuration;

    private bool _isOpen = false;
    private string _searchTerm = string.Empty;
    private SortType _sortType = SortType.Alphabet;

    public MountListWindow(
        IDalamudPluginInterface pluginInterface, 
        CashContainer cashContainer, 
        ITextureProvider textureProvider, 
        Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _cashContainer = cashContainer;
        _textureProvider = textureProvider;
        _configuration = configuration;
    }
    
    public void Initialize()
    {
        SortMounts();
        _pluginInterface.UiBuilder.Draw += Draw;
    }

    public void Show() => _isOpen = true;

    private void SortMounts()
    {
        switch (_sortType)
        {
            case SortType.Alphabet:
                Mounts.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                break;
            case SortType.Id:
                Mounts.Sort((a, b) => a.Id.CompareTo(b.Id));
                break;
            case SortType.Patch:
                Mounts.Sort(_cashContainer.PatchSort);
                break;
            case SortType.Unlocked:
                Mounts.Sort((a, b) => b.IsMountUnlocked.CompareTo(a.IsMountUnlocked));
                break;
            case SortType.Locked:
                Mounts.Sort((a, b) => a.IsMountUnlocked.CompareTo(b.IsMountUnlocked));
                break;
        }
    }
    
    private (int unlocked, int total) GetUnlockStats()
    {
        var total = Mounts.Count;
        var unlocked = Mounts.Count(m => m.IsMountUnlocked);
        return (unlocked, total);
    }

    public void Draw()
    {
        if (!_isOpen)
            return;

        ImGui.SetNextWindowSizeConstraints(
            new Vector2(1400, 175),
            new Vector2(1400, float.MaxValue)
        );
        
        ImGui.SetNextWindowSize(new Vector2(1400, 620), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("WhichMount List", ref _isOpen, ImGuiWindowFlags.NoCollapse))
            return;
        
        DrawSearchBar();
        ImGui.SameLine();
        DrawSortDropdown();
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 50);
        DrawUnlockCounter();

        ImGui.Separator();

        ImGui.Text("Show columns:");
        ImGui.SameLine(); DrawCheckbox("Mount ID", () => _configuration.ShowDatabaseMountId, v => _configuration.ShowDatabaseMountId = v);
        ImGui.SameLine(); DrawCheckbox("Seats", () => _configuration.ShowDatabaseSeats, v => _configuration.ShowDatabaseSeats = v);
        ImGui.SameLine(); DrawCheckbox("Actions", () => _configuration.ShowDatabaseActions, v => _configuration.ShowDatabaseActions = v);
        ImGui.SameLine(); DrawCheckbox("Unique BGM", () => _configuration.ShowDatabaseUniqueBGM, v => _configuration.ShowDatabaseUniqueBGM = v);
        ImGui.SameLine(); DrawCheckbox("Patch", () => _configuration.ShowDatabasePatch, v => _configuration.ShowDatabasePatch = v);
        ImGui.SameLine(); DrawCheckbox("Unlocked", () => _configuration.ShowDatabaseUnlockStatus, v => _configuration.ShowDatabaseUnlockStatus = v);
        
        var columnCount = 3;
        if (_configuration.ShowDatabaseMountId) columnCount++;
        if (_configuration.ShowDatabaseSeats) columnCount++;
        if (_configuration.ShowDatabaseActions) columnCount++;
        if (_configuration.ShowDatabaseUniqueBGM) columnCount++;
        if (_configuration.ShowDatabasePatch) columnCount++;
        if (_configuration.ShowDatabaseUnlockStatus) columnCount++;
        
        DrawTable(columnCount);

        ImGui.End();
    }

    private void DrawUnlockCounter()
    {
        var (unlockedCount, totalCount) = GetUnlockStats();
        ImGui.Text($"Mounts Unlocked: {unlockedCount} / {totalCount}");
    }

    private void DrawSearchBar()
    {
        ImGui.Text("Search:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(300);
        ImGui.InputText("##MountSearch", ref _searchTerm, 128);
    }

    private void DrawTable(int columnCount)
    {
        var filtered = FilterTableEntities();
        if (filtered.Count == 0)
        {
            ImGui.TextColored(Constants.RedTextColor, "No mounts found");
            return;
        }
        
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg;
        var rowHeight = 66f;
        var headerHeight = ImGui.GetTextLineHeightWithSpacing();
        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        var visibleCount = filtered.Count;
        var totalHeight = headerHeight + (visibleCount * rowHeight) + spacing;

        if (ImGui.BeginTable("MountsTable", columnCount, flags, new Vector2(-1, totalHeight)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            SetupTableColumns();

            ImGui.TableHeadersRow();
            
            foreach (var mount in filtered)
            {
                // Exclude mounts that doesn't exist in the game 
                if (mount.IconId == 0) continue;
                
                ImGui.TableNextRow();
                
                DrawMountIcon(mount);

                AddTextColumn(mount.Name);

                if (_configuration.ShowDatabaseMountId) AddTextColumn(mount.Id.ToString());
                if (_configuration.ShowDatabaseSeats) AddTextColumn(mount.NumberSeats.ToString());
                if (_configuration.ShowDatabaseActions) AddTextColumn(mount.HasActions ? "Yes" : "No");
                if (_configuration.ShowDatabaseUniqueBGM) AddTextColumn(mount.HasUniqueMusic ? "Yes" : "No");
                if (_configuration.ShowDatabasePatch) AddTextColumn(_cashContainer.GetCachedData(mount.Id, TargetData.Patch));
                if (_configuration.ShowDatabaseUnlockStatus) HandleUnlockTextColumn(mount);
                
                AddTextResizableColumn(_cashContainer.GetCachedData(mount.Id, TargetData.AcquiredBy));
            }

            ImGui.EndTable();
        }
    }

    private static void HandleUnlockTextColumn(MountModel model)
    {
        ImGui.TableNextColumn();
        var color = model.IsMountUnlocked ? Constants.GreenTextColor : Constants.RedTextColor;
        var text = model.IsMountUnlocked ? "Unlocked" : "Locked";
        var cellWidth = ImGui.GetColumnWidth();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorScreenPos();
        var offsetX = (cellWidth - textSize.X) * 0.5f;
        
        ImGui.SetCursorScreenPos(new Vector2(cursorPos.X + offsetX, cursorPos.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, color); 
        ImGui.TextUnformatted(text);                                      
        ImGui.PopStyleColor();
    }

    private List<MountModel> FilterTableEntities()
    {
        return string.IsNullOrWhiteSpace(_searchTerm) 
                   ? Mounts 
                   : Mounts.Where(m => m.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private void SetupTableColumns()
    {
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 64);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 250f);
        if (_configuration.ShowDatabaseMountId) ImGui.TableSetupColumn("Mount ID", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_configuration.ShowDatabaseSeats) ImGui.TableSetupColumn("Seats", ImGuiTableColumnFlags.WidthFixed, 50);
        if (_configuration.ShowDatabaseActions) ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_configuration.ShowDatabaseUniqueBGM) ImGui.TableSetupColumn("Unique BGM", ImGuiTableColumnFlags.WidthFixed, 90);
        if (_configuration.ShowDatabasePatch) ImGui.TableSetupColumn("Patch", ImGuiTableColumnFlags.WidthFixed, 60);
        if (_configuration.ShowDatabaseUnlockStatus) ImGui.TableSetupColumn("Unlocked", ImGuiTableColumnFlags.WidthFixed, 70);
        ImGui.TableSetupColumn("Acquired By", ImGuiTableColumnFlags.WidthFixed, 746f);
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
    
    private void DrawCheckbox(string label, Func<bool> getter, Action<bool> setter)
    {
        var value = getter();
        if (ImGui.Checkbox(label, ref value))
        {
            setter(value);
            _configuration.Save();
        }
    }
    
    private void DrawSortDropdown()
    {
        ImGui.Text("Sort by:");
        ImGui.SameLine();

        ImGui.PushItemWidth(150);
        var sortTypeStr = _sortType.ToString();
        if (ImGui.BeginCombo("##SortType", sortTypeStr))
        {
            foreach (var type in Enum.GetValues<SortType>())
            {
                var isSelected = type == _sortType;
                if (ImGui.Selectable(type.ToString(), isSelected))
                {
                    _sortType = type;
                    SortMounts();
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
    }

    private ISharedImmediateTexture GetIcon(uint id, bool hq = false) 
        => _textureProvider.GetFromGameIcon(new GameIconLookup(id, hq));

    public void Release()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

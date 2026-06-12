using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Bindings.ImGui;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.Utils;
using static WhichMount.UI.MountTableWidgets;

namespace WhichMount.UI;

#pragma warning disable CA1416

[InjectFields]
public class MountDatabaseTab : IInitializable
{
    [Inject] private CacheContainer _cacheContainer;
    [Inject] private ITextureProvider _textureProvider;
    [Inject] private Configuration _configuration;
    [Inject] private MountChatLinks _chatLinks;

    private enum SortType
    {
        Alphabet,
        Id,
        Patch,
        Unlocked,
        Locked,
        MarketBoard,
    }

    private static readonly (string Label, int Major)[] ExpansionFilters =
    [
        ("All", 0),
        (Expansions.ARealmReborn, 2),
        (Expansions.Heavensward, 3),
        (Expansions.Stormblood, 4),
        (Expansions.Shadowbringers, 5),
        (Expansions.Endwalker, 6),
        (Expansions.Dawntrail, 7),
    ];

    private List<MountModel> _mounts = [];
    private string _searchTerm = string.Empty;
    private SortType _sortType = SortType.Alphabet;
    private int _expansionIndex;

    public void Initialize()
    {
        _mounts = _cacheContainer.MountModels.Where(m => m.IconId != 0).ToList();
        SortMounts();
    }

    public void SetSearch(string term)
    {
        _searchTerm = term;
        _expansionIndex = 0;
    }

    public void Draw()
    {
        DrawSearchBar();
        ImGui.SameLine();
        DrawSortDropdown();
        ImGui.SameLine();
        DrawExpansionDropdown();
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 50);
        DrawUnlockCounter();

        ImGui.Separator();

        ImGui.Text("Show columns:");
        ImGui.SameLine(); DrawCheckbox("Mount ID", () => _configuration.ShowDatabaseMountId, v => _configuration.ShowDatabaseMountId = v);
        ImGui.SameLine(); DrawCheckbox("Seats", () => _configuration.ShowDatabaseSeats, v => _configuration.ShowDatabaseSeats = v);
        ImGui.SameLine(); DrawCheckbox("Actions", () => _configuration.ShowDatabaseActions, v => _configuration.ShowDatabaseActions = v);
        ImGui.SameLine(); DrawCheckbox("Unique BGM", () => _configuration.ShowDatabaseUniqueBGM, v => _configuration.ShowDatabaseUniqueBGM = v);
        ImGui.SameLine(); DrawCheckbox("Market Board availability", () => _configuration.ShowDatabaseMBAvailable, v => _configuration.ShowDatabaseMBAvailable = v);
        ImGui.SameLine(); DrawCheckbox("Patch", () => _configuration.ShowDatabasePatch, v => _configuration.ShowDatabasePatch = v);
        ImGui.SameLine(); DrawCheckbox("Unlocked", () => _configuration.ShowDatabaseUnlockStatus, v => _configuration.ShowDatabaseUnlockStatus = v);

        var columnCount = 3;
        if (_configuration.ShowDatabaseMountId) columnCount++;
        if (_configuration.ShowDatabaseSeats) columnCount++;
        if (_configuration.ShowDatabaseActions) columnCount++;
        if (_configuration.ShowDatabaseUniqueBGM) columnCount++;
        if (_configuration.ShowDatabaseMBAvailable) columnCount++;
        if (_configuration.ShowDatabasePatch) columnCount++;
        if (_configuration.ShowDatabaseUnlockStatus) columnCount++;

        DrawTable(columnCount);
    }

    private void SortMounts()
    {
        Comparison<MountModel> byName = (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        int ThenByName(int primary, MountModel a, MountModel b) => primary != 0 ? primary : byName(a, b);

        _mounts.Sort(_sortType switch
        {
            SortType.Id => (a, b) => a.Id.CompareTo(b.Id),
            SortType.Patch => _cacheContainer.PatchSort,
            SortType.Unlocked => (a, b) => ThenByName(b.IsMountUnlocked.CompareTo(a.IsMountUnlocked), a, b),
            SortType.Locked => (a, b) => ThenByName(a.IsMountUnlocked.CompareTo(b.IsMountUnlocked), a, b),
            SortType.MarketBoard => (a, b) => ThenByName(b.IsMarketBoardAvailable.CompareTo(a.IsMarketBoardAvailable), a, b),
            _ => byName,
        });
    }

    private (int unlocked, int total) GetUnlockStats()
    {
        var total = _mounts.Count;
        var unlocked = _mounts.Count(m => m.IsMountUnlocked);
        return (unlocked, total);
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
        var headerHeight = ImGui.GetTextLineHeightWithSpacing();
        var spacing = ImGui.GetStyle().ItemSpacing.Y;
        var visibleCount = filtered.Count;
        var totalHeight = headerHeight + (visibleCount * RowHeight) + spacing;

        if (ImGui.BeginTable("MountsTable", columnCount, flags, new Vector2(-1, totalHeight)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);

            SetupTableColumns();

            DrawTableHeaders();

            var clipper = new ImGuiListClipper();
            clipper.Begin(filtered.Count, RowHeight);
            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DrawTableRow(filtered[i]);
                }
            }
            clipper.End();

            ImGui.EndTable();
        }
    }

    private void DrawTableRow(MountModel mount)
    {
        ImGui.TableNextRow();

        AddIconColumn(_textureProvider, mount, _chatLinks);

        AddTextColumn(mount.Name);

        if (_configuration.ShowDatabaseMountId) AddTextColumn(mount.Id.ToString(), true);
        if (_configuration.ShowDatabaseSeats) AddTextColumn(mount.NumberSeats.ToString(), true);
        if (_configuration.ShowDatabaseActions) AddTextColumn(mount.HasActions ? "Yes" : "No", GetBooleanColor(mount.HasActions), true);
        if (_configuration.ShowDatabaseUniqueBGM) AddTextColumn(mount.HasUniqueMusic ? "Yes" : "No", GetBooleanColor(mount.HasUniqueMusic), true);
        if (_configuration.ShowDatabaseMBAvailable) AddTextColumn(mount.IsMarketBoardAvailable ? "Yes" : "No", GetBooleanColor(mount.IsMarketBoardAvailable), true);
        if (_configuration.ShowDatabasePatch) AddTextColumn(_cacheContainer.GetCachedData(mount.Id, TargetData.Patch), true);
        if (_configuration.ShowDatabaseUnlockStatus) AddUnlockStatusColumn(mount.IsMountUnlocked);

        AddWrappedTextColumn(_cacheContainer.GetCachedData(mount.Id, TargetData.AcquiredBy));
    }

    private List<MountModel> FilterTableEntities()
    {
        var major = ExpansionFilters[_expansionIndex].Major;
        var hasSearch = !string.IsNullOrWhiteSpace(_searchTerm);

        if (major == 0 && !hasSearch)
            return _mounts;

        return _mounts.Where(m => (major == 0 || MatchesExpansion(m, major)) 
                                  && (!hasSearch || m.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)))
                      .ToList();
    }

    private bool MatchesExpansion(MountModel mount, int major)
    {
        var patch = _cacheContainer.GetCachedData(mount.Id, TargetData.Patch);
        var dot = patch.IndexOf('.');
        if (dot <= 0 || !int.TryParse(patch[..dot], out var patchMajor))
            return false;

        // 1.x legacy mounts count as ARR
        return major == 2 ? patchMajor <= 2 : patchMajor == major;
    }

    private void SetupTableColumns()
    {
        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 64);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 250f);
        if (_configuration.ShowDatabaseMountId) ImGui.TableSetupColumn("Mount ID", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_configuration.ShowDatabaseSeats) ImGui.TableSetupColumn("Seats", ImGuiTableColumnFlags.WidthFixed, 50);
        if (_configuration.ShowDatabaseActions) ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 70);
        if (_configuration.ShowDatabaseUniqueBGM) ImGui.TableSetupColumn("Unique BGM", ImGuiTableColumnFlags.WidthFixed, 90);
        if (_configuration.ShowDatabaseMBAvailable) ImGui.TableSetupColumn("MB available", ImGuiTableColumnFlags.WidthFixed, 90);
        if (_configuration.ShowDatabasePatch) ImGui.TableSetupColumn("Patch", ImGuiTableColumnFlags.WidthFixed, 60);
        if (_configuration.ShowDatabaseUnlockStatus) ImGui.TableSetupColumn("Unlocked", ImGuiTableColumnFlags.WidthFixed, 70);
        ImGui.TableSetupColumn("Acquired By", ImGuiTableColumnFlags.WidthFixed, 746f);
    }

    private void DrawTableHeaders()
    {
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);
        ImGui.TableNextColumn(); ImGui.TableHeader("Icon");
        ImGui.TableNextColumn(); ImGui.TableHeader("Name");
        if (_configuration.ShowDatabaseMountId) { ImGui.TableNextColumn(); ImGui.TableHeader("Mount ID"); }
        if (_configuration.ShowDatabaseSeats) AddCenteredHeader("Seats");
        if (_configuration.ShowDatabaseActions) AddCenteredHeader("Actions");
        if (_configuration.ShowDatabaseUniqueBGM) AddCenteredHeader("Unique BGM");
        if (_configuration.ShowDatabaseMBAvailable) AddCenteredHeader("MB available");
        if (_configuration.ShowDatabasePatch) AddCenteredHeader("Patch");
        if (_configuration.ShowDatabaseUnlockStatus) AddCenteredHeader("Unlocked");
        ImGui.TableNextColumn(); ImGui.TableHeader("Acquired By");
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

    private void DrawExpansionDropdown()
    {
        ImGui.Text("Expansion:");
        ImGui.SameLine();

        ImGui.SetNextItemWidth(150);
        if (ImGui.BeginCombo("##ExpansionFilter", ExpansionFilters[_expansionIndex].Label))
        {
            for (var i = 0; i < ExpansionFilters.Length; i++)
            {
                var isSelected = i == _expansionIndex;
                if (ImGui.Selectable(ExpansionFilters[i].Label, isSelected))
                {
                    _expansionIndex = i;
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
    }
}

#pragma warning restore CA1416

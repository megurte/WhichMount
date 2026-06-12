using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Bindings.ImGui;
using WhichMount.ComponentInjector;

namespace WhichMount.UI;

#pragma warning disable CA1416

[InjectFields]
public class MountListWindow : DalamudWindow, IPluginComponent, IInitializable
{
    [Inject] private IDalamudPluginInterface _pluginInterface;
    [Inject] private MountDatabaseTab _databaseTab;
    [Inject] private MountTracksTab _tracksTab;
    [Inject] private MountChatLinks _chatLinks;

    private bool _isOpen;
    private bool _selectDatabaseTab;

    public void Initialize()
    {
        _pluginInterface.UiBuilder.Draw += Draw;
        _chatLinks.OpenDatabaseRequested = OpenDatabase;
    }

    public void Show() => _isOpen = true;

    public void OpenDatabase(string searchTerm)
    {
        _databaseTab.SetSearch(searchTerm);
        _selectDatabaseTab = true;
        _isOpen = true;
    }

    public override void Draw()
    {
        if (!_isOpen)
            return;

        ImGui.SetNextWindowSizeConstraints(new Vector2(1400, 175), new Vector2(1400, float.MaxValue));
        ImGui.SetNextWindowSize(new Vector2(1400, 620), ImGuiCond.FirstUseEver);

        if (!ImGui.Begin("WhichMount List", ref _isOpen, ImGuiWindowFlags.NoCollapse))
            return;

        if (ImGui.BeginTabBar("##MountListTabs"))
        {
            var databaseTabFlags = _selectDatabaseTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            _selectDatabaseTab = false;

            if (ImGui.BeginTabItem("Mount Database", databaseTabFlags))
            {
                _databaseTab.Draw();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Mounts Track"))
            {
                _tracksTab.Draw();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
    }

    public void Release()
    {
        _pluginInterface.UiBuilder.Draw -= Draw;
    }
}

#pragma warning restore CA1416

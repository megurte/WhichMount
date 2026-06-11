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
public class MountTracksTab
{
    [Inject] private MountTrackContainer _trackContainer;
    [Inject] private CashContainer _cashContainer;
    [Inject] private ITextureProvider _textureProvider;

    public void Draw()
    {
        foreach (var track in _trackContainer.Tracks)
        {
            DrawTrack(track);
        }
    }

    private void DrawTrack(MountTrackModel track)
    {
        var isOpen = ImGui.CollapsingHeader($"{track.Name}##Track{track.Reward.Id}");
        DrawCompletionMark(track);

        if (!isOpen)
            return;

        ImGui.Spacing();
        DrawMainReward(track);
        ImGui.Spacing();
        DrawMembersTable(track);
        ImGui.Spacing();
    }

    private static void DrawCompletionMark(MountTrackModel track)
    {
        var headerMin = ImGui.GetItemRectMin();
        var headerMax = ImGui.GetItemRectMax();
        var markX = headerMin.X + ImGui.GetTreeNodeToLabelSpacing() + ImGui.CalcTextSize(track.Name).X + 18;
        var center = new Vector2(markX, (headerMin.Y + headerMax.Y) * 0.5f);
        var drawList = ImGui.GetWindowDrawList();
        const float radius = 8f;

        if (track.IsRewardUnlocked)
        {
            drawList.AddCircleFilled(center, radius, ImGui.GetColorU32(Constants.GreenTextColor));

            var checkLeft = new Vector2(center.X - 3.5f, center.Y + 0.5f);
            var checkMid = new Vector2(center.X - 1f, center.Y + 3f);
            var checkRight = new Vector2(center.X + 4f, center.Y - 3f);
            var checkColor = ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 1f));
            drawList.AddLine(checkLeft, checkMid, checkColor, 2f);
            drawList.AddLine(checkMid, checkRight, checkColor, 2f);
        }
        else
        {
            drawList.AddCircle(center, radius, ImGui.GetColorU32(ImGuiCol.TextDisabled), 0, 1.5f);
        }
    }

    private void DrawMainReward(MountTrackModel track)
    {
        var reward = track.Reward;

        ImGui.Text("Main Reward");

        DrawMountIcon(_textureProvider, reward.IconId);
        ImGui.SameLine();
        ImGui.BeginGroup();
        ImGui.TextUnformatted(reward.Name);
        ImGui.TextColored(GetBooleanColor(reward.IsMountUnlocked), reward.IsMountUnlocked ? "Unlocked" : "Locked");
        ImGui.EndGroup();

        ImGui.SameLine(0, 40);
        var collected = track.CollectedCount;
        var total = track.Members.Count;
        ImGui.TextColored(GetBooleanColor(collected == total), $"Collected: {collected} / {total}");
    }

    private void DrawMembersTable(MountTrackModel track)
    {
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg;
        if (!ImGui.BeginTable($"##TrackMounts{track.Reward.Id}", 4, flags))
            return;

        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 64);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 250f);
        ImGui.TableSetupColumn("Unlocked", ImGuiTableColumnFlags.WidthFixed, 90);
        ImGui.TableSetupColumn("Acquired By", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        foreach (var mount in track.Members)
        {
            ImGui.TableNextRow();

            AddIconColumn(_textureProvider, mount.IconId);
            AddTextColumn(mount.Name);
            AddUnlockStatusColumn(mount.IsMountUnlocked);
            AddWrappedTextColumn(_cashContainer.GetCachedData(mount.Id, TargetData.AcquiredBy));
        }

        ImGui.EndTable();
    }
}

#pragma warning restore CA1416

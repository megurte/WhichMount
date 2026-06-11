using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Bindings.ImGui;
using WhichMount.Utils;

namespace WhichMount.UI;

public static class MountTableWidgets
{
    public const float IconSize = 64f;
    public const float RowHeight = 66f;

    public static Vector4 GetBooleanColor(bool condition)
    {
        return condition ? Constants.GreenTextColor : Constants.RedTextColor;
    }

    public static void DrawMountIcon(ITextureProvider textureProvider, uint iconId)
    {
        var icon = textureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty();
        ImGui.Image(icon.Handle, new Vector2(IconSize, IconSize));
    }

    public static void AddIconColumn(ITextureProvider textureProvider, uint iconId)
    {
        ImGui.TableNextColumn();
        DrawMountIcon(textureProvider, iconId);
    }

    public static void AddTextColumn(string msg, bool centerAlign = false)
    {
        ImGui.TableNextColumn();
        if (centerAlign)
        {
            CenterInColumn(msg);
        }
        ImGui.TextUnformatted(msg);
    }

    public static void AddTextColumn(string msg, Vector4 color, bool centerAlign = false)
    {
        ImGui.TableNextColumn();
        if (centerAlign)
        {
            CenterInColumn(msg);
        }
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(msg);
        ImGui.PopStyleColor();
    }

    public static void AddWrappedTextColumn(string msg)
    {
        ImGui.TableNextColumn();
        ImGui.TextWrapped(msg);
    }

    public static void AddUnlockStatusColumn(bool unlocked)
    {
        AddTextColumn(unlocked ? "Unlocked" : "Locked", GetBooleanColor(unlocked), true);
    }

    public static void AddCenteredHeader(string label)
    {
        ImGui.TableNextColumn();
        var cellWidth = ImGui.GetColumnWidth();
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.TableHeader($"##{label}");
        var textSize = ImGui.CalcTextSize(label);
        var textPos = new Vector2(cursor.X + ((cellWidth - textSize.X) * 0.5f), cursor.Y);
        ImGui.GetWindowDrawList().AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);
    }

    private static void CenterInColumn(string text)
    {
        var cellWidth = ImGui.GetColumnWidth();
        var textSize = ImGui.CalcTextSize(text);
        var cursorPos = ImGui.GetCursorScreenPos();
        var offsetX = (cellWidth - textSize.X) * 0.5f;
        ImGui.SetCursorScreenPos(new Vector2(cursorPos.X + offsetX, cursorPos.Y));
    }
}

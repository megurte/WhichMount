using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace WhichMount.UI;

public abstract class DalamudWindow : Window, IDisposable
{
    protected DalamudWindow() : base("DalamudWindow", ImGuiWindowFlags.NoFocusOnAppearing, false) { }

    public void Open(bool focus = true)
    {
        IsOpen = true;
        Collapsed = false;

        if (focus)
        {
            BringToFront();
        }
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void Toggle(bool focus = true)
    {
        if (!IsOpen)
            Open(focus);
        else
            Close();
    }
    
    public virtual void Dispose()
    {
        Close();
    }
}

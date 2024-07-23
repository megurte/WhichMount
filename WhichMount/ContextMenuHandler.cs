using System;
using Dalamud.Game;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;

namespace WhichMount;
 
public class ContextMenuHandler : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IContextMenu _contextMenu;
    private readonly IChatGui _chatGui;

    public ContextMenuHandler(IDalamudPluginInterface pluginInterface, IContextMenu contextMenu, IChatGui chatGui)
    {
        _pluginInterface = pluginInterface;
        _contextMenu = contextMenu;
        _chatGui = chatGui;
    }

    private void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
    {
        if (!_pluginInterface.UiBuilder.ShouldModifyUi || !IsMenuValid(menuOpenedArgs))
        {
            return;
        }

        menuOpenedArgs.AddMenuItem(new MenuItem
        {
            PrefixChar = 'M',
            Name = "Search Mount",
            OnClicked = CheckMount,
        });
    }

    private void CheckMount(IMenuItemClickedArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }

        var name = menuTargetDefault.TargetName; 

        if (name != null)
        {
            var mountId = 1;

            if (mountId != 0)
            {
                var mountName = GetMountNameById(mountId);
                _chatGui.Print($"Current Mount: {mountName}");
            }
            else
            {
                _chatGui.Print("No mount is currently active.");
            }
        }
    }
    
    private string GetMountNameById(int mountId)
    {

        return "Sample Mount Name";
    }
    
    private static bool IsMenuValid(IMenuArgs menuOpenedArgs)
    {
        if (menuOpenedArgs.Target is not MenuTargetDefault menuTargetDefault)
        {
            return false;
        }

        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (menuOpenedArgs.AddonName)
        {
            case null: // Nameplate/Model menu
            case "LookingForGroup":
            case "PartyMemberList":
            case "FriendList":
            case "FreeCompany":
            case "SocialList":
            case "ContactList":
            case "ChatLog":
            case "_PartyList":
            case "LinkShell":
            case "CrossWorldLinkshell":
            case "ContentMemberList": // Eureka/Bozja/...
            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty;
        }

        return false;
    }
    
    public void Enable()
    {
        _contextMenu.OnMenuOpened += OnOpenContextMenu;
    }

    public void Disable()
    {
        _contextMenu.OnMenuOpened -= OnOpenContextMenu;
    }

    public void Dispose()
    {
        _contextMenu.OnMenuOpened -= OnOpenContextMenu;
    }
}

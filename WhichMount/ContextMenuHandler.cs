using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HtmlAgilityPack;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount;
 
#pragma warning disable CA1416

public enum TargetData
{
    Name = 0,
    Icon = 1,
    AcquisitionType = 2,
    AcquiredBy = 3,
    Seats = 4,
    IsObtainable = 5
}

public class ContextMenuHandler
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IChatGui _chatGui;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly IContextMenu _contextMenu;
    private readonly Configuration _configuration;

    private HashSet<Mount> _uniqueMusicMounts = new();

    public ContextMenuHandler(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui, 
        IDataManager dataManager, 
        IObjectTable objectTable, 
        IContextMenu contextMenu,
        Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _chatGui = chatGui;
        _dataManager = dataManager;
        _objectTable = objectTable;
        _contextMenu = contextMenu;
        _configuration = configuration;
        _contextMenu.OnMenuOpened += OnOpenContextMenu;
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
            OnClicked = FetchAndDisplayMountInfo 
        });
    }

    private unsafe void FetchAndDisplayMountInfo (IMenuItemClickedArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault) return;
        
        var targetCharacter = _objectTable.SearchById(menuTargetDefault.TargetObjectId);

        if (targetCharacter == null)
        {
            return;
        }
        
        var mountId = ((Character*) targetCharacter.Address)->Mount.MountId;
        
        if (mountId == 0)
        {
            _chatGui.Print("No mount is currently active.");
            return;
        }
        
        var mountModel = new MountModel(_dataManager, mountId, targetCharacter.Name.ToString());

        if (!mountModel.TryInitData())
        {
            _chatGui.Print("Cannot find mount");
            return;
        }

        var view = new ChatView(_chatGui, _configuration);
        view.BindModel(mountModel);
    }
    
    private bool IsMenuValid(IMenuArgs menuOpenedArgs)
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
            case "ContentMemberList":
            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty 
                       && CommonUtils.Validation.IsWorldValid(menuTargetDefault.TargetHomeWorld.Id, _dataManager);
        }

        return false;
    }
    
    public void Dispose()
    {
        _contextMenu.OnMenuOpened -= OnOpenContextMenu;
    }
}

#pragma warning restore CA1416

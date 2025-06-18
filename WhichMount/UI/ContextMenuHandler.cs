using System;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.Utils;

namespace WhichMount.UI;
 
#pragma warning disable CA1416

public class ContextMenuHandler : IPluginComponent, IInitializable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IChatGui _chatGui;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly IContextMenu _contextMenu;
    private readonly Configuration _configuration;
    private readonly CashContainer _cashContainer;

    public ContextMenuHandler(
        IDalamudPluginInterface pluginInterface,
        IChatGui chatGui, 
        IDataManager dataManager, 
        IObjectTable objectTable, 
        IContextMenu contextMenu,
        Configuration configuration,
        CashContainer cashContainer)
    {
        _pluginInterface = pluginInterface;
        _chatGui = chatGui;
        _dataManager = dataManager;
        _objectTable = objectTable;
        _contextMenu = contextMenu;
        _configuration = configuration;
        _cashContainer = cashContainer;
    }
    
    public void Initialize()
    {
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
            _chatGui.Print("Character not found");
            return;
        }
        
        var mountId = ((Character*) targetCharacter.Address)->Mount.MountId;
        
        if (mountId == 0)
        {
            _chatGui.Print("No mount is currently active.");
            return;
        }
        
        var mountModel = new MountModel(_dataManager, _cashContainer, mountId, targetCharacter.Name.ToString());

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
            case "BeginnerChatList":
                return menuTargetDefault.TargetName != string.Empty 
                       && CommonUtils.Validation.IsWorldValid(menuTargetDefault.TargetHomeWorld.RowId, _dataManager);
            case "BlackList":
            case "MuteList":
                return menuTargetDefault.TargetName != string.Empty;
        }

        return false;
    }
    
    public void Release()
    {
        _contextMenu.OnMenuOpened -= OnOpenContextMenu;
    }
}

#pragma warning restore CA1416

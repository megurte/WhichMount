using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

public class ContextMenuHandler
{
    private const string WikiUrl = "https://ffxiv.consolegameswiki.com/wiki/Mounts";
    private const int WebTableIndex = 7;
    
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
        
        var mountModel = new MountModel(_dataManager, mountId);

        if (!mountModel.TryInitMountData())
        {
            _chatGui.Print("Cannot find mount");
            return;
        }
        
        _chatGui.Print($"{targetCharacter.Name}'s mount: {mountModel.Name}");
        
        if (_configuration.ShowMountId)
            _chatGui.Print($"Mount ID: {mountId}");
        if (_configuration.ShowSeats) 
            _chatGui.Print($"Number of seats: {mountModel.NumberSeats}");
        if (_configuration.ShowHasActions)
            _chatGui.Print($"Has actions: {(mountModel.HasActions ? "Yes" : "No")}");
        if (_configuration.ShowHasUniqueMusic)
            _chatGui.Print($"Has uniqueMusic: {(mountModel.HasUniqueMusic ? "Yes" : "No")}");
        if (_configuration.ShowMusic)
            _chatGui.Print($"Music: {mountModel.MusicName}");
                
        //GetMountAcquiredByAsync(mountName);
    }
    
    private async void GetMountAcquiredByAsync(string mountName)
    {
        var requestedStrings = await GetMountAcquiredBy(mountName, _configuration);
        foreach (var item in requestedStrings)
        {
            _chatGui.Print(item);
        }
    }
    
    private static async Task<List<string>> GetMountAcquiredBy(string mountName, Configuration configuration)
    {
        var requestResult = new List<string>();
        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(WikiUrl);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(response);

        var mountTables = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'sortable')]");

        if (mountTables == null || mountTables.Count < 8)
        {
            requestResult.Add("mount table not found");
            return requestResult;
        }

        var mountTable = mountTables[WebTableIndex];
        var mountNodes = mountTable.SelectNodes(".//tr");

        if (mountNodes == null || mountNodes.Count == 0)
        {
            requestResult.Add("mount rows not found");
            return requestResult;
        }

        foreach (var mountNode in mountNodes.Skip(1))
        {
            var cells = mountNode.SelectNodes("td");

            if (cells is {Count: >= 3})
            {
                var nameNode = cells[0].SelectSingleNode(".//a");
                
                if (nameNode != null)
                {
                    var name = nameNode.InnerText.Trim();
                    
                    if (string.Equals(name, mountName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (configuration.ShowAvailability)
                        {
                            var availability = cells[5].InnerText.Trim();
                            var availabilityText = availability == "1"
                                                       ? "This mount is currently obtainable"
                                                       : "This mount is NOT currently obtainable";
                            requestResult.Add($"{availabilityText}");
                        }

                        if (configuration.ShowSeats)
                        {
                            var seats = cells[4].InnerText.Trim();
                            requestResult.Add($"Number of seats: {seats}");
                        }
                        var acquiredBy = cells[3].InnerText.Trim();
                        requestResult.Add($"Source: {CommonUtils.WikiStringConverter.ConvertString(acquiredBy)}");
                        return requestResult;
                    }
                }
            }
        }
        
        requestResult.Add("source information not found");
        return requestResult;
    }
    
    private string GetMountNameById(uint mountId)
    {
        var mountData = _dataManager.GetExcelSheet<Mount>()!.GetRow(mountId);
        
        if (mountData != null)
        {
            var name = CommonUtils.SeStringConverter
                            .ParseSeStringLumina(_dataManager.GetExcelSheet<Mount>()!.GetRow(mountData.RowId)?.Singular);
            return name;
        }

        return "Mount not found";
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

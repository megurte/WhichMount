using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Game.Gui.ContextMenu;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HtmlAgilityPack;
using Lumina.Excel.GeneratedSheets;

namespace WhichMount;
 
#pragma warning disable CA1416

public class ContextMenuHandler
{
    private const string WikiUrl = "https://ffxiv.consolegameswiki.com/wiki/Mounts";
    private const int WebTableIndex = 7;
    
    private void OnOpenContextMenu(IMenuOpenedArgs menuOpenedArgs)
    {
        if (!Service.Interface.UiBuilder.ShouldModifyUi || !IsMenuValid(menuOpenedArgs))
        {
            return;
        }

        menuOpenedArgs.AddMenuItem(new MenuItem
        {
            PrefixChar = 'M',
            Name = "Search Mount",
            OnClicked = CheckMount
        });
    }

    private unsafe void CheckMount(IMenuItemClickedArgs args)
    {
        if (args.Target is not MenuTargetDefault menuTargetDefault)
        {
            return;
        }

        var targetCharacter = Service.ObjectTable.SearchById(menuTargetDefault.TargetObjectId);

        if (targetCharacter != null)
        {
            var mountId = ((Character*) targetCharacter.Address)->Mount.MountId;
            
            if (mountId != 0)
            {
                var mountName = GetMountNameById(mountId);
                
                Service.ChatGui.Print($"{targetCharacter.Name}'s Mount Name: {mountName}");
                GetMountAcquiredByAsync(mountName);
            }
            else
            {
                Service.ChatGui.Print("No mount is currently active.");
            }
        }
    }
    
    private async void GetMountAcquiredByAsync(string mountName)
    {
        var acquiredBy = await GetMountAcquiredBy(mountName);
        Service.ChatGui.Print($"Source: {acquiredBy}");
    }
    
    private static async Task<string> GetMountAcquiredBy(string mountName)
    {
        var url = WikiUrl;
        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(url);
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(response);

        var mountTables = htmlDocument.DocumentNode.SelectNodes("//table[contains(@class, 'sortable')]");

        if (mountTables == null || mountTables.Count < 8)
        {
            return "Mount table not found";
        }

        var mountTable = mountTables[WebTableIndex];
        var mountNodes = mountTable.SelectNodes(".//tr");

        if (mountNodes == null || mountNodes.Count == 0)
        {
            return "Mount rows not found";
        }

        foreach (var mountNode in mountNodes.Skip(1))
        {
            var cells = mountNode.SelectNodes("td");

            if (cells != null && cells.Count >= 3)
            {
                var nameNode = cells[0].SelectSingleNode(".//a");
                if (nameNode != null)
                {
                    var name = nameNode.InnerText.Trim();
                    if (string.Equals(name, mountName, StringComparison.OrdinalIgnoreCase))
                    {
                        var acquiredBy = cells[3].InnerText.Trim();
                        return Utils.WikiStringConverter.ConvertString(acquiredBy);
                    }
                }
            }
        }

        return "Acquired information not found";
    }
    
    private string GetMountNameById(uint mountId)
    {
        var mountData = Service.DataManager.GetExcelSheet<Mount>()!.GetRow(mountId);
        
        if (mountData != null)
        {
            var name = Utils.SeStringConverter
                            .ParseSeStringLumina(Service.DataManager.GetExcelSheet<Mount>()!.GetRow(mountData.RowId)?.Singular);
            return name;
        }

        return "Mount not found";
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
            case "ContentMemberList":
            case "BlackList":
                return menuTargetDefault.TargetName != string.Empty;
        }

        return false;
    }
    
    public void Enable()
    {
        Service.ContextMenu.OnMenuOpened += OnOpenContextMenu;
    }

    public void Dispose()
    {
        Service.ContextMenu.OnMenuOpened -= OnOpenContextMenu;
    }
}

#pragma warning restore CA1416

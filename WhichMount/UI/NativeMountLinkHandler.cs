using System;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using WhichMount.ComponentInjector;
using WhichMount.Models;

namespace WhichMount.UI;

[InjectFields]
public unsafe class NativeMountLinkHandler : IPluginComponent, IInitializable
{
    private const uint HqItemIdOffset = 1_000_000;

    [Inject] private IContextMenu _contextMenu;
    [Inject] private Configuration _configuration;
    [Inject] private MountItemMap _itemMap;
    
    public Action<uint>? OpenMountRequested { get; set; }

    public void Initialize()
    {
        _contextMenu.OnMenuOpened += OnMenuOpened;
    }

    public bool CanShare(uint mountId) => _itemMap.TryGetItem(mountId, out _);

    public bool TryShare(MountModel mount)
    {
        if (!_itemMap.TryGetItem(mount.Id, out var itemId))
            return false;

        ShareItem(itemId);
        return true;
    }

    public void ShareItem(uint itemId)
    {
        var utf8 = Utf8String.FromSequence(SeString.CreateItemLink(itemId, false).Encode());
        UIModule.Instance()->ProcessChatBoxEntry(utf8);
        utf8->Dtor(true);
    }

    public bool TryCreateItemLink(MountModel mount, out SeString link)
    {
        if (_itemMap.TryGetItem(mount.Id, out var itemId))
        {
            link = SeString.CreateItemLink(itemId, false);
            return true;
        }

        link = null!;
        return false;
    }

    private void OnMenuOpened(IMenuOpenedArgs args)
    {
        if (!_configuration.EnableContextMenu)
            return;

        if (args.AddonName != "ChatLog" || args.Target is not MenuTargetDefault target || !string.IsNullOrEmpty(target.TargetName))
            return;

        var itemId = AgentChatLog.Instance()->ContextItemId;
        if (itemId >= HqItemIdOffset)
            itemId -= HqItemIdOffset;

        if (!_itemMap.TryGetMount(itemId, out var mountId) && !_itemMap.TryGetMountByTotem(itemId, out mountId))
            return;

        args.AddMenuItem(new MenuItem
        {
            PrefixChar = 'M',
            Name = "Open in WhichMount",
            OnClicked = _ => OpenMountRequested?.Invoke(mountId)
        });
    }

    public void Release()
    {
        _contextMenu.OnMenuOpened -= OnMenuOpened;
    }
}

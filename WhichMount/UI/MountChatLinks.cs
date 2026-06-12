using System;
using System.Linq;
using Dalamud.Game.Text.SeStringHandling;
using WhichMount.ComponentInjector;
using WhichMount.Models;

namespace WhichMount.UI;

[InjectFields]
public class MountChatLinks : IInitializable
{
    [Inject] private NativeMountLinkHandler _native;
    [Inject] private MarkerMountLinkHandler _marker;
    [Inject] private CacheContainer _cacheContainer;
    
    public Action<string>? OpenDatabaseRequested { get; set; }
    
    public void Initialize()
    {
        _native.OpenMountRequested = OpenInDatabase;
        _marker.OpenMountRequested = OpenInDatabase;
    }

    public bool IsNativeLink(MountModel mount) => _native.CanShare(mount.Id);

    public void ShareToChat(MountModel mount)
    {
        if (!_native.TryShare(mount))
            _marker.ShareToChat(mount);
    }

    public void ShareItemToChat(uint itemId) => _native.ShareItem(itemId);
    
    public SeString BuildChatLink(MountModel mount) => _native.TryCreateItemLink(mount, out var link) ? link : _marker.BuildLocalLink(mount);

    private void OpenInDatabase(uint mountId)
    {
        var mount = _cacheContainer.MountModels.FirstOrDefault(m => m.Id == mountId);
        if (mount != null)
            OpenDatabaseRequested?.Invoke(mount.Name);
    }
}

using System.Collections.Generic;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using WhichMount.ComponentInjector;

namespace WhichMount.Models;

[InjectFields]
public class MountItemMap : IPluginComponent, IInitializable
{
    private const uint MountUnlockActionId = 1322;

    [Inject] private IDataManager _dataManager;
    [Inject] private IPluginLog _pluginLog;

    private readonly Dictionary<uint, uint> _itemByMount = new();
    private readonly Dictionary<uint, uint> _mountByItem = new();
    private readonly Dictionary<uint, uint> _mountByTotem = new();

    public void Initialize()
    {
        foreach (var track in MountTracks.All)
        {
            foreach (var member in track.Members)
            {
                if (member.HasTotem)
                    _mountByTotem.TryAdd(member.TotemItemId, member.MountId);
            }
        }

        foreach (var item in _dataManager.GetExcelSheet<Item>())
        {
            TryAddToDictionaries(item);
        }

        _pluginLog.Information($"Mount item map: {_itemByMount.Count} mounts have an unlock item");
    }

    private void TryAddToDictionaries(Item item)
    {
        if (item.ItemAction.RowId == 0)
            return;

        var action = item.ItemAction.Value;
        if (action.Action.RowId != MountUnlockActionId)
            return;

        var mountId = (uint)action.Data[0];
        _itemByMount.TryAdd(mountId, item.RowId);
        _mountByItem.TryAdd(item.RowId, mountId);
    }

    public bool TryGetItem(uint mountId, out uint itemId) => _itemByMount.TryGetValue(mountId, out itemId);

    public bool TryGetMount(uint itemId, out uint mountId) => _mountByItem.TryGetValue(itemId, out mountId);

    public bool TryGetMountByTotem(uint itemId, out uint mountId) => _mountByTotem.TryGetValue(itemId, out mountId);

    public void Release()
    {
        _itemByMount.Clear();
        _mountByItem.Clear();
        _mountByTotem.Clear();
    }
}

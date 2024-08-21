using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using WhichMount.Utils;

namespace WhichMount.Models;

public class MountModel {
    private readonly IDataManager _dataManager;
    private readonly uint id;
    private Mount? _mountItem;
    private HashSet<Mount> _uniqueMusicMounts
    {
        get
        {
            return _dataManager.Excel.GetSheet<Mount>()!
                               .Where(mount => mount.RideBGM != null)
                               .GroupBy(mount => mount.RideBGM.Row)
                               .Where(group => group.Count() == 1)
                               .Select(group => group.First())
                               .ToHashSet();
        }
    }

    private Dictionary<uint, BGM> _mountMusics
    {
        get
        {
            if (_mountMusics == null)
            {
                return _dataManager.Excel.GetSheet<BGM>()!.ToDictionary(k => k.RowId, v => v);
            }

            return _mountMusics;
        }
    }

    public MountModel(IDataManager dataManager, uint id)
    {
        _dataManager = dataManager;
        this.id = id;
    }

    public bool TryInitMountData()
    {
        _mountItem = GetMountObject(id);
        return _mountItem != null;
    }
    
    public Mount? GetMountObject(uint mountId)
    {
        return _dataManager.GetExcelSheet<Mount>()!.GetRow(mountId);
    }
    
    public string Name => _mountItem.Singular.ToDalamudString().ToTitleCase();
    public int NumberSeats => _mountItem.ExtraSeats + 1;
    public bool HasActions => _mountItem.MountAction.Row != 0;
    public bool HasUniqueMusic => _uniqueMusicMounts.Contains(_mountItem);

    //TODO
    /*public string MusicName()
    {
        _mountMusics.TryGetValue(_mountItem.RideBGM.Row, out var bgm);
        return bgm.
    }*/
    public string MusicName => _mountItem.RideBGM.Value.ToString();
}

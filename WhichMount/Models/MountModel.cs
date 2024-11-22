using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using WhichMount.Utils;

namespace WhichMount.Models;

public enum TargetData
{
    Name = 0,
    Id = 1,
    Icon = 2,
    AcquisitionType = 3,
    AcquiredBy = 4,
    Seats = 5,
    IsObtainable = 6,
    CashShop = 7,
    Patch = 8,
}

public class MountModel {
    
    public uint Id { get; }
    public string Owner { get; }
    public string Name => _mountItem.Singular.ToDalamudString().ToTitleCase();
    public int NumberSeats => _mountItem.ExtraSeats + 1;
    public bool HasActions => _mountItem.MountAction.RowId != 0;
    public bool HasUniqueMusic => _uniqueMusicMounts.Contains(_mountItem);
    
    private const string ResourceName = "WhichMount.Resources.MountList.csv";
    
    private readonly IDataManager _dataManager;
    private Mount _mountItem;
    private HashSet<Mount> _uniqueMusicMounts
    {
        get
        {
            return _dataManager.Excel.GetSheet<Mount>()!
                               .GroupBy(mount => mount.RideBGM.RowId)
                               .Where(group => group.Count() == 1)
                               .Select(group => group.First())
                               .ToHashSet();
        }
    }

    public MountModel(IDataManager dataManager, uint id, string owner)
    {
        _dataManager = dataManager;
        Id = id;
        Owner = owner;
    }

    public bool TryInitData()
    {
        _mountItem = GetMountObject(Id);
        return !Name.IsNullOrEmpty();
    }
    
    public Mount GetMountObject(uint mountId)
    {
        return _dataManager.GetExcelSheet<Mount>()!.GetRow(mountId);
    }
    
    public string GetDataByTable(TargetData targetData)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
    
        if (stream is not {CanRead: true})
        {
            return "Stream is not available or not readable.";
        }

        using var reader = new StreamReader(stream);

        while (reader.ReadLine() is { } line)
        {
            var columns = line.Split('|');

            if (!uint.TryParse(columns[(int)TargetData.Id], out var mountId) || mountId != Id)
            {
                continue;
            }

            if (targetData < 0 || (int)targetData >= columns.Length)
            {
                return $"Invalid targetData index: {targetData}";
            }

            return columns[(int)targetData];
        }

        return "Data not found.";
    }
}

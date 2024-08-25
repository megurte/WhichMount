using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using WhichMount.Utils;

namespace WhichMount.Models;

public class MountModel {
    
    public uint Id { get; }
    public string Owner { get; }
    public string Name => _mountItem.Singular.ToDalamudString().ToTitleCase();
    public int NumberSeats => _mountItem.ExtraSeats + 1;
    public bool HasActions => _mountItem.MountAction.Row != 0;
    public bool HasUniqueMusic => _uniqueMusicMounts.Contains(_mountItem);
    
    private const string ResourceName = "WhichMount.Resources.MountList.csv";
    
    private readonly IDataManager _dataManager;
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

    public MountModel(IDataManager dataManager, uint id, string owner)
    {
        _dataManager = dataManager;
        Id = id;
        Owner = owner;
    }

    public bool TryInitData()
    {
        _mountItem = GetMountObject(Id);
        return _mountItem != null;
    }
    
    public Mount? GetMountObject(uint mountId)
    {
        return _dataManager.GetExcelSheet<Mount>()!.GetRow(mountId);
    }
    
    public string GetDataByTable(TargetData targetData)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        
        if (stream is {CanRead: true})
        {
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line)
            {
                var columns = line.Split(',');

                if (columns[(int)TargetData.Name].Equals(Name, StringComparison.OrdinalIgnoreCase))
                {
                    if (targetData >= 0 && (int)targetData < columns.Length)
                    {
                        return columns[(int)targetData];
                    }
                            
                    return $"Invalid targetData index: {targetData}";
                }
            }
        }

        return "Mount not found";
    }
}

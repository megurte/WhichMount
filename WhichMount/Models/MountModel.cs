using System.IO;
using System.Reflection;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using WhichMount.Utils;

namespace WhichMount.Models;

public enum TargetData
{
    Name = 0,
    Id = 1,
    //Icon = 2,
    AcquisitionType = 2,
    AcquiredBy = 3,
    IsObtainable = 4,
    CashShop = 5,
    MarketBoard = 6,
    Seats = 7,
    Patch = 8
}

public unsafe class MountModel 
{
    public uint Id { get; }
    public string Owner { get; }
    public uint IconId => _mountItem.Icon;
    public string Name => _mountItem.Singular.ToDalamudString().ToTitleCase();
    public int NumberSeats => _mountItem.ExtraSeats + 1;
    public bool HasActions => _mountItem.MountAction.RowId != 0;
    public bool HasUniqueMusic => _cashContainer.HasUniqueMusic(Id);
    public bool IsMountUnlocked => PlayerState.Instance()->IsMountUnlocked(Id);
    public bool IsMarketBoardAvailable => _cashContainer.GetCachedData(Id, TargetData.MarketBoard) == "1";
    
    private const string ResourceName = "WhichMount.Resources.MountList.csv";
    
    private readonly IDataManager _dataManager;
    private readonly CashContainer _cashContainer;
    private Mount _mountItem;
    
    public MountModel(IDataManager dataManager, CashContainer cashContainer, uint id, string owner)
    {
        _dataManager = dataManager;
        _cashContainer = cashContainer;
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
        return _dataManager.GetExcelSheet<Mount>().GetRow(mountId);
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

            if (columns.Length <= (int)TargetData.Id)
            {
                continue;
            }

            if (!uint.TryParse(columns[(int)TargetData.Id], out var mountId) || mountId != Id)
            {
                continue;
            }

            if ((int)targetData >= columns.Length)
            {
                return $"Invalid targetData index: {targetData}";
            }

            return columns[(int)targetData];
        }

        return "Data not found.";
    }
}

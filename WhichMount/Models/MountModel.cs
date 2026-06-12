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

public unsafe class MountModel(IDataManager dataManager, CacheContainer cacheContainer, uint id, string owner)
{
    public uint Id { get; } = id;
    public string Owner { get; } = owner;
    public uint IconId => _mountItem.Icon;
    public string Name => _mountItem.Singular.ToDalamudString().ToTitleCase();
    public int NumberSeats => _mountItem.ExtraSeats + 1;
    public bool HasActions => _mountItem.MountAction.RowId != 0;
    public bool HasUniqueMusic => cacheContainer.HasUniqueMusic(Id);
    public bool IsMountUnlocked => PlayerState.Instance()->IsMountUnlocked(Id);
    public bool IsMarketBoardAvailable => cacheContainer.GetCachedData(Id, TargetData.MarketBoard) == "1";

    private Mount _mountItem;

    public bool TryInitData()
    {
        _mountItem = GetMountObject(Id);
        return !Name.IsNullOrEmpty();
    }
    
    public Mount GetMountObject(uint mountId)
    {
        return dataManager.GetExcelSheet<Mount>().GetRow(mountId);
    }
}

using System;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using WhichMount.Models;

namespace WhichMount.Utils;

public static class CommonUtils
{
    public readonly struct Validation
    {
        public static bool IsWorldValid(uint worldId, IDataManager dataManager)
        {
            return IsWorldValid(GetWorld(worldId, dataManager));
        }
    
        public static World GetWorld(uint worldId, IDataManager dataManager)
        {
            var worldSheet = dataManager.GetExcelSheet<World>();
            return !worldSheet.TryGetRow(worldId, out var world) ? worldSheet.First() : world;
        }
    
        public static bool IsWorldValid(World world)
        {
            if (world.Name.IsEmpty || GetRegionCode(world) == string.Empty)
            {
                return false;
            }

            return char.IsUpper(world.Name.ToString()[0]);
        }
    
        public static string GetRegionCode(World world)
        {
            return world.DataCenter.Value.Region switch
            {
                1 => "JP",
                2 => "NA",
                3 => "EU",
                4 => "OC",
                _ => string.Empty,
            };
        }
    }
    
    public static int PatchSort(this CashContainer cashContainer, MountModel a, MountModel b)
    {
        var patchA = cashContainer.GetCachedData(a.Id, TargetData.Patch);
        var patchB = cashContainer.GetCachedData(b.Id, TargetData.Patch);

        var isUnknownA = string.IsNullOrWhiteSpace(patchA) || patchA == "Unknown";
        var isUnknownB = string.IsNullOrWhiteSpace(patchB) || patchB == "Unknown";

        if (isUnknownA && !isUnknownB) return 1;
        if (!isUnknownA && isUnknownB) return -1;
        if (isUnknownA && isUnknownB) return 0;

        return string.Compare(patchB, patchA, StringComparison.Ordinal);
    }
}

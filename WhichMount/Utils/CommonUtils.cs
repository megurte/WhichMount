using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

namespace WhichMount.Utils;

public static class CommonUtils
{
    public readonly struct SeStringConverter
    {
        public static string ParseSeStringLumina(SeString? luminaString)
            => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;
    }
    
    public readonly struct WikiStringConverter
    {
        public static string ConvertString(string text) => text?.Replace("&#160;", string.Empty) ?? string.Empty;
    }

    public readonly struct Validation
    {
        public static bool IsWorldValid(uint worldId, IDataManager dataManager)
        {
            return IsWorldValid(GetWorld(worldId, dataManager));
        }
    
        public static World GetWorld(uint worldId, IDataManager dataManager)
        {
            var worldSheet = dataManager.GetExcelSheet<World>()!;
            var world = worldSheet.FirstOrDefault(x => x.RowId == worldId);
            if (world == null)
            {
                return worldSheet.First();
            }

            return world;
        }
    
        public static bool IsWorldValid(World world)
        {
            if (world.Name.RawData.IsEmpty || GetRegionCode(world) == string.Empty)
            {
                return false;
            }

            return char.IsUpper((char)world.Name.RawData[0]);
        }
    
        public static string GetRegionCode(World world)
        {
            return world.DataCenter?.Value?.Region switch
            {
                1 => "JP",
                2 => "NA",
                3 => "EU",
                4 => "OC",
                _ => string.Empty,
            };
        }
    }
}

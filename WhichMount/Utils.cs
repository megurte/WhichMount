using Lumina.Text;

namespace WhichMount;

public static class Utils
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
}

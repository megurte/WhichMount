using System.Globalization;
using Dalamud.Game.Text.SeStringHandling;

namespace WhichMount.Utils;

public static class StringUtils
{
    public static CultureInfo Culture { get; set; } = new CultureInfo("en-US"); // TODO move to constants
    
    public static string ToTitleCase(this string str) {
        return ToTitleCase(str, Culture);
    }
    
    public static string ToTitleCase(this SeString seString) {
        return ToTitleCase(seString.ToString());
    }

    public static string ToTitleCase(this string str, CultureInfo culture) {
        var textInfo = culture.TextInfo;
        return textInfo.ToTitleCase(str);
    }
}

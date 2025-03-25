using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TextCleaner
{
    private static readonly Dictionary<string, string> SYMBOLS_MAPPING = new()
    {
        ["‘"] = "'",
        ["’"] = "'",
    };

    private static readonly Regex ReplaceSymbolRegex = new(
        string.Join("|", new List<string>(SYMBOLS_MAPPING.Keys).ConvertAll(Regex.Escape)),
        RegexOptions.Compiled);

    private static readonly Regex EmojiRegex = new(
        @"(\uD83D[\uDE00-\uDE4F])|" + // Emoticons
        @"(\uD83C[\uDF00-\uDFFF])|" + // Symbols & Pictographs
        @"(\uD83D[\uDC00-\uDDFF])|" + // Transport & Map Symbols
        @"(\uD83C[\uDDE6-\uDDFF])", // Flags (iOS)
        RegexOptions.Compiled);

    public static string CleanText(string text)
    {
        // Clean the text
        text = text.Trim();
        // Replace all chinese symbols with their english counterparts
        text = ReplaceSymbolRegex.Replace(text, match => SYMBOLS_MAPPING[match.Value]);
        // Remove emojis
        text = EmojiRegex.Replace(text, "");
        // Remove continuous periods (...) and commas (,,,)
        text = Regex.Replace(text, @",{2,}", m => m.Value[0].ToString());
        return text;
    }
}

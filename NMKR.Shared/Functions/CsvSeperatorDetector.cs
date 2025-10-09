using System.IO;
using System.Linq;

public static class CsvSeperatorDetector
{
    private static readonly char[] SeparatorChars = { ';', '|', '\t', ',' };

    public static char DetectSeparator(string csvFilePath)
    {
        string[] lines = File.ReadAllLines(csvFilePath);
        return DetectSeparator(lines);
    }

    public static char DetectSeparator(string[] lines)
    {
        var q = SeparatorChars.Select(sep => new
                { Separator = sep, Found = lines.GroupBy(line => line.Count(ch => ch == sep)) })
            .OrderByDescending(res => res.Found.Count(grp => grp.Key > 0))
            .ThenBy(res => res.Found.Count())
            .First();

        return q.Separator;
    }
}
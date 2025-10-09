using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CardanoSharp.Wallet.Extensions;
using NMKR.Shared.Functions;

public static class StringExtensions
{
    /// <summary>
    /// takes a substring between two anchor strings (or the end of the string if that anchor is null)
    /// </summary>
    /// <param Name="this">a string</param>
    /// <param Name="from">an optional string to search after</param>
    /// <param Name="until">an optional string to search before</param>
    /// <param Name="comparison">an optional comparison for the search</param>
    /// <returns>a substring based on the search</returns>
    public static string Between(this string @this, string from = null, string until = null,
        StringComparison comparison = StringComparison.InvariantCulture)
    {
        var fromLength = (from ?? string.Empty).Length;
        var startIndex = !string.IsNullOrEmpty(from)
            ? @this.IndexOf(from, comparison) + fromLength
            : 0;

        if (startIndex < fromLength)
        {
            return "";

        }

        var endIndex = !string.IsNullOrEmpty(until)
            ? @this.IndexOf(until, startIndex, comparison)
            : @this.Length;

        if (endIndex < 0)
        {
            return "";
        }

        var subString = @this.Substring(startIndex, endIndex - startIndex);
        return subString;
    }
    public static string ReplaceWithArray(this string s, string oldValue, string newValue)
    {
        if (newValue.Length<64)
            return s.Replace(oldValue, newValue);


        var lines = GlobalFunctions.SplitStringIntoChunks(newValue);
        string res = "[";

        string res1 = "";
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(res1))
                res1 += ",";

            res1 += "\"" + line + "\"";
        }
        res += res1 + "]";

        return s.Replace("\""+oldValue+"\"", res);
    }
    public static string ReplaceWithArrayInsensitive(this string s, string oldValue, string newValue)
    {
        if (newValue.Length < 64)
            return s.Replace(oldValue, newValue);


        var lines = GlobalFunctions.SplitStringIntoChunks(newValue);
        string res = "[";

        string res1 = "";
        foreach (var line in lines)
        {
            if (!string.IsNullOrEmpty(res1))
                res1 += ",";

            res1 += "\"" + line + "\"";
        }
        res += res1 + "]";

        return Regex.Replace(s,"\"" + oldValue + "\"", res, RegexOptions.IgnoreCase);
    }
    public static string ReplaceInsensitive(this string str, string from, string to)
    {
        str = Regex.Replace(str, from, to, RegexOptions.IgnoreCase);
        return str;
    }

    public static string Truncate(this string s, int length)
    {
        if (s == null)
            return "";
        if (s.Length > length) return s.Substring(0, length);
        return s;
    }
    public static string ToHex(this string s)
    {
        return GlobalFunctions.ToHexString(s);
    }
    public static string ToHexFilterPlaceholder(this string s)
    {
        if (s.StartsWith("<") && s.EndsWith(">"))
            return s;
        return GlobalFunctions.ToHexString(s);
    }
    public static string FromHex(this string s)
    {
        return GlobalFunctions.FromHexString(s);
    }
    public static byte[] HexStringToByteArray(this string hex)
    {
        if (hex.Length % 2 == 1)
        {
            throw new ArgumentException("Hex string must have an even number of characters");
        }

        if (hex.StartsWith("0x"))
        {
            hex = hex.Substring(2);
        }

        var b = hex.HexToByteArray();
        return b;
    }
    public static int FindClosingBracketIndex(string text, char openedBracket = '{', char closedBracket = '}')
    {
        int index = text.IndexOf(openedBracket);
        int bracketCount = 1;
        var textArray = text.ToCharArray();

        for (int i = index + 1; i < textArray.Length; i++)
        {
            if (textArray[i] == openedBracket)
            {
                bracketCount++;
            }
            else if (textArray[i] == closedBracket)
            {
                bracketCount--;
            }

            if (bracketCount == 0)
            {
                index = i;
                break;
            }
        }

        return index;
    }
    public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
    {
        return source ?? Enumerable.Empty<T>();
    }
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        HashSet<TKey> seenKeys = new();
        foreach (TSource element in source)
        {
            if (seenKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }

    public static long RoundOff(this long i)
    {
        return ((long) Math.Round(i / 1000.0)) * 1000;
    }

    public static string[] Split(this string s, int length)
    {
        System.Globalization.StringInfo str = new System.Globalization.StringInfo(s);

        int lengthAbs = Math.Abs(length);

        if (str == null || str.LengthInTextElements == 0 || lengthAbs == 0 || str.LengthInTextElements <= lengthAbs)
            return new string[] { str.ToString() };

        string[] array = new string[(str.LengthInTextElements % lengthAbs == 0 ? str.LengthInTextElements / lengthAbs : (str.LengthInTextElements / lengthAbs) + 1)];

        if (length > 0)
            for (int iStr = 0, iArray = 0; iStr < str.LengthInTextElements && iArray < array.Length; iStr += lengthAbs, iArray++)
                array[iArray] = str.SubstringByTextElements(iStr, (str.LengthInTextElements - iStr < lengthAbs ? str.LengthInTextElements - iStr : lengthAbs));
        else // if (length < 0)
            for (int iStr = str.LengthInTextElements - 1, iArray = array.Length - 1; iStr >= 0 && iArray >= 0; iStr -= lengthAbs, iArray--)
                array[iArray] = str.SubstringByTextElements((iStr - lengthAbs < 0 ? 0 : iStr - lengthAbs + 1), (iStr - lengthAbs < 0 ? iStr + 1 : lengthAbs));

        return array;
    }
}
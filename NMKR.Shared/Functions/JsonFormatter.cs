using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;

public static class JsonFormatter
{
  //  private const string INDENT_STRING = "    ";

    // Version 1
    public static string FormatJson(string json, string indent = "  ")
    {
        var indentation = 0;
        var quoteCount = 0;
        var escapeCount = 0;

        var result =
            from ch in json ?? string.Empty
            let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
            let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
            let unquoted = quotes % 2 == 0
            let colon = ch == ':' && unquoted ? ": " : null
            let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
            let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
            let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
            let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
            select colon ?? nospace ?? lineBreak ?? (
                openChar.Length > 1 ? openChar : closeChar
            );

        return string.Concat(result);
    }


    private class IndentJsonInfo
    {
        public IndentJsonInfo(string prefix, char openingTag)
        {
            Prefix = prefix;
            OpeningTag = openingTag;
            Data = new();
        }
        public readonly string Prefix;
        public readonly char OpeningTag;
        public bool isOutputStarted;
        public List<string> Data;
    }

    // Version 2
    internal static string IndentJSON(string jsonString, int startIndent = 0, int indentSpaces = 2)
    {
        if (String.IsNullOrEmpty(jsonString))
            return jsonString;

        try
        {
            var jsonCache = new List<IndentJsonInfo>();
            IndentJsonInfo currentItem = null;

            var sbResult = new StringBuilder();

            int curIndex = 0;
            bool inQuotedText = false;

            var chunk = new StringBuilder();

            var saveChunk = new Action(() =>
            {
                if (chunk.Length == 0)
                    return;
                if (currentItem == null)
                    throw new("Invalid JSON: No container.");
                currentItem.Data.Add(chunk.ToString());
                chunk = new();
            });

            while (curIndex < jsonString.Length)
            {
                var cChar = jsonString[curIndex];
                if (inQuotedText)
                {
                    // Get the rest of quoted text.
                    chunk.Append(cChar);

                    // Determine if the quote is escaped.
                    bool isEscaped = false;
                    var excapeIndex = curIndex;
                    while (excapeIndex > 0 && jsonString[--excapeIndex] == '\\') isEscaped = !isEscaped;

                    if (cChar == '"' && !isEscaped)
                        inQuotedText = false;
                }
                else if (Char.IsWhiteSpace(cChar))
                {
                    // Ignore all whitespace outside of quotes.
                }
                else
                {
                    // Outside of Quotes.
                    switch (cChar)
                    {
                        case '"':
                            chunk.Append(cChar);
                            inQuotedText = true;
                            break;
                        case ',':
                            chunk.Append(cChar);
                            saveChunk();
                            break;
                        case '{':
                        case '[':
                            currentItem = new(chunk.ToString(), cChar);
                            jsonCache.Add(currentItem);
                            chunk = new();
                            break;
                        case '}':
                        case ']':
                            saveChunk();
                            for (int i = 0; i < jsonCache.Count; i++)
                            {
                                var item = jsonCache[i];
                                var isLast = i == jsonCache.Count - 1;
                                if (!isLast)
                                {
                                    if (!item.isOutputStarted)
                                    {
                                        sbResult.AppendLine(
                                            "".PadLeft((startIndent + i) * indentSpaces) +
                                            item.Prefix + item.OpeningTag);
                                        item.isOutputStarted = true;
                                    }
                                    var newIndentString = "".PadLeft((startIndent + i + 1) * indentSpaces);
                                    foreach (var listItem in item.Data)
                                    {
                                        sbResult.AppendLine(newIndentString + listItem);
                                    }
                                    item.Data = new();
                                }
                                else // If Last
                                {
                                    if (!(
                                        (item.OpeningTag == '{' && cChar == '}') ||
                                        (item.OpeningTag == '[' && cChar == ']')
                                       ))
                                    {
                                        throw new("Invalid JSON: Container Mismatch, Open '" + item.OpeningTag + "', Close '" + cChar + "'.");
                                    }

                                    string closing = null;
                                    if (item.isOutputStarted)
                                    {
                                        var newIndentString = "".PadLeft((startIndent + i + 1) * indentSpaces);
                                        foreach (var listItem in item.Data)
                                        {
                                            sbResult.AppendLine(newIndentString + listItem);
                                        }
                                        closing = cChar.ToString();
                                    }
                                    else
                                    {
                                        closing =
                                            item.Prefix + item.OpeningTag +
                                            String.Join("", currentItem.Data.ToArray()) +
                                            cChar;
                                    }

                                    jsonCache.RemoveAt(i);
                                    currentItem = (jsonCache.Count > 0) ? jsonCache[jsonCache.Count - 1] : null;
                                    chunk.Append(closing);
                                }
                            }
                            break;
                        default:
                            chunk.Append(cChar);
                            break;
                    }
                }
                curIndex++;
            }

            if (inQuotedText)
                throw new("Invalid JSON: Incomplete Quote");
            else if (jsonCache.Count != 0)
                throw new("Invalid JSON: Incomplete Structure");
            else
            {
                if (chunk.Length > 0)
                    sbResult.AppendLine("".PadLeft(startIndent * indentSpaces) + chunk);
                var result = sbResult.ToString();
                return result;
            }
        }
        catch 
        {
            throw;  // Comment out to return unformatted text if the format failed.
                    // Invalid JSON, skip the formatting.
          //  return jsonString;
        }
    }

}
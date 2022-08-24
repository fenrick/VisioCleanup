// -----------------------------------------------------------------------
// <copyright file="LevelOutputFormat.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Rendering;

/// <summary>
/// Implements the {Level} element. can now have a fixed width applied to it, as well as casing rules. Width is
/// set through formats like "u3" (uppercase three chars), "w1" (one lowercase char), or "t4" (title case four chars).
/// </summary>
internal static class LevelOutputFormat
{
    private static readonly string[][] LowercaseLevelMap =
    {
        new[] { "v", "vb", "vrb", "verb" },
        new[] { "d", "de", "dbg", "dbug" },
        new[] { "i", "in", "inf", "info" },
        new[] { "w", "wn", "wrn", "warn" },
        new[] { "e", "er", "err", "eror" },
        new[] { "f", "fa", "ftl", "fatl" },
    };

    private static readonly string[][] TitleCaseLevelMap =
    {
        new[] { "V", "Vb", "Vrb", "Verb" },
        new[] { "D", "De", "Dbg", "Dbug" },
        new[] { "I", "In", "Inf", "Info" },
        new[] { "W", "Wn", "Wrn", "Warn" },
        new[] { "E", "Er", "Err", "Eror" },
        new[] { "F", "Fa", "Ftl", "Fatl" },
    };

    private static readonly string[][] UppercaseLevelMap =
    {
        new[] { "V", "VB", "VRB", "VERB" },
        new[] { "D", "DE", "DBG", "DBUG" },
        new[] { "I", "IN", "INF", "INFO" },
        new[] { "W", "WN", "WRN", "WARN" },
        new[] { "E", "ER", "ERR", "EROR" },
        new[] { "F", "FA", "FTL", "FATL" },
    };

    internal static string GetLevelMoniker(LogEventLevel value, string format)
    {
        const int shortLength = 2;
        const int longLength = 3;
        const int secondDecimalPosition = 10;
        const int maxWidth = 4;

        if (format.Length is not shortLength and not longLength)
        {
            return Casing.Format(value.ToString(), format);
        }

        // Using int.Parse() here requires allocating a string to exclude the first character prefix.
        // Junk like "wxy" will be accepted but produce benign results.
        var width = format[1] - '0';
        if (format.Length == longLength)
        {
            width *= secondDecimalPosition;
            width += format[shortLength] - '0';
        }

        if (width < 1)
        {
            return string.Empty;
        }

        if (width > maxWidth)
        {
            var stringValue = value.ToString();
            if (stringValue.Length > width)
            {
                stringValue = stringValue[..width];
            }

            return Casing.Format(stringValue, formatString: null);
        }

        var index = (int)value;
        if (index is >= 0 and <= (int)LogEventLevel.Fatal)
        {
            return MapCaseLevelMoniker(format, index, width, value.ToString());
        }

        return Casing.Format(value.ToString(), format);
    }

    private static string MapCaseLevelMoniker(string format, int index, int width, string value) =>
        format[0] switch
        {
            'w' => LowercaseLevelMap[index][width - 1],
            'u' => UppercaseLevelMap[index][width - 1],
            't' => TitleCaseLevelMap[index][width - 1],
            _ => Casing.Format(value, format),
        };
}

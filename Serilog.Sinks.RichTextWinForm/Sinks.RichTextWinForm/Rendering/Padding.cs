﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Padding.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering;

using Serilog.Parsing;

/// <summary>The padding.</summary>
internal static class Padding
{
    /// <summary>The padding chars.</summary>
    private static readonly char[] PaddingChars = new string(' ', 80).ToCharArray();

    /// <summary>Writes the provided <paramref name="value"/> to the output, applying direction-based padding when<paramref name="alignment"/> is provided.</summary>
    /// <param name="output">Output object to write result.</param>
    /// <param name="value">Provided value.</param>
    /// <param name="alignment">The alignment settings to apply when rendering <paramref name="value"/> .</param>
    internal static void Apply(TextBoxBase output, string value, Alignment? alignment)
    {
        if (alignment is null || (value.Length >= alignment.Value.Width))
        {
            output.AppendText(value);
            return;
        }

        var pad = alignment.Value.Width - value.Length;

        if (alignment.Value.Direction == AlignmentDirection.Left)
        {
            output.AppendText(value);
        }

        if (pad <= PaddingChars.Length)
        {
            using StringWriter buffer = new ();
            buffer.Write(PaddingChars, 0, pad);
            output.AppendText(buffer.ToString());
        }
        else
        {
            output.AppendText(new string(' ', pad));
        }

        if (alignment.Value.Direction == AlignmentDirection.Right)
        {
            output.AppendText(value);
        }
    }
}

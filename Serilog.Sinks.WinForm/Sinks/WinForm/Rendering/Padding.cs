// -----------------------------------------------------------------------
// <copyright file="Padding.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Rendering
{
    using System.IO;

    using Serilog.Parsing;

    internal static class Padding
    {
        private static readonly char[] PaddingChars = new string(' ', 80).ToCharArray();

        /// <summary>
        /// Writes the provided <paramref name="value" /> to the output, applying direction-based padding when
        /// <paramref name="alignment" /> is provided.
        /// </summary>
        /// <param name="output">Output object to write result.</param>
        /// <param name="value">Provided value.</param>
        /// <param name="alignment">The alignment settings to apply when rendering <paramref name="value" /> .</param>
        public static void Apply(TextWriter output, string value, Alignment? alignment)
        {
            if (alignment is null || (value.Length >= alignment.Value.Width))
            {
                output.Write(value);
                return;
            }

            var pad = alignment.Value.Width - value.Length;

            if (alignment.Value.Direction == AlignmentDirection.Left)
            {
                output.Write(value);
            }

            if (pad <= PaddingChars.Length)
            {
                using StringWriter buffer = new();
                buffer.Write(PaddingChars, 0, pad);
                output.Write(buffer.ToString());
            }
            else
            {
                output.Write(new string(' ', pad));
            }

            if (alignment.Value.Direction == AlignmentDirection.Right)
            {
                output.Write(value);
            }
        }
    }
}

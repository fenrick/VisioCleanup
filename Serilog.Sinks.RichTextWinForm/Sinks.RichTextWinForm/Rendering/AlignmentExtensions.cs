// -----------------------------------------------------------------------
// <copyright file="AlignmentExtensions.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering
{
    using Serilog.Parsing;

    internal static class AlignmentExtensions
    {
        public static Alignment Widen(this Alignment alignment, int amount)
        {
            return new(alignment.Direction, alignment.Width + amount);
        }
    }
}
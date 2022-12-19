// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AlignmentExtensions.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering;

using Serilog.Parsing;

/// <summary>The alignment extensions.</summary>
internal static class AlignmentExtensions
{
    /// <summary>The widen.</summary>
    /// <param name="alignment">The alignment.</param>
    /// <param name="amount">The amount.</param>
    /// <returns>The <see cref="Alignment"/>.</returns>
    internal static Alignment Widen(this Alignment alignment, int amount) => new (alignment.Direction, alignment.Width + amount);
}

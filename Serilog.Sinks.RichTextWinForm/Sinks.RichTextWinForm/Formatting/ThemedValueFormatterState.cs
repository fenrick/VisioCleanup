// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatterState.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System;
using System.Windows.Forms;

/// <summary>Formatter State.</summary>
internal readonly struct ThemedValueFormatterState : IEquatable<ThemedValueFormatterState>
{
    /// <summary>Gets rich text box.</summary>
    /// <value>Output rich text box.</value>
    internal RichTextBox Output { get; init; }

    /// <summary>Gets format string.</summary>
    /// <value>Format string.</value>
    internal string Format { get; init; }

    /// <summary>Gets a value indicating whether it's a top level object.</summary>
    /// <value>Top level.</value>
    internal bool IsTopLevel { get; init; }

    /// <inheritdoc />
    public bool Equals(ThemedValueFormatterState other)
    {
        if (!this.Output.Equals(other.Output))
        {
            return false;
        }

        if (!string.Equals(this.Format, other.Format, StringComparison.InvariantCulture))
        {
            return false;
        }

        return this.IsTopLevel == other.IsTopLevel;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ThemedValueFormatterState other && this.Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(this.Output, this.Format, this.IsTopLevel);

    /// <summary>Next within a new formatter.</summary>
    /// <returns>New formatter state with this state within it.</returns>
    internal ThemedValueFormatterState Nest() => new() { Output = this.Output };
}

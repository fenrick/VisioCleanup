﻿// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatterState.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System.Windows.Forms;

/// <summary>Formatter State.</summary>
internal readonly struct ThemedValueFormatterState
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

    /// <summary>Next within a new formatter.</summary>
    /// <returns>New formatter state with this state within it.</returns>
    internal ThemedValueFormatterState Nest() => new() { Output = this.Output };
}

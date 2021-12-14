// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System;
using System.Windows.Forms;

using Serilog.Data;
using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Themes;

internal abstract class ThemedValueFormatter : LogEventPropertyValueVisitor<ThemedValueFormatterState, int>
{
    private readonly RichTextTheme theme;

    protected ThemedValueFormatter(RichTextTheme theme) => this.theme = theme ?? throw new ArgumentNullException(nameof(theme));

    internal void Format(LogEventPropertyValue value, RichTextBox output, string formatString, bool literalTopLevel = false)
    {
        var themedValueFormatterState = new ThemedValueFormatterState { Output = output, Format = formatString, IsTopLevel = literalTopLevel };
        this.Visit(themedValueFormatterState, value);
    }

    protected StyleReset ApplyStyle(RichTextBox output, RichTextThemeStyle style) => this.theme.Apply(output, style);
}

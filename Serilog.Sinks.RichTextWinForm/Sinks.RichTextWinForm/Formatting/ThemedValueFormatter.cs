// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting
{
    using System;
    using System.Windows.Forms;

    using Serilog.Data;
    using Serilog.Events;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal abstract class ThemedValueFormatter : LogEventPropertyValueVisitor<ThemedValueFormatterState, int>
    {
        private readonly RichTextTheme theme;

        protected ThemedValueFormatter(RichTextTheme theme)
        {
            this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }

        public int Format(LogEventPropertyValue value, RichTextBox output, string format, bool literalTopLevel = false)
        {
            return this.Visit(new ThemedValueFormatterState { Output = output, Format = format, IsTopLevel = literalTopLevel }, value);
        }

        public abstract ThemedValueFormatter SwitchTheme(RichTextTheme theme);

        protected StyleReset ApplyStyle(RichTextBox output, RichTextThemeStyle style, ref int invisibleCharacterCount)
        {
            return this.theme.Apply(output, style, ref invisibleCharacterCount);
        }
    }
}
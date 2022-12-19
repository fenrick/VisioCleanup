// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemedValueFormatter.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using Serilog.Data;
using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The themed value formatter.</summary>
internal abstract class ThemedValueFormatter : LogEventPropertyValueVisitor<ThemedValueFormatterState, int>
{
    /// <summary>The theme.</summary>
    private readonly RichTextTheme theme;

    /// <summary>Initialises a new instance of the <see cref="ThemedValueFormatter"/> class.</summary>
    /// <param name="theme">The theme.</param>
    protected ThemedValueFormatter(RichTextTheme theme) => this.theme = theme ?? throw new ArgumentNullException(nameof(theme));

    /// <summary>The visit dictionary value line.</summary>
    /// <param name="pair">The pair.</param>
    /// <param name="delim">The delim.</param>
    /// <param name="count">The count.</param>
    protected delegate void VisitDictionaryValueLine(KeyValuePair<ScalarValue, LogEventPropertyValue> pair, ref string delim, ref int count);

    /// <summary>The format.</summary>
    /// <param name="value">The value.</param>
    /// <param name="output">The output.</param>
    /// <param name="formatString">The format string.</param>
    /// <param name="literalTopLevel">The literal top level.</param>
    internal void Format(LogEventPropertyValue value, RichTextBox output, string formatString, bool literalTopLevel = false)
    {
        var themedValueFormatterState = new ThemedValueFormatterState { Output = output, Format = formatString, IsTopLevel = literalTopLevel };
        this.Visit(themedValueFormatterState, value);
    }

    /// <summary>The apply style.</summary>
    /// <param name="output">The output.</param>
    /// <param name="style">The style.</param>
    /// <returns>The <see cref="StyleReset"/>.</returns>
    protected StyleReset ApplyStyle(RichTextBox output, RichTextThemeStyle style) => this.theme.Apply(output, style);

    /// <summary>The output text.</summary>
    /// <param name="output">The output.</param>
    /// <param name="text">The text.</param>
    /// <param name="richTextThemeStyle">The rich text theme style.</param>
    protected void OutputText(RichTextBox output, string text, RichTextThemeStyle richTextThemeStyle)
    {
        using (this.ApplyStyle(output, richTextThemeStyle))
        {
            output.AppendText(text);
        }
    }

    /// <summary>The visit dictionary value internal.</summary>
    /// <param name="state">The state.</param>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="lineFormatting">The line formatting.</param>
    /// <returns>The <see cref="int"/>.</returns>
    protected int VisitDictionaryValueInternal(ThemedValueFormatterState state, DictionaryValue dictionary, VisitDictionaryValueLine lineFormatting)
    {
        var count = 0;

        this.OutputText(state.Output, "{", RichTextThemeStyle.TertiaryText);

        var delim = string.Empty;
        foreach (var pair in dictionary.Elements)
        {
            lineFormatting(pair, ref delim, ref count);
        }

        this.OutputText(state.Output, "}", RichTextThemeStyle.TertiaryText);

        return count;
    }
}

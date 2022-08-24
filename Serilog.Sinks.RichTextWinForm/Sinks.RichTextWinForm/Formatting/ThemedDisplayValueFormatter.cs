// -----------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System.Globalization;

using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RichTextWinForm.Themes;
internal sealed class ThemedDisplayValueFormatter : ThemedValueFormatter
{
    private readonly IFormatProvider? formatProvider;

internal ThemedDisplayValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
        : base(theme) =>
        this.formatProvider = formatProvider;

    internal void FormatLiteralValue(ScalarValue scalar, RichTextBox output, string format)
    {
        switch (scalar.Value)
        {
            case null:
                this.FormatNullValue(output);
                break;
            case string str:
                this.FormatStringValue(output, format, str);
                break;
            case bool b:
                this.FormatBooleanValue(output, b);
                break;
            case char ch:
                this.FormatCharacterValue(output, ch);
                break;
            case ValueType:
                this.FormatNumberValue(scalar, output, format);
                break;
            default:
                this.FormatScalarValue(scalar, output, format);
                break;
        }
    }

    protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary) =>
        this.VisitDictionaryValueInternal(
            state,
            dictionary,
            (KeyValuePair<ScalarValue, LogEventPropertyValue> pair, ref string delim, ref int count) =>
                {
                    var scalarValue = pair.Key;
                    var logEventPropertyValue = pair.Value;

                    if (delim.Length != 0)
                    {
                        this.OutputText(state.Output, delim, RichTextThemeStyle.TertiaryText);
                    }

                    delim = ", ";

                    this.OutputText(state.Output, "[", RichTextThemeStyle.TertiaryText);

                    using (this.ApplyStyle(state.Output, RichTextThemeStyle.String))
                    {
                        count += this.Visit(state.Nest(), scalarValue);
                    }

                    this.OutputText(state.Output, "]=", RichTextThemeStyle.TertiaryText);

                    count += this.Visit(state.Nest(), logEventPropertyValue);
                });

    protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
    {
        if (scalar is null)
        {
            throw new ArgumentNullException(nameof(scalar));
        }

        this.FormatLiteralValue(scalar, state.Output, state.Format);
        return 0;
    }

    protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
    {
        if (sequence is null)
        {
            throw new ArgumentNullException(nameof(sequence));
        }

        this.OutputText(state.Output, "[", RichTextThemeStyle.TertiaryText);

        var delim = string.Empty;
        foreach (var t in sequence.Elements)
        {
            if (delim.Length != 0)
            {
                this.OutputText(state.Output, delim, RichTextThemeStyle.TertiaryText);
            }

            delim = ", ";
            this.Visit(state, t);
        }

        this.OutputText(state.Output, "]", RichTextThemeStyle.TertiaryText);

        return 0;
    }

    protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
    {
        var count = 0;

        if (structure.TypeTag is not null)
        {
            this.OutputText(state.Output, structure.TypeTag, RichTextThemeStyle.Name);
            state.Output.AppendText(" ");
        }

        this.OutputText(state.Output, "{", RichTextThemeStyle.TertiaryText);

        var delim = string.Empty;
        foreach (var logEventProperty in structure.Properties)
        {
            if (delim.Length != 0)
            {
                this.OutputText(state.Output, delim, RichTextThemeStyle.TertiaryText);
            }

            delim = ", ";

            this.OutputText(state.Output, logEventProperty.Name, RichTextThemeStyle.Name);
            this.OutputText(state.Output, "=", RichTextThemeStyle.TertiaryText);

            count += this.Visit(state.Nest(), logEventProperty.Value);
        }

        this.OutputText(state.Output, "}", RichTextThemeStyle.TertiaryText);
        return count;
    }

    private void FormatBooleanValue(RichTextBox output, bool booleanValue) =>
        this.OutputText(output, booleanValue.ToString(CultureInfo.CurrentCulture), RichTextThemeStyle.Boolean);

    private void FormatCharacterValue(RichTextBox output, char charValue) =>
        this.OutputText(output, string.Concat("'", charValue.ToString(CultureInfo.CurrentCulture), "'"), RichTextThemeStyle.Scalar);

    private void FormatNullValue(RichTextBox output) => this.OutputText(output, "null", RichTextThemeStyle.Null);

    private void FormatNumberValue(LogEventPropertyValue scalar, RichTextBox output, string format)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            using StringWriter buffer = new();
            scalar.Render(buffer, format, this.formatProvider);
            output.AppendText(buffer.ToString());
        }
    }

    private void FormatScalarValue(LogEventPropertyValue scalar, RichTextBox output, string format)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new();
            scalar.Render(buffer, format, this.formatProvider);
            output.AppendText(buffer.ToString());
        }
    }

    private void FormatStringValue(RichTextBox output, string format, string stringValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.String))
        {
            if (!string.Equals(format, "l", StringComparison.Ordinal))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(stringValue, buffer);
                output.AppendText(buffer.ToString());

                return;
            }

            output.AppendText(stringValue);
        }
    }
}

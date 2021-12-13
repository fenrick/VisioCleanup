// -----------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

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
            case ValueType and (int or uint or long):
            case ValueType and (ulong or decimal or byte):
            case ValueType and (sbyte or short or ushort):
            case ValueType and (float or double):
                this.FormatNumberValue(scalar, output, format);
                break;
            case bool b:
                this.FormatBooleanValue(output, b);
                break;
            case char ch:
                this.FormatCharacterValue(output, ch);
                break;
            default:
                this.FormatScalarValue(scalar, output, format);
                break;
        }
    }

    protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
    {
        var count = 0;

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("{");
        }

        var delim = string.Empty;
        foreach (var pair in dictionary.Elements)
        {
            var scalarValue = pair.Key;
            var logEventPropertyValue = pair.Value;

            if (delim.Length != 0)
            {
                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
                {
                    state.Output.AppendText(delim);
                }
            }

            delim = ", ";

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText("[");
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.String))
            {
                count += this.Visit(state.Nest(), scalarValue);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText("]");
                state.Output.AppendText("=");
            }

            count += this.Visit(state.Nest(), logEventPropertyValue);
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("}");
        }

        return count;
    }

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

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("[");
        }

        var delim = string.Empty;
        foreach (var t in sequence.Elements)
        {
            if (delim.Length != 0)
            {
                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
                {
                    state.Output.AppendText(delim);
                }
            }

            delim = ", ";
            this.Visit(state, t);
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("]");
        }

        return 0;
    }

    protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
    {
        var count = 0;

        if (structure.TypeTag is not null)
        {
            using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name))
            {
                state.Output.AppendText(structure.TypeTag);
            }

            state.Output.AppendText(" ");
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("{");
        }

        var delim = string.Empty;
        foreach (var logEventProperty in structure.Properties)
        {
            if (delim.Length != 0)
            {
                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
                {
                    state.Output.AppendText(delim);
                }
            }

            delim = ", ";

            var property = logEventProperty;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name))
            {
                state.Output.AppendText(property.Name);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText("=");
            }

            count += this.Visit(state.Nest(), property.Value);
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("}");
        }

        return count;
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

    private void FormatCharacterValue(RichTextBox output, char charValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            output.AppendText("'");
            output.AppendText(charValue.ToString(CultureInfo.CurrentCulture));
            output.AppendText("'");
        }
    }

    private void FormatBooleanValue(RichTextBox output, bool booleanValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Boolean))
        {
            output.AppendText(booleanValue.ToString(CultureInfo.CurrentCulture));
        }
    }

    private void FormatNumberValue(LogEventPropertyValue scalar, RichTextBox output, string format)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
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

    private void FormatNullValue(RichTextBox output)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Null))
        {
            output.AppendText("null");
        }
    }
}

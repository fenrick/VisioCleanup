// -----------------------------------------------------------------------
// <copyright file="ThemedJsonValueFormatter.cs" company="Jolyon Suthers">
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

internal sealed class ThemedJsonValueFormatter : ThemedValueFormatter
{
    private readonly ThemedDisplayValueFormatter displayFormatter;

    internal ThemedJsonValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
        : base(theme) =>
        this.displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);

    protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
    {
        if (dictionary is null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

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

            var style = scalarValue.Value switch
            {
                string => RichTextThemeStyle.String,
                null => RichTextThemeStyle.Null,
                _ => RichTextThemeStyle.Scalar,
            };

            using (this.ApplyStyle(state.Output, style))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString((scalarValue.Value ?? "null").ToString(), buffer);
                state.Output.AppendText(buffer.ToString());
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(":");
                state.Output.AppendText(" ");
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

        // At the top level, for scalar values, use "display" rendering.
        if (state.IsTopLevel)
        {
            this.displayFormatter.FormatLiteralValue(scalar, state.Output, state.Format);
            return 0;
        }

        this.FormatLiteralValue(scalar, state.Output);
        return 0;
    }

    protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
    {
        if (sequence == null)
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
            this.Visit(state.Nest(), t);
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("]");
        }

        return 0;
    }

    protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
    {
        if (structure is null)
        {
            throw new ArgumentNullException(nameof(structure));
        }

        var count = 0;

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
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(property.Name, buffer);
                state.Output.AppendText(buffer.ToString());
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(":");
                state.Output.AppendText(" ");
            }

            count += this.Visit(state.Nest(), property.Value);
        }

        if (structure.TypeTag is not null)
        {
            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(delim);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString("$type", buffer);
                state.Output.AppendText(buffer.ToString());
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(":");
                state.Output.AppendText(" ");
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.String))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(structure.TypeTag, buffer);
                state.Output.AppendText(buffer.ToString());
            }
        }

        using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
        {
            state.Output.AppendText("}");
        }

        return count;
    }

    private void FormatLiteralValue(ScalarValue scalar, RichTextBox output)
    {
        switch (scalar.Value)
        {
            case null:
                this.FormatNullValue(output);
                break;
            case string str:
                this.FormatStringValue(output, str);
                break;
            case double d:
                this.FormatDoubleValue(output, d);
                break;
            case float f:
                this.FormatFloatValue(output, f);
                break;
            case bool b:
                this.FormatBooleanValue(output, b);
                break;
            case char ch:
                this.FormatCharacterValue(output, ch);
                break;
            case DateTime:
            case DateTimeOffset:
                this.FormatDateTimeValue(output, scalar.Value);
                break;
            case ValueType:
                this.FormatNumberValue(output, scalar.Value);
                break;
            default:
                this.FormatScalarValue(output, scalar.Value);
                break;
        }
    }

    private void FormatScalarValue(RichTextBox output, object value)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new();
            JsonValueFormatter.WriteQuotedJsonString(value.ToString(), buffer);
            output.AppendText(buffer.ToString());
        }
    }

    private void FormatDateTimeValue(RichTextBox output, object value)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            output.AppendText("\"");
            output.AppendText(((IFormattable)value).ToString("O", CultureInfo.CurrentCulture));
            output.AppendText("\"");
        }
    }

    private void FormatCharacterValue(RichTextBox output, char characterValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new();
            JsonValueFormatter.WriteQuotedJsonString(characterValue.ToString(CultureInfo.CurrentCulture), buffer);
            output.AppendText(buffer.ToString());
        }
    }

    private void FormatBooleanValue(RichTextBox output, bool booleanValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Boolean))
        {
            output.AppendText(booleanValue ? "true" : "false");
        }
    }

    private void FormatFloatValue(RichTextBox output, float floatValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            if (double.IsNaN(floatValue) || double.IsInfinity(floatValue))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(floatValue.ToString(CultureInfo.CurrentCulture), buffer);
                output.AppendText(buffer.ToString());
                return;
            }

            output.AppendText(floatValue.ToString("R", CultureInfo.CurrentCulture));
        }
    }

    private void FormatDoubleValue(RichTextBox output, double doubleValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(doubleValue.ToString(CultureInfo.CurrentCulture), buffer);
                output.AppendText(buffer.ToString());
                return;
            }

            output.AppendText(doubleValue.ToString("R", CultureInfo.CurrentCulture));
        }
    }

    private void FormatNumberValue(RichTextBox output, object value)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            output.AppendText(((IFormattable)value).ToString(format: null, CultureInfo.CurrentCulture));
        }
    }

    private void FormatStringValue(RichTextBox output, string stringValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.String))
        {
            using StringWriter buffer = new();
            JsonValueFormatter.WriteQuotedJsonString(stringValue, buffer);
            output.AppendText(buffer.ToString());
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

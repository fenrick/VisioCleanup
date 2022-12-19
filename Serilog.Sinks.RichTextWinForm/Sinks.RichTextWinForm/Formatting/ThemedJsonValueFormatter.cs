// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemedJsonValueFormatter.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System.Globalization;

using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The themed json value formatter.</summary>
internal sealed class ThemedJsonValueFormatter : ThemedValueFormatter
{
    /// <summary>The display formatter.</summary>
    private readonly ThemedDisplayValueFormatter displayFormatter;

    /// <summary>Initialises a new instance of the <see cref="ThemedJsonValueFormatter"/> class.</summary>
    /// <param name="theme">The theme.</param>
    /// <param name="formatProvider">The format provider.</param>
    internal ThemedJsonValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
        : base(theme) =>
        this.displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);

    /// <summary>The visit dictionary value.</summary>
    /// <param name="state">The state.</param>
    /// <param name="dictionary">The dictionary.</param>
    /// <returns>The <see cref="int"/>.</returns>
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

                    var style = scalarValue.Value switch
                    {
                        string => RichTextThemeStyle.String,
                        null => RichTextThemeStyle.Null,
                        _ => RichTextThemeStyle.Scalar,
                    };

                    using (this.ApplyStyle(state.Output, style))
                    {
                        using StringWriter buffer = new ();
                        JsonValueFormatter.WriteQuotedJsonString((scalarValue.Value ?? "null").ToString(), buffer);
                        state.Output.AppendText(buffer.ToString());
                    }

                    this.OutputText(state.Output, ": ", RichTextThemeStyle.TertiaryText);

                    count += this.Visit(state.Nest(), logEventPropertyValue);
                });

    /// <summary>The visit scalar value.</summary>
    /// <param name="state">The state.</param>
    /// <param name="scalar">The scalar.</param>
    /// <returns>The <see cref="int"/>.</returns>
    /// <exception cref="ArgumentNullException">Null argument.</exception>
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

    /// <summary>The visit sequence value.</summary>
    /// <param name="state">The state.</param>
    /// <param name="sequence">The sequence.</param>
    /// <returns>The <see cref="int"/>.</returns>
    /// <exception cref="ArgumentNullException">Null argument.</exception>
    protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
    {
        if (sequence == null)
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
            this.Visit(state.Nest(), t);
        }

        this.OutputText(state.Output, "]", RichTextThemeStyle.TertiaryText);

        return 0;
    }

    /// <summary>The visit structure value.</summary>
    /// <param name="state">The state.</param>
    /// <param name="structure">The structure.</param>
    /// <returns>The <see cref="int"/>.</returns>
    protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
    {
        var count = 0;

        this.OutputText(state.Output, "{", RichTextThemeStyle.TertiaryText);

        var delim = string.Empty;
        foreach (var logEventProperty in structure.Properties)
        {
            if (delim.Length != 0)
            {
                this.OutputText(state.Output, delim, RichTextThemeStyle.TertiaryText);
            }

            delim = ", ";

            var property = logEventProperty;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString(property.Name, buffer);
                state.Output.AppendText(buffer.ToString());
            }

            this.OutputText(state.Output, ": ", RichTextThemeStyle.TertiaryText);

            count += this.Visit(state.Nest(), property.Value);
        }

        if (structure.TypeTag is not null)
        {
            this.OutputText(state.Output, delim, RichTextThemeStyle.TertiaryText);

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString("$type", buffer);
                state.Output.AppendText(buffer.ToString());
            }

            this.OutputText(state.Output, ": ", RichTextThemeStyle.TertiaryText);

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.String))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString(structure.TypeTag, buffer);
                state.Output.AppendText(buffer.ToString());
            }
        }

        this.OutputText(state.Output, "}", RichTextThemeStyle.TertiaryText);

        return count;
    }

    /// <summary>The format boolean value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="booleanValue">The boolean value.</param>
    private void FormatBooleanValue(RichTextBox output, bool booleanValue) => this.OutputText(output, booleanValue ? "true" : "false", RichTextThemeStyle.Boolean);

    /// <summary>The format character value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="characterValue">The character value.</param>
    private void FormatCharacterValue(RichTextBox output, char characterValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new ();
            JsonValueFormatter.WriteQuotedJsonString(characterValue.ToString(CultureInfo.CurrentCulture), buffer);
            output.AppendText(buffer.ToString());
        }
    }

    /// <summary>The format date time value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="value">The value.</param>
    private void FormatDateTimeValue(RichTextBox output, object value) =>
        this.OutputText(output, string.Concat("\"", ((IFormattable)value).ToString("O", CultureInfo.CurrentCulture), "\""), RichTextThemeStyle.Scalar);

    /// <summary>The format double value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="doubleValue">The double value.</param>
    private void FormatDoubleValue(RichTextBox output, double doubleValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            if (double.IsNaN(doubleValue) || double.IsInfinity(doubleValue))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString(doubleValue.ToString(CultureInfo.CurrentCulture), buffer);
                output.AppendText(buffer.ToString());
                return;
            }

            output.AppendText(doubleValue.ToString("R", CultureInfo.CurrentCulture));
        }
    }

    /// <summary>The format float value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="floatValue">The float value.</param>
    private void FormatFloatValue(RichTextBox output, float floatValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            if (double.IsNaN(floatValue) || double.IsInfinity(floatValue))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString(floatValue.ToString(CultureInfo.CurrentCulture), buffer);
                output.AppendText(buffer.ToString());
                return;
            }

            output.AppendText(floatValue.ToString("R", CultureInfo.CurrentCulture));
        }
    }

    /// <summary>The format literal value.</summary>
    /// <param name="scalar">The scalar.</param>
    /// <param name="output">The output.</param>
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

    /// <summary>The format null value.</summary>
    /// <param name="output">The output.</param>
    private void FormatNullValue(RichTextBox output) => this.OutputText(output, "null", RichTextThemeStyle.Null);

    /// <summary>The format number value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="value">The value.</param>
    private void FormatNumberValue(RichTextBox output, object value) =>
        this.OutputText(output, ((IFormattable)value).ToString(format: null, CultureInfo.CurrentCulture), RichTextThemeStyle.Number);

    /// <summary>The format scalar value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="value">The value.</param>
    private void FormatScalarValue(RichTextBox output, object value)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new ();
            JsonValueFormatter.WriteQuotedJsonString(value.ToString(), buffer);
            output.AppendText(buffer.ToString());
        }
    }

    /// <summary>The format string value.</summary>
    /// <param name="output">The output.</param>
    /// <param name="stringValue">The string value.</param>
    private void FormatStringValue(RichTextBox output, string stringValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.String))
        {
            using StringWriter buffer = new ();
            JsonValueFormatter.WriteQuotedJsonString(stringValue, buffer);
            output.AppendText(buffer.ToString());
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting;

using System.Globalization;

using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The themed display value formatter.</summary>
internal sealed class ThemedDisplayValueFormatter : ThemedValueFormatter
{
    /// <summary>The format provider.</summary>
    private readonly IFormatProvider? formatProvider;

    /// <summary>
    /// Initialises a new instance of the <see cref="ThemedDisplayValueFormatter"/> class.
    /// </summary>
    /// <param name="theme">
    /// The theme.
    /// </param>
    /// <param name="formatProvider">
    /// The format provider.
    /// </param>
    internal ThemedDisplayValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
        : base(theme) =>
        this.formatProvider = formatProvider;

    /// <summary>
    /// The format literal value.
    /// </summary>
    /// <param name="scalar">
    /// The scalar.
    /// </param>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="format">
    /// The format.
    /// </param>
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

    /// <summary>
    /// The visit dictionary value.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <param name="dictionary">
    /// The dictionary.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
    {
        return this.VisitDictionaryValueInternal(
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
    }

    /// <summary>
    /// The visit scalar value.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <param name="scalar">
    /// The scalar.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// </exception>
    protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
    {
        if (scalar is null)
        {
            throw new ArgumentNullException(nameof(scalar));
        }

        this.FormatLiteralValue(scalar, state.Output, state.Format);
        return 0;
    }

    /// <summary>
    /// The visit sequence value.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <param name="sequence">
    /// The sequence.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// </exception>
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

    /// <summary>
    /// The visit structure value.
    /// </summary>
    /// <param name="state">
    /// The state.
    /// </param>
    /// <param name="structure">
    /// The structure.
    /// </param>
    /// <returns>
    /// The <see cref="int"/>.
    /// </returns>
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

    /// <summary>
    /// The format boolean value.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="booleanValue">
    /// The boolean value.
    /// </param>
    private void FormatBooleanValue(RichTextBox output, bool booleanValue)
    {
        this.OutputText(output, booleanValue.ToString(CultureInfo.CurrentCulture), RichTextThemeStyle.Boolean);
    }

    /// <summary>
    /// The format character value.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="charValue">
    /// The char value.
    /// </param>
    private void FormatCharacterValue(RichTextBox output, char charValue)
    {
        this.OutputText(output, string.Concat("'", charValue.ToString(CultureInfo.CurrentCulture), "'"), RichTextThemeStyle.Scalar);
    }

    /// <summary>
    /// The format null value.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    private void FormatNullValue(RichTextBox output) => this.OutputText(output, "null", RichTextThemeStyle.Null);

    /// <summary>
    /// The format number value.
    /// </summary>
    /// <param name="scalar">
    /// The scalar.
    /// </param>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="format">
    /// The format.
    /// </param>
    private void FormatNumberValue(LogEventPropertyValue scalar, RichTextBox output, string format)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
        {
            using StringWriter buffer = new ();
            scalar.Render(buffer, format, this.formatProvider);
            output.AppendText(buffer.ToString());
        }
    }

    /// <summary>
    /// The format scalar value.
    /// </summary>
    /// <param name="scalar">
    /// The scalar.
    /// </param>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="format">
    /// The format.
    /// </param>
    private void FormatScalarValue(LogEventPropertyValue scalar, RichTextBox output, string format)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
        {
            using StringWriter buffer = new ();
            scalar.Render(buffer, format, this.formatProvider);
            output.AppendText(buffer.ToString());
        }
    }

    /// <summary>
    /// The format string value.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="format">
    /// The format.
    /// </param>
    /// <param name="stringValue">
    /// The string value.
    /// </param>
    private void FormatStringValue(RichTextBox output, string format, string stringValue)
    {
        using (this.ApplyStyle(output, RichTextThemeStyle.String))
        {
            if (!string.Equals(format, "l", StringComparison.Ordinal))
            {
                using StringWriter buffer = new ();
                JsonValueFormatter.WriteQuotedJsonString(stringValue, buffer);
                output.AppendText(buffer.ToString());

                return;
            }

            output.AppendText(stringValue);
        }
    }
}

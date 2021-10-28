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

public class ThemedJsonValueFormatter : ThemedValueFormatter
{
    private readonly ThemedDisplayValueFormatter displayFormatter;

    public ThemedJsonValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
        : base(theme) =>
        this.displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);

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

            RichTextThemeStyle style;
            if (scalarValue.Value == null)
            {
                style = RichTextThemeStyle.Null;
            }
            else
            {
                style = scalarValue.Value is string ? RichTextThemeStyle.String : RichTextThemeStyle.Scalar;
            }

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

        if (structure.TypeTag != null)
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
        var value = scalar.Value;

        switch (value)
        {
            case null:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Null))
                    {
                        output.AppendText("null");
                    }

                    break;
                }

            case string str:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.String))
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(str, buffer);
                        output.AppendText(buffer.ToString());
                    }

                    break;
                }

            case ValueType and (int or uint or long):
            case ValueType and (ulong or decimal or byte):
            case ValueType and (sbyte or short or ushort):
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                    {
                        output.AppendText(((IFormattable)value).ToString(null, CultureInfo.CurrentUICulture));
                    }

                    break;
                }

            case double d:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(d.ToString(CultureInfo.CurrentUICulture), buffer);
                            output.AppendText(buffer.ToString());
                            break;
                        }

                        output.AppendText(d.ToString("R", CultureInfo.CurrentUICulture));
                        break;
                    }
                }

            case float f:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                    {
                        if (double.IsNaN(f) || double.IsInfinity(f))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(f.ToString(CultureInfo.CurrentUICulture), buffer);
                            output.AppendText(buffer.ToString());
                            break;
                        }

                        output.AppendText(f.ToString("R", CultureInfo.CurrentUICulture));

                        break;
                    }
                }

            case bool b:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Boolean))
                    {
                        output.AppendText(b ? "true" : "false");
                    }

                    break;
                }

            case char ch:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(ch.ToString(CultureInfo.CurrentUICulture), buffer);
                        output.AppendText(buffer.ToString());
                    }

                    break;
                }

            case ValueType and (DateTime or DateTimeOffset):
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                    {
                        output.AppendText("\"");
                        output.AppendText(((IFormattable)value).ToString("O", CultureInfo.CurrentUICulture));
                        output.AppendText("\"");
                    }

                    break;
                }

            default:
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(value.ToString(), buffer);
                        output.AppendText(buffer.ToString());
                    }

                    break;
                }
        }
    }
}

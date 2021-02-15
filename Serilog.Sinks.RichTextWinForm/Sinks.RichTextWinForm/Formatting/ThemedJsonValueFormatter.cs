﻿// -----------------------------------------------------------------------
// <copyright file="ThemedJsonValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class ThemedJsonValueFormatter : ThemedValueFormatter
    {
        private readonly ThemedDisplayValueFormatter displayFormatter;

        private readonly IFormatProvider formatProvider;

        public ThemedJsonValueFormatter(RichTextTheme theme, IFormatProvider formatProvider)
            : base(theme)
        {
            this.displayFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);
            this.formatProvider = formatProvider;
        }

        public override ThemedValueFormatter SwitchTheme(RichTextTheme theme)
        {
            return new ThemedJsonValueFormatter(theme, this.formatProvider);
        }

        protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
        {
            var count = 0;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("{");
            }

            var delim = string.Empty;
            foreach (var element in dictionary.Elements)
            {
                if (delim.Length != 0)
                {
                    using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                    {
                        state.Output.AppendText(delim);
                    }
                }

                delim = ", ";

                var style = element.Key.Value == null ? RichTextThemeStyle.Null : element.Key.Value is string ? RichTextThemeStyle.String : RichTextThemeStyle.Scalar;

                using (this.ApplyStyle(state.Output, style, ref count))
                {
                    using StringWriter buffer = new();
                    JsonValueFormatter.WriteQuotedJsonString((element.Key.Value ?? "null").ToString(), buffer);
                    state.Output.AppendText(buffer.ToString());
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText(": ");
                }

                count += this.Visit(state.Nest(), element.Value);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
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
                return this.displayFormatter.FormatLiteralValue(scalar, state.Output, state.Format);
            }

            return this.FormatLiteralValue(scalar, state.Output);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var count = 0;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("[");
            }

            var delim = string.Empty;
            for (var index = 0; index < sequence.Elements.Count; ++index)
            {
                if (delim.Length != 0)
                {
                    using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                    {
                        state.Output.AppendText(delim);
                    }
                }

                delim = ", ";
                this.Visit(state.Nest(), sequence.Elements[index]);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("]");
            }

            return count;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("{");
            }

            var delim = string.Empty;
            for (var index = 0; index < structure.Properties.Count; ++index)
            {
                if (delim.Length != 0)
                {
                    using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                    {
                        state.Output.AppendText(delim);
                    }
                }

                delim = ", ";

                var property = structure.Properties[index];

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name, ref count))
                {
                    using StringWriter buffer = new();
                    JsonValueFormatter.WriteQuotedJsonString(property.Name, buffer);
                    state.Output.AppendText(buffer.ToString());
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText(": ");
                }

                count += this.Visit(state.Nest(), property.Value);
            }

            if (structure.TypeTag != null)
            {
                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText(delim);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name, ref count))
                {
                    using StringWriter buffer = new();
                    JsonValueFormatter.WriteQuotedJsonString("$type", buffer);
                    state.Output.AppendText(buffer.ToString());
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText(": ");
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.String, ref count))
                {
                    using StringWriter buffer = new();
                    JsonValueFormatter.WriteQuotedJsonString(structure.TypeTag, buffer);
                    state.Output.AppendText(buffer.ToString());
                }
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("}");
            }

            return count;
        }

        private int FormatLiteralValue(ScalarValue scalar, RichTextBox output)
        {
            var value = scalar.Value;
            var count = 0;

            if (value == null)
            {
                using (this.ApplyStyle(output, RichTextThemeStyle.Null, ref count))
                {
                    output.AppendText("null");
                }

                return count;
            }

            if (value is string str)
            {
                using (this.ApplyStyle(output, RichTextThemeStyle.String, ref count))
                {
                    using StringWriter buffer = new();
                    JsonValueFormatter.WriteQuotedJsonString(str, buffer);
                    output.AppendText(buffer.ToString());
                }

                return count;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong || value is decimal || value is byte || value is sbyte || value is short || value is ushort)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number, ref count))
                    {
                        output.AppendText(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));
                    }

                    return count;
                }

                if (value is double d)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number, ref count))
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(d.ToString(CultureInfo.InvariantCulture), buffer);
                            output.AppendText(buffer.ToString());
                        }
                        else
                        {
                            output.AppendText(d.ToString("R", CultureInfo.InvariantCulture));
                        }
                    }

                    return count;
                }

                if (value is float f)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number, ref count))
                    {
                        if (double.IsNaN(f) || double.IsInfinity(f))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(f.ToString(CultureInfo.InvariantCulture), buffer);
                            output.AppendText(buffer.ToString());
                        }
                        else
                        {
                            output.AppendText(f.ToString("R", CultureInfo.InvariantCulture));
                        }
                    }

                    return count;
                }

                if (value is bool b)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Boolean, ref count))
                    {
                        output.AppendText(b ? "true" : "false");
                    }

                    return count;
                }

                if (value is char ch)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar, ref count))
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(ch.ToString(), buffer);
                        output.AppendText(buffer.ToString());
                    }

                    return count;
                }

                if (value is DateTime || value is DateTimeOffset)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar, ref count))
                    {
                        output.AppendText("\"");
                        output.AppendText(((IFormattable)value).ToString("O", CultureInfo.InvariantCulture));
                        output.AppendText("\"");
                    }

                    return count;
                }
            }

            using (this.ApplyStyle(output, RichTextThemeStyle.Scalar, ref count))
            {
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(value.ToString(), buffer);
                output.AppendText(buffer.ToString());
            }

            return count;
        }
    }
}
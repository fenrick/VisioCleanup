// -----------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Formatting.Json;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class ThemedDisplayValueFormatter : ThemedValueFormatter
    {
        private readonly IFormatProvider formatProvider;

        public ThemedDisplayValueFormatter(RichTextTheme theme, IFormatProvider formatProvider)
            : base(theme)
        {
            this.formatProvider = formatProvider;
        }

        public int FormatLiteralValue(ScalarValue scalar, RichTextBox output, string format)
        {
            var value = scalar.Value;
            var count = 0;

            switch (value)
            {
                case null:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Null))
                        {
                            output.AppendText("null");
                        }

                        return count;
                    }

                case string str:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.String))
                        {
                            if (format != "l")
                            {
                                using StringWriter buffer = new();
                                JsonValueFormatter.WriteQuotedJsonString(str, buffer);
                                output.AppendText(buffer.ToString());

                                return count;
                            }

                            output.AppendText(str);

                            return count;
                        }
                    }

                case ValueType when value is int || value is uint || value is long || value is ulong || value is decimal || value is byte || value is sbyte || value is short
                                    || value is ushort || value is float || value is double:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        return count;
                    }

                case bool b:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Boolean))
                        {
                            output.AppendText(b.ToString());
                        }

                        return count;
                    }

                case char ch:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                        {
                            output.AppendText("\'");
                            output.AppendText(ch.ToString());
                            output.AppendText("\'");
                        }

                        return count;
                    }

                default:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        return count;
                    }
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
            foreach (var element in dictionary.Elements)
            {
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
                    count += this.Visit(state.Nest(), element.Key);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
                {
                    state.Output.AppendText("]=");
                }

                count += this.Visit(state.Nest(), element.Value);
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

            return this.FormatLiteralValue(scalar, state.Output, state.Format);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence is null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var count = 0;

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

            return count;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;

            if (structure.TypeTag != null)
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
    }
}
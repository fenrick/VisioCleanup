// -----------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
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
    using Serilog.Sinks.RichTextWinForm.Resources;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class ThemedDisplayValueFormatter : ThemedValueFormatter
    {
        private readonly IFormatProvider? formatProvider;

        public ThemedDisplayValueFormatter(RichTextTheme theme, IFormatProvider? formatProvider)
            : base(theme) =>
            this.formatProvider = formatProvider;

        public void FormatLiteralValue(ScalarValue scalar, RichTextBox output, string format)
        {
            var value = scalar.Value;

            switch (value)
            {
                case null:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Null))
                        {
                            output.AppendText(en_AU.ThemedDisplayValueFormatter_null);
                        }

                        break;
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

                                break;
                            }

                            output.AppendText(str);

                            break;
                        }
                    }

                case ValueType when value is int || value is uint || value is long:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        break;
                    }

                case ValueType when value is ulong || value is decimal || value is byte:
                
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        break;
                    }

                case ValueType when value is sbyte || value is short || value is ushort:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        break;
                    }

                case ValueType when value is float || value is double:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Number))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        break;
                    }

                case bool b:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Boolean))
                        {
                            output.AppendText(b.ToString(CultureInfo.CurrentUICulture));
                        }

                        break;
                    }

                case char ch:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                        {
                            output.AppendText(en_AU.ThemedDisplayValueFormatter_SingleQuote);
                            output.AppendText(ch.ToString(CultureInfo.CurrentUICulture));
                            output.AppendText(en_AU.ThemedDisplayValueFormatter_SingleQuote);
                        }

                        break;
                    }

                default:
                    {
                        using (this.ApplyStyle(output, RichTextThemeStyle.Scalar))
                        {
                            using StringWriter buffer = new();
                            scalar.Render(buffer, format, this.formatProvider);
                            output.AppendText(buffer.ToString());
                        }

                        break;
                    }
            }
        }

        protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
        {
            var count = 0;

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_LeftCurlyBracket);
            }

            var delim = string.Empty;
            foreach (var (scalarValue, logEventPropertyValue) in dictionary.Elements)
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
                    state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_LeftSquareBracket);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.String))
                {
                    count += this.Visit(state.Nest(), scalarValue);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
                {
                    state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_RightSquareBracket);
                    state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_Equals);
                }

                count += this.Visit(state.Nest(), logEventPropertyValue);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_RightCurlyBracket);
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
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_LeftSquareBracket);
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
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_RightSquareBracket);
            }

            return 0;
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

                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_Space);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_LeftCurlyBracket);
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
                    state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_Equals);
                }

                count += this.Visit(state.Nest(), property.Value);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText))
            {
                state.Output.AppendText(en_AU.ThemedDisplayValueFormatter_RightCurlyBracket);
            }

            return count;
        }
    }
}

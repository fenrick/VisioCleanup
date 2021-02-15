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

            if (value is null)
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
                    if (format != "l")
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(str, buffer);
                        output.AppendText(buffer.ToString());
                    }
                    else
                    {
                        output.AppendText(str);
                    }
                }

                return count;
            }

            if (value is ValueType)
            {
                if (value is int || value is uint || value is long || value is ulong || value is decimal || value is byte || value is sbyte || value is short || value is ushort
                    || value is float || value is double)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Number, ref count))
                    {
                        using StringWriter buffer = new();
                        scalar.Render(buffer, format, this.formatProvider);
                        output.AppendText(buffer.ToString());
                    }

                    return count;
                }

                if (value is bool b)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Boolean, ref count))
                    {
                        output.AppendText(b.ToString());
                    }

                    return count;
                }

                if (value is char ch)
                {
                    using (this.ApplyStyle(output, RichTextThemeStyle.Scalar, ref count))
                    {
                        output.AppendText("\'");
                        output.AppendText(ch.ToString());
                        output.AppendText("\'");
                    }

                    return count;
                }
            }

            using (this.ApplyStyle(output, RichTextThemeStyle.Scalar, ref count))
            {
                using StringWriter buffer = new();
                scalar.Render(buffer, format, this.formatProvider);
                output.AppendText(buffer.ToString());
            }

            return count;
        }

        public override ThemedValueFormatter SwitchTheme(RichTextTheme theme)
        {
            return new ThemedDisplayValueFormatter(theme, this.formatProvider);
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

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText("[");
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.String, ref count))
                {
                    count += this.Visit(state.Nest(), element.Key);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText("]=");
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

            return this.FormatLiteralValue(scalar, state.Output, state.Format);
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence is null)
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
                this.Visit(state, sequence.Elements[index]);
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

            if (structure.TypeTag != null)
            {
                using (this.ApplyStyle(state.Output, RichTextThemeStyle.Name, ref count))
                {
                    state.Output.AppendText(structure.TypeTag);
                }

                state.Output.AppendText(" ");
            }

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
                    state.Output.AppendText(property.Name);
                }

                using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
                {
                    state.Output.AppendText("=");
                }

                count += this.Visit(state.Nest(), property.Value);
            }

            using (this.ApplyStyle(state.Output, RichTextThemeStyle.TertiaryText, ref count))
            {
                state.Output.AppendText("}");
            }

            return count;
        }
    }
}
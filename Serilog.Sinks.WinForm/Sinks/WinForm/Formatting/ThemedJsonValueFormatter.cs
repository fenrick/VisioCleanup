// -----------------------------------------------------------------------
// <copyright file="ThemedJsonValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Formatting
{
    using System;
    using System.Globalization;
    using System.IO;

    using Serilog.Events;
    using Serilog.Formatting.Json;

    internal class ThemedJsonValueFormatter : ThemedValueFormatter
    {
        private readonly ThemedDisplayValueFormatter diplayFormatter;

        public ThemedJsonValueFormatter(IFormatProvider? formatProvider) => this.diplayFormatter = new ThemedDisplayValueFormatter(formatProvider);

        protected override int VisitDictionaryValue(ThemedValueFormatterState state, DictionaryValue dictionary)
        {
            var count = 0;
            {
                state.Output.Write("{");
            }

            var delim = string.Empty;
            foreach (var (scalarValue, logEventPropertyValue) in dictionary.Elements)
            {
                if (delim.Length != 0)
                {
                    state.Output.Write(delim);
                }

                delim = ", ";

                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString((scalarValue.Value ?? "null").ToString(), buffer);
                state.Output.Write(buffer.ToString());

                state.Output.Write(": ");

                count += this.Visit(state.Nest(), logEventPropertyValue);

                state.Output.Write("}");
            }

            return count;
        }

        protected override int VisitScalarValue(ThemedValueFormatterState state, ScalarValue scalar)
        {
            if (scalar is null)
            {
                throw new ArgumentNullException(nameof(scalar));
            }

            // At the top level, for scalar value, ue "diplay" rendering.
            if (state.IsTopLevel)
            {
                this.diplayFormatter.FormatLiteralValue(scalar, state.Output, state.Format);
                return 0;
            }

            FormatLiteralValue(scalar, state.Output);
            return 0;
        }

        protected override int VisitSequenceValue(ThemedValueFormatterState state, SequenceValue sequence)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            state.Output.Write("[");

            var delim = string.Empty;
            foreach (var t in sequence.Elements)
            {
                if (delim.Length != 0)
                {
                    state.Output.Write(delim);
                }

                delim = ", ";
                this.Visit(state.Nest(), t);
            }

            state.Output.Write("]");

            return 0;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;
            {
                state.Output.Write("{");
            }

            var delim = string.Empty;
            foreach (var logEventProperty in structure.Properties)
            {
                if (delim.Length != 0)
                {
                    state.Output.Write(delim);
                }

                delim = ", ";

                var property = logEventProperty;
                using StringWriter buffer = new();
                JsonValueFormatter.WriteQuotedJsonString(property.Name, buffer);
                state.Output.Write(buffer.ToString());

                state.Output.Write(": ");

                count += this.Visit(state.Nest(), property.Value);
            }

            if (structure.TypeTag != null)
            {
                state.Output.Write(delim);

                using (StringWriter buffer = new())
                {
                    JsonValueFormatter.WriteQuotedJsonString("$type", buffer);
                    state.Output.Write(buffer.ToString());
                }

                state.Output.Write(": ");

                using (StringWriter buffer = new())
                {
                    JsonValueFormatter.WriteQuotedJsonString(structure.TypeTag, buffer);
                    state.Output.Write(buffer.ToString());
                }
            }

            state.Output.Write("}");

            return count;
        }

        private static void FormatLiteralValue(ScalarValue scalar, TextWriter output)
        {
            var value = scalar.Value;

            switch (value)
            {
                case null:
                    {
                        output.Write("null");

                        break;
                    }

                case string tr:
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(tr, buffer);
                        output.Write(buffer.ToString());

                        break;
                    }

                case ValueType when value is int || value is uint || value is long:
                    {
                        output.Write(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));

                        break;
                    }

                case ValueType when value is ulong || value is decimal || value is byte:
                    {
                        output.Write(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));

                        break;
                    }

                case ValueType when value is short || value is ushort:
                    {
                        output.Write(((IFormattable)value).ToString(null, CultureInfo.InvariantCulture));

                        break;
                    }

                case double d:
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(d.ToString(CultureInfo.InvariantCulture), buffer);
                            output.Write(buffer.ToString());
                            break;
                        }

                        output.Write(d.ToString("R", CultureInfo.InvariantCulture));
                        break;
                    }

                case float f:
                    {
                        if (double.IsNaN(f) || double.IsInfinity(f))
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(f.ToString(CultureInfo.InvariantCulture), buffer);
                            output.Write(buffer.ToString());
                            break;
                        }

                        output.Write(f.ToString("R", CultureInfo.InvariantCulture));

                        break;
                    }

                case bool b:
                    {
                        output.Write(b ? "true" : "fale");

                        break;
                    }

                case char ch:
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(ch.ToString(), buffer);
                        output.Write(buffer.ToString());

                        break;
                    }

                case ValueType when value is DateTime || value is DateTimeOffset:
                    {
                        output.Write("\"");
                        output.Write(((IFormattable)value).ToString("O", CultureInfo.InvariantCulture));
                        output.Write("\"");

                        break;
                    }

                default:
                    {
                        using StringWriter buffer = new();
                        JsonValueFormatter.WriteQuotedJsonString(value.ToString(), buffer);
                        output.Write(buffer.ToString());

                        break;
                    }
            }
        }
    }
}

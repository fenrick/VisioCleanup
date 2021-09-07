// -----------------------------------------------------------------------
// <copyright file="ThemedDisplayValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Formatting
{
    using System;
    using System.IO;

    using Serilog.Events;
    using Serilog.Formatting.Json;

    internal class ThemedDisplayValueFormatter : ThemedValueFormatter
    {
        private readonly IFormatProvider? formatProvider;

        public ThemedDisplayValueFormatter(IFormatProvider? formatProvider) => this.formatProvider = formatProvider;

        public void FormatLiteralValue(ScalarValue scalar, TextWriter output, string format)
        {
            var value = scalar.Value;

            switch (value)
            {
                case null:
                    {
                        output.Write("null");

                        break;
                    }

                case string str:
                    {
                        if (format != "l")
                        {
                            using StringWriter buffer = new();
                            JsonValueFormatter.WriteQuotedJsonString(str, buffer);
                            output.Write(buffer.ToString());

                            break;
                        }

                        output.Write(str);

                        break;
                    }

                case bool b:
                    {
                        output.Write(b.ToString());

                        break;
                    }

                case char ch:
                    {
                        output.Write("\'");
                        output.Write(ch.ToString());
                        output.Write("\'");

                        break;
                    }

                default:
                    {
                        using StringWriter buffer = new();
                        scalar.Render(buffer, format, this.formatProvider);
                        output.Write(buffer.ToString());

                        break;
                    }
            }
        }

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

                state.Output.Write("[");

                count += this.Visit(state.Nest(), scalarValue);

                state.Output.Write("]=");

                count += this.Visit(state.Nest(), logEventPropertyValue);
            }

            state.Output.Write("}");

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

            state.Output.Write("[");

            var delim = string.Empty;
            foreach (var t in sequence.Elements)
            {
                if (delim.Length != 0)
                {
                    state.Output.Write(delim);
                }

                delim = ", ";
                this.Visit(state, t);
            }

            state.Output.Write("]");

            return 0;
        }

        protected override int VisitStructureValue(ThemedValueFormatterState state, StructureValue structure)
        {
            var count = 0;

            if (structure.TypeTag != null)
            {
                state.Output.Write(structure.TypeTag);

                state.Output.Write(" ");
            }

            state.Output.Write("{");

            var delim = string.Empty;
            foreach (var logEventProperty in structure.Properties)
            {
                if (delim.Length != 0)
                {
                    state.Output.Write(delim);
                }

                delim = ", ";

                var property = logEventProperty;

                state.Output.Write(property.Name);

                state.Output.Write("=");

                count += this.Visit(state.Nest(), property.Value);
            }

            state.Output.Write("}");

            return count;
        }
    }
}

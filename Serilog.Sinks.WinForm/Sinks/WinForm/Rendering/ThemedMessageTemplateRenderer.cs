// -----------------------------------------------------------------------
// <copyright file="ThemedMessageTemplateRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Rendering
{
    using System.Collections.Generic;
    using System.IO;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.WinForm.Formatting;

    internal class ThemedMessageTemplateRenderer
    {
        private readonly bool isLiteral;

        private readonly ThemedValueFormatter valueFormatter;

        public ThemedMessageTemplateRenderer(ThemedValueFormatter valueFormatter, bool isLiteral)
        {
            this.valueFormatter = valueFormatter;
            this.isLiteral = isLiteral;
        }

        public void Render(MessageTemplate template, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            foreach (var token in template.Tokens)
            {
                if (token is TextToken tt)
                {
                    this.RenderTextToken(tt, output);
                    continue;
                }

                var pt = (PropertyToken)token;
                this.RenderPropertyToken(pt, properties, output);
            }
        }

        private void RenderAlignedPropertyTokenUnbuffered(PropertyToken pt, TextWriter output, LogEventPropertyValue propertyValue)
        {
            this.RenderValue(this.valueFormatter, propertyValue, output, pt.Format);
        }

        private void RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
            {
                {
                    output.Write(pt.ToString());
                }

                return;
            }

            if (!pt.Alignment.HasValue)
            {
                this.RenderValue(this.valueFormatter, propertyValue, output, pt.Format);
                return;
            }

            this.RenderAlignedPropertyTokenUnbuffered(pt, output, propertyValue);
        }

        private void RenderTextToken(TextToken tt, TextWriter output)
        {
            {
                output.Write(tt.Text);
            }
        }

        private void RenderValue(ThemedValueFormatter themedValueFormatter, LogEventPropertyValue propertyValue, TextWriter output, string format)
        {
            if (this.isLiteral && propertyValue is ScalarValue { Value: string } sv)
            {
                {
                    output.Write(sv.Value.ToString() ?? string.Empty);
                }

                return;
            }

            themedValueFormatter.Format(propertyValue, output, format, this.isLiteral);
        }
    }
}
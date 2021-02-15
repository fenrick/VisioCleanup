// -----------------------------------------------------------------------
// <copyright file="ThemedMessageTemplateRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Rendering
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Formatting;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class ThemedMessageTemplateRenderer
    {
        private static readonly RichTextTheme noTheme = new EmptyRichTextTheme();

        private readonly bool isLiteral;

        private readonly RichTextTheme theme;

        private readonly ThemedValueFormatter unthemedValueFormatter;

        private readonly ThemedValueFormatter valueFormatter;

        public ThemedMessageTemplateRenderer(RichTextTheme theme, ThemedValueFormatter valueFormatter, bool isLiteral)
        {
            this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
            this.valueFormatter = valueFormatter;
            this.isLiteral = isLiteral;
            this.unthemedValueFormatter = valueFormatter.SwitchTheme(noTheme);
        }

        public int Render(MessageTemplate template, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextBox output)
        {
            var count = 0;
            foreach (var token in template.Tokens)
            {
                if (token is TextToken tt)
                {
                    count += this.RenderTextToken(tt, output);
                }
                else
                {
                    var pt = (PropertyToken)token;
                    count += this.RenderPropertyToken(pt, properties, output);
                }
            }

            return count;
        }

        private int RenderAlignedPropertyTokenUnbuffered(PropertyToken pt, RichTextBox output, LogEventPropertyValue propertyValue)
        {
            return this.RenderValue(this.theme, this.valueFormatter, propertyValue, output, pt.Format);
        }

        private int RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextBox output)
        {
            if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
            {
                var count = 0;
                using (this.theme.Apply(output, RichTextThemeStyle.Invalid, ref count))
                {
                    output.AppendText(pt.ToString());
                }

                return count;
            }

            if (!pt.Alignment.HasValue)
            {
                return this.RenderValue(this.theme, this.valueFormatter, propertyValue, output, pt.Format);
            }

            return this.RenderAlignedPropertyTokenUnbuffered(pt, output, propertyValue);
        }

        private int RenderTextToken(TextToken tt, RichTextBox output)
        {
            var count = 0;
            using (this.theme.Apply(output, RichTextThemeStyle.Text, ref count))
            {
                output.AppendText(tt.Text);
            }

            return count;
        }

        private int RenderValue(RichTextTheme theme, ThemedValueFormatter valueFormatter, LogEventPropertyValue propertyValue, RichTextBox output, string format)
        {
            var count = 0;
            if (this.isLiteral && propertyValue is ScalarValue sv && sv.Value is string)
            {
                using (theme.Apply(output, RichTextThemeStyle.String, ref count))
                {
                    output.AppendText(sv.Value.ToString());
                }

                return count;
            }

            return valueFormatter.Format(propertyValue, output, format, this.isLiteral);
        }
    }
}
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
        private readonly bool isLiteral;

        private readonly RichTextTheme theme;

        private readonly ThemedValueFormatter valueFormatter;

        public ThemedMessageTemplateRenderer(RichTextTheme theme, ThemedValueFormatter valueFormatter, bool isLiteral)
        {
            this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
            this.valueFormatter = valueFormatter;
            this.isLiteral = isLiteral;
        }

        public void Render(MessageTemplate template, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextBox output)
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

        private void RenderAlignedPropertyTokenUnbuffered(PropertyToken pt, RichTextBox output, LogEventPropertyValue propertyValue)
        {
            this.RenderValue(this.theme, this.valueFormatter, propertyValue, output, pt.Format);
        }

        private void RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, RichTextBox output)
        {
            if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
            {
                using (this.theme.Apply(output, RichTextThemeStyle.Invalid))
                {
                    output.AppendText(pt.ToString());
                }

                return;
            }

            if (!pt.Alignment.HasValue)
            {
                this.RenderValue(this.theme, this.valueFormatter, propertyValue, output, pt.Format);
                return;
            }

            this.RenderAlignedPropertyTokenUnbuffered(pt, output, propertyValue);
        }

        private void RenderTextToken(TextToken tt, RichTextBox output)
        {
            using (this.theme.Apply(output, RichTextThemeStyle.Text))
            {
                output.AppendText(tt.Text);
            }
        }

        private void RenderValue(RichTextTheme richTextTheme, ThemedValueFormatter themedValueFormatter, LogEventPropertyValue propertyValue, RichTextBox output, string format)
        {
            if (this.isLiteral && propertyValue is ScalarValue { Value: string } sv)
            {
                using (richTextTheme.Apply(output, RichTextThemeStyle.String))
                {
                    output.AppendText(sv.Value.ToString() ?? string.Empty);
                }

                return;
            }

            themedValueFormatter.Format(propertyValue, output, format, this.isLiteral);
        }
    }
}
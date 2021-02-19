// -----------------------------------------------------------------------
// <copyright file="PropertiesTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Formatting;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class PropertiesTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly MessageTemplate outputTemplate;

        private readonly ThemedValueFormatter valueFormatter;

        public PropertiesTokenRenderer(RichTextTheme theme, PropertyToken token, MessageTemplate outputTemplate, IFormatProvider formatProvider)
        {
            this.outputTemplate = outputTemplate;
            var isJson = false;

            if (token.Format != null)
            {
                foreach (var c in token.Format)
                {
                    if (c == 'j')
                    {
                        isJson = true;
                    }
                }
            }

            this.valueFormatter = isJson ? new ThemedJsonValueFormatter(theme, formatProvider) : new ThemedDisplayValueFormatter(theme, formatProvider);
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            var included = logEvent.Properties
                .Where(p => !TemplateContainsPropertyName(logEvent.MessageTemplate, p.Key) && !TemplateContainsPropertyName(this.outputTemplate, p.Key))
                .Select(p => new LogEventProperty(p.Key, p.Value));

            var value = new StructureValue(included);

            this.valueFormatter.Format(value, output, string.Empty);
        }

        private static bool TemplateContainsPropertyName(MessageTemplate template, string propertyName)
        {
            foreach (var token in template.Tokens)
            {
                if (token is PropertyToken namedProperty && (namedProperty.PropertyName == propertyName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
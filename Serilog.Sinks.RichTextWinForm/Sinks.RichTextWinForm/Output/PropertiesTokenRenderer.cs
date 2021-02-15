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

        private readonly RichTextTheme theme;

        private readonly PropertyToken token;

        private readonly ThemedValueFormatter valueFormatter;

        public PropertiesTokenRenderer(RichTextTheme theme, PropertyToken token, MessageTemplate outputTemplate, IFormatProvider formatProvider)
        {
            this.outputTemplate = outputTemplate;
            this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
            this.token = token ?? throw new ArgumentNullException(nameof(token));
            var isJson = false;

            if (token.Format != null)
            {
                for (var i = 0; i < token.Format.Length; ++i)
                {
                    if (token.Format[i] == 'j')
                    {
                        isJson = true;
                    }
                }
            }

            this.valueFormatter = isJson ? (ThemedValueFormatter)new ThemedJsonValueFormatter(theme, formatProvider) : new ThemedDisplayValueFormatter(theme, formatProvider);
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            var included = logEvent.Properties
                .Where(p => !TemplateContainsPropertyName(logEvent.MessageTemplate, p.Key) && !TemplateContainsPropertyName(this.outputTemplate, p.Key))
                .Select(p => new LogEventProperty(p.Key, p.Value));

            var value = new StructureValue(included);

            this.valueFormatter.Format(value, output, null);
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
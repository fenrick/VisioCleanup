// -----------------------------------------------------------------------
// <copyright file="MessageTemplateOutputTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Formatting;
    using Serilog.Sinks.RichTextWinForm.Rendering;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class MessageTemplateOutputTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly ThemedMessageTemplateRenderer renderer;

        private readonly RichTextTheme theme;

        private readonly PropertyToken token;

        public MessageTemplateOutputTokenRenderer(RichTextTheme theme, PropertyToken token, IFormatProvider formatProvider)
        {
            this.theme = theme ?? throw new ArgumentNullException(nameof(theme));
            this.token = token ?? throw new ArgumentNullException(nameof(token));
            bool isLiteral = false, isJson = false;

            if (token.Format != null)
            {
                for (var i = 0; i < token.Format.Length; ++i)
                {
                    if (token.Format[i] == 'l')
                    {
                        isLiteral = true;
                    }
                    else if (token.Format[i] == 'j')
                    {
                        isJson = true;
                    }
                }
            }

            var valueFormatter = isJson ? (ThemedValueFormatter)new ThemedJsonValueFormatter(theme, formatProvider) : new ThemedDisplayValueFormatter(theme, formatProvider);

            this.renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            this.renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
        }
    }
}
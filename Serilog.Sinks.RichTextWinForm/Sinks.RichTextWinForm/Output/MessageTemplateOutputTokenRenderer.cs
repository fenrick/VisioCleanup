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

        public MessageTemplateOutputTokenRenderer(RichTextTheme theme, PropertyToken token, IFormatProvider formatProvider)
        {
            bool isLiteral = false, isJson = false;

            if (token.Format != null)
            {
                foreach (var character in token.Format)
                {
                    switch (character)
                    {
                        case 'l':
                            isLiteral = true;
                            break;
                        case 'j':
                            isJson = true;
                            break;
                    }
                }
            }

            ThemedValueFormatter valueFormatter;
            if (isJson)
            {
                valueFormatter = new ThemedJsonValueFormatter(theme, formatProvider);
            }
            else
            {
                valueFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);
            }

            this.renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            this.renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
        }
    }
}
// -----------------------------------------------------------------------
// <copyright file="MessageTemplateOutputTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System;
    using System.IO;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.WinForm.Formatting;
    using Serilog.Sinks.WinForm.Rendering;

    internal class MessageTemplateOutputTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly ThemedMessageTemplateRenderer renderer;

        public MessageTemplateOutputTokenRenderer(PropertyToken token, IFormatProvider? formatProvider)
        {
            bool isLiteral = false, isJson = false;

            if (token.Format != null)
            {
                foreach (var character in token.Format)
                {
                    if (character == 'l')
                    {
                        isLiteral = true;
                    }
                    
                    if (character == 'j')
                    {
                        isJson = true;
                    }
                }
            }

            ThemedValueFormatter valueFormatter;
            if (isJson)
            {
                valueFormatter = new ThemedJsonValueFormatter(formatProvider);
            }
            else
            {
                valueFormatter = new ThemedDisplayValueFormatter(formatProvider);
            }

            this.renderer = new ThemedMessageTemplateRenderer(valueFormatter, isLiteral);
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            this.renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
        }
    }
}

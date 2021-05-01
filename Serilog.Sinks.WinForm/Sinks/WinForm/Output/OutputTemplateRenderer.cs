// -----------------------------------------------------------------------
// <copyright file="OutputTemplateRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;
    using Serilog.Parsing;

    /// <summary>Render output as per .</summary>
    public class OutputTemplateRenderer : ITextFormatter
    {
        private readonly OutputTemplateTokenRenderer[] outputTemplateTokenRenderers;

        /// <summary>Initialises a new instance of the <see cref="OutputTemplateRenderer" /> class.</summary>
        /// <param name="outputTemplate">Template for output.</param>
        /// <param name="formatProvider"><see cref="OutputTemplateRenderer.Format" /> provider.</param>
        /// <exception cref="System.ArgumentNullException">No output template.</exception>
        public OutputTemplateRenderer(string outputTemplate, IFormatProvider? formatProvider)
        {
            if (outputTemplate is null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var template = new MessageTemplateParser().Parse(outputTemplate);

            var templateTokenRenderers = new List<OutputTemplateTokenRenderer>();
            foreach (var token in template.Tokens)
            {
                if (token is TextToken tt)
                {
                    templateTokenRenderers.Add(new TextTokenRenderer(tt.Text));
                    continue;
                }

                var pt = (PropertyToken)token;
                switch (pt.PropertyName)
                {
                    case OutputProperties.LevelPropertyName:
                        templateTokenRenderers.Add(new LevelTokenRenderer(pt));
                        break;
                    case OutputProperties.NewLinePropertyName:
                        templateTokenRenderers.Add(new NewLineTokenRenderer(pt.Alignment));
                        break;
                    case OutputProperties.ExceptionPropertyName:
                        templateTokenRenderers.Add(new ExceptionTokenRenderer());
                        break;
                    case OutputProperties.MessagePropertyName:
                        templateTokenRenderers.Add(new MessageTemplateOutputTokenRenderer(pt, formatProvider));
                        break;
                    case OutputProperties.TimestampPropertyName:
                        templateTokenRenderers.Add(new TimestampTokenRenderer(pt, formatProvider));
                        break;
                    case "Properties":
                        templateTokenRenderers.Add(new PropertiesTokenRenderer(pt, template, formatProvider));
                        break;
                    default:
                        templateTokenRenderers.Add(new EventPropertyTokenRenderer(pt, formatProvider));
                        break;
                }
            }

            this.outputTemplateTokenRenderers = templateTokenRenderers.ToArray();
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            foreach (var renderer in this.outputTemplateTokenRenderers)
            {
                renderer.Render(logEvent, output);
            }
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="OutputTemplateRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Formatting;
    using Serilog.Formatting.Display;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Themes;

    /// <summary>Render output as per theme.</summary>
    public class OutputTemplateRenderer : ITextFormatter
    {
        private readonly OutputTemplateTokenRenderer[] renderers;

        /// <summary>Initialises a new instance of the <see cref="OutputTemplateRenderer" /> class.</summary>
        /// <param name="theme">Rich text theme.</param>
        /// <param name="outputTemplate">Template for output.</param>
        /// <param name="formatProvider">Format provider.</param>
        /// <exception cref="ArgumentNullException">No output template.</exception>
        public OutputTemplateRenderer(RichTextTheme theme, string outputTemplate, IFormatProvider formatProvider)
        {
            if (outputTemplate is null)
            {
                throw new ArgumentNullException(nameof(outputTemplate));
            }

            var template = new MessageTemplateParser().Parse(outputTemplate);

            var renderers = new List<OutputTemplateTokenRenderer>();
            foreach (var token in template.Tokens)
            {
                if (token is TextToken tt)
                {
                    renderers.Add(new TextTokenRenderer(theme, tt.Text));
                    continue;
                }

                var pt = (PropertyToken)token;
                if (pt.PropertyName == OutputProperties.LevelPropertyName)
                {
                    renderers.Add(new LevelTokenRenderer(theme, pt));
                }
                else if (pt.PropertyName == OutputProperties.NewLinePropertyName)
                {
                    renderers.Add(new NewLineTokenRenderer(pt.Alignment));
                }
                else if (pt.PropertyName == OutputProperties.ExceptionPropertyName)
                {
                    renderers.Add(new ExceptionTokenRenderer(theme));
                }
                else if (pt.PropertyName == OutputProperties.MessagePropertyName)
                {
                    renderers.Add(new MessageTemplateOutputTokenRenderer(theme, pt, formatProvider));
                }
                else if (pt.PropertyName == OutputProperties.TimestampPropertyName)
                {
                    renderers.Add(new TimestampTokenRenderer(theme, pt, formatProvider));
                }
                else if (pt.PropertyName == "Properties")
                {
                    renderers.Add(new PropertiesTokenRenderer(theme, pt, template, formatProvider));
                }
                else
                {
                    renderers.Add(new EventPropertyTokenRenderer(theme, pt, formatProvider));
                }
            }

            this.renderers = renderers.ToArray();
        }

        /// <inheritdoc />
        public void Format(LogEvent logEvent, TextWriter output)
        {
            throw new InvalidOperationException("Not valid for this Sink!");
        }

        /// <summary>Format log events to a richtextbox.</summary>
        /// <param name="logEvent">Log event.</param>
        /// <param name="output">A rich textbox.</param>
        /// <exception cref="ArgumentNullException">Required parameter's not supplied.</exception>
        public void Format(LogEvent logEvent, RichTextBox output)
        {
            if (logEvent is null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            foreach (var renderer in this.renderers)
            {
                renderer.Render(logEvent, output);
            }
        }
    }
}
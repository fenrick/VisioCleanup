// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputTemplateRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Themes;

using TextWriter = System.IO.TextWriter;

/// <summary>Render output as per theme.</summary>
public class OutputTemplateRenderer : ITextFormatter
{
    /// <summary>The output template token renderers.</summary>
    private readonly IOutputTemplateTokenRenderer[] outputTemplateTokenRenderers;

    /// <summary>Initialises a new instance of the <see cref="OutputTemplateRenderer"/> class. Initialises a new instance of
    /// the <see cref="OutputTemplateRenderer"/> class.</summary>
    /// <param name="theme">Rich text theme.</param>
    /// <param name="outputTemplate">Template for output.</param>
    /// <param name="formatProvider">FormatProvider.</param>
    /// <exception cref="System.ArgumentNullException">No output template.</exception>
    internal OutputTemplateRenderer(RichTextTheme theme, string outputTemplate, IFormatProvider? formatProvider)
    {
        if (outputTemplate is null)
        {
            throw new ArgumentNullException(nameof(outputTemplate));
        }

        var template = new MessageTemplateParser().Parse(outputTemplate);

        var templateTokenRenderers = new List<IOutputTemplateTokenRenderer>();
        foreach (var token in template.Tokens)
        {
            if (token is TextToken tt)
            {
                templateTokenRenderers.Add(new TextTokenRenderer(theme, tt.Text));
                continue;
            }

            var pt = (PropertyToken)token;
            switch (pt.PropertyName)
            {
                case OutputProperties.LevelPropertyName:
                    templateTokenRenderers.Add(new LevelTokenRenderer(theme, pt));
                    break;
                case OutputProperties.NewLinePropertyName:
                    templateTokenRenderers.Add(new NewLineTokenRenderer(pt.Alignment));
                    break;
                case OutputProperties.ExceptionPropertyName:
                    templateTokenRenderers.Add(new ExceptionTokenRenderer(theme));
                    break;
                case OutputProperties.MessagePropertyName:
                    templateTokenRenderers.Add(new MessageTemplateOutputTokenRenderer(theme, pt, formatProvider));
                    break;
                case OutputProperties.TimestampPropertyName:
                    templateTokenRenderers.Add(new TimestampTokenRenderer(theme, pt, formatProvider));
                    break;
                case "Properties":
                    templateTokenRenderers.Add(new PropertiesTokenRenderer(theme, pt, template, formatProvider));
                    break;
                default:
                    templateTokenRenderers.Add(new EventPropertyTokenRenderer(theme, pt, formatProvider));
                    break;
            }
        }

        this.outputTemplateTokenRenderers = templateTokenRenderers.ToArray();
    }

    /// <inheritdoc />
    public void Format(LogEvent logEvent, TextWriter output) => throw new InvalidOperationException("Not valid for this Sink!");

    /// <summary>Render log events to a richtextbox.</summary>
    /// <param name="logEvent"><see cref="Log"/> event.</param>
    /// <param name="output">A rich textbox.</param>
    /// <exception cref="System.ArgumentNullException">Required parameter's not supplied.</exception>
    internal void Format(LogEvent logEvent, RichTextBox output)
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

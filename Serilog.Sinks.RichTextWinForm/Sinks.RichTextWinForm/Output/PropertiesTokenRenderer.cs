// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertiesTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Formatting;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The properties token renderer.</summary>
internal sealed class PropertiesTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The output template.</summary>
    private readonly MessageTemplate outputTemplate;

    /// <summary>The value formatter.</summary>
    private readonly ThemedValueFormatter valueFormatter;

    /// <summary>Initialises a new instance of the <see cref="PropertiesTokenRenderer"/> class.</summary>
    /// <param name="theme">The theme.</param>
    /// <param name="token">The token.</param>
    /// <param name="outputTemplate">The output template.</param>
    /// <param name="formatProvider">The format provider.</param>
    internal PropertiesTokenRenderer(RichTextTheme theme, PropertyToken token, MessageTemplate outputTemplate, IFormatProvider? formatProvider)
    {
        this.outputTemplate = outputTemplate;
        var isJson = false;

        if (!string.IsNullOrEmpty(token.Format))
        {
            foreach (var dummy in token.Format.Where(c => c == 'j'))
            {
                isJson = true;
            }
        }

        this.valueFormatter = isJson ? new ThemedJsonValueFormatter(theme, formatProvider) : new ThemedDisplayValueFormatter(theme, formatProvider);
    }

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        var included = logEvent.Properties.Where(p => !TemplateContainsPropertyName(logEvent.MessageTemplate, p.Key) && !TemplateContainsPropertyName(this.outputTemplate, p.Key))
            .Select(p => new LogEventProperty(p.Key, p.Value));

        StructureValue value = new (included);

        this.valueFormatter.Format(value, output, string.Empty);
    }

    /// <summary>The template contains property name.</summary>
    /// <param name="template">The template.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The <see cref="bool"/>.</returns>
    private static bool TemplateContainsPropertyName(MessageTemplate template, string propertyName)
    {
        foreach (var token in template.Tokens)
        {
            if (token is PropertyToken namedProperty && string.Equals(namedProperty.PropertyName, propertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

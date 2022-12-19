// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventPropertyTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Rendering;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The event property token renderer.</summary>
internal sealed class EventPropertyTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The format provider.</summary>
    private readonly IFormatProvider? formatProvider;

    /// <summary>The theme.</summary>
    private readonly RichTextTheme theme;

    /// <summary>The token.</summary>
    private readonly PropertyToken token;

    /// <summary>Initialises a new instance of the <see cref="EventPropertyTokenRenderer"/> class.</summary>
    /// <param name="theme">The theme.</param>
    /// <param name="token">The token.</param>
    /// <param name="formatProvider">The format provider.</param>
    internal EventPropertyTokenRenderer(RichTextTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        this.theme = theme;
        this.token = token;
        this.formatProvider = formatProvider;
    }

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        // If a property is missing, don't render anything (message templates render the raw token here).
        if (!logEvent.Properties.TryGetValue(this.token.PropertyName, out var propertyValue))
        {
            Padding.Apply(output, string.Empty, this.token.Alignment);
            return;
        }

        using (this.theme.Apply(output, RichTextThemeStyle.SecondaryText))
        {
            using StringWriter writer = new ();

            // If the value is a scalar string, support some additional formats: 'u' for uppercase
            // and 'w' for lowercase.
            if (propertyValue is ScalarValue { Value: string literalString })
            {
                var cased = Casing.Format(literalString, this.token.Format);
                writer.Write(cased);
            }
            else
            {
                propertyValue.Render(writer, this.token.Format, this.formatProvider);
            }

            if (this.token.Alignment.HasValue)
            {
                var str = writer.ToString();
                Padding.Apply(output, str, this.token.Alignment);
            }
            else
            {
                output.AppendText(writer.ToString());
            }
        }
    }
}

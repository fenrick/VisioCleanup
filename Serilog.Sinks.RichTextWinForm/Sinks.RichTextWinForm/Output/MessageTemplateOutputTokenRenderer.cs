// -----------------------------------------------------------------------
// <copyright file="MessageTemplateOutputTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using System;
using System.Windows.Forms;

using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Formatting;
using Serilog.Sinks.RichTextWinForm.Rendering;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>Message Template Output Token Renderer.</summary>
internal sealed class MessageTemplateOutputTokenRenderer : IOutputTemplateTokenRenderer
{
    private readonly ThemedMessageTemplateRenderer renderer;

    /// <summary>Initialises a new instance of the <see cref="MessageTemplateOutputTokenRenderer" /> class.</summary>
    /// <param name="theme">Theme.</param>
    /// <param name="token">Token.</param>
    /// <param name="formatProvider">Format provider.</param>
    internal MessageTemplateOutputTokenRenderer(RichTextTheme theme, PropertyToken token, IFormatProvider? formatProvider)
    {
        var isLiteral = false;
        var isJson = false;

        if (token is null)
        {
            throw new ArgumentNullException(nameof(token));
        }

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
                    default:
                        // do nothing
                        break;
                }
            }
        }

        ThemedValueFormatter valueFormatter = isJson ? new ThemedJsonValueFormatter(theme, formatProvider) : new ThemedDisplayValueFormatter(theme, formatProvider);

        this.renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
    }

    /// <inheritdoc />
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        if (logEvent is null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        this.renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
    }
}

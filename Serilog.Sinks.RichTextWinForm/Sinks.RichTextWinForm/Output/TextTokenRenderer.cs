// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The text token renderer.</summary>
internal sealed class TextTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The text.</summary>
    private readonly string text;

    /// <summary>The theme.</summary>
    private readonly RichTextTheme theme;

    /// <summary>Initialises a new instance of the <see cref="TextTokenRenderer"/> class.</summary>
    /// <param name="theme">The theme.</param>
    /// <param name="text">The text.</param>
    internal TextTokenRenderer(RichTextTheme theme, string text)
    {
        this.theme = theme;
        this.text = text;
    }

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        using (this.theme.Apply(output, RichTextThemeStyle.TertiaryText))
        {
            output.AppendText(this.text);
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExceptionTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The exception token renderer.</summary>
internal sealed class ExceptionTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The stack frame line prefix.</summary>
    private const string StackFrameLinePrefix = "   ";

    /// <summary>The theme.</summary>
    private readonly RichTextTheme theme;

    /// <summary>Initialises a new instance of the <see cref="ExceptionTokenRenderer"/> class.</summary>
    /// <param name="theme">The theme.</param>
    internal ExceptionTokenRenderer(RichTextTheme theme) => this.theme = theme;

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        // Padding is never applied by this renderer.
        if (logEvent.Exception is null)
        {
            return;
        }

        StringReader lines = new (logEvent.Exception.ToString());
        string? nextLine;
        while ((nextLine = lines.ReadLine()) != null)
        {
            var style = nextLine.StartsWith(StackFrameLinePrefix, StringComparison.CurrentCulture) ? RichTextThemeStyle.SecondaryText : RichTextThemeStyle.Text;
            using (this.theme.Apply(output, style))
            {
                output.AppendText(nextLine);
            }
        }
    }
}

// -----------------------------------------------------------------------
// <copyright file="TextTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using System.Windows.Forms;

using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Themes;

internal sealed class TextTokenRenderer : OutputTemplateTokenRenderer
{
    private readonly string text;

    private readonly RichTextTheme theme;

    internal TextTokenRenderer(RichTextTheme theme, string text)
    {
        this.theme = theme;
        this.text = text;
    }

    internal override void Render(LogEvent logEvent, RichTextBox output)
    {
        using (this.theme.Apply(output, RichTextThemeStyle.TertiaryText))
        {
            output.AppendText(this.text);
        }
    }
}

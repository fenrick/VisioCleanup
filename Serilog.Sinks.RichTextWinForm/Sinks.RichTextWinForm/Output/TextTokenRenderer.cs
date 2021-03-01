// -----------------------------------------------------------------------
// <copyright file="TextTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System.Windows.Forms;

    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class TextTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly string text;

        private readonly RichTextTheme theme;

        public TextTokenRenderer(RichTextTheme theme, string text)
        {
            this.theme = theme;
            this.text = text;
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            using (this.theme.Apply(output, RichTextThemeStyle.TertiaryText))
            {
                output.AppendText(this.text);
            }
        }
    }
}
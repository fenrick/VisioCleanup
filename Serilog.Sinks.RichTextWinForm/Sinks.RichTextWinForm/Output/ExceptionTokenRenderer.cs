// -----------------------------------------------------------------------
// <copyright file="ExceptionTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Sinks.RichTextWinForm.Themes;

    internal class ExceptionTokenRenderer : OutputTemplateTokenRenderer
    {
        private const string StackFrameLinePrefix = "   ";

        private readonly RichTextTheme theme;

        public ExceptionTokenRenderer(RichTextTheme theme) => this.theme = theme;

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            // Padding is never applied by this renderer.
            if (logEvent.Exception is null)
            {
                return;
            }

            StringReader lines = new(logEvent.Exception.ToString());
            string? nextLine;
            while ((nextLine = lines.ReadLine()) != null)
            {
                var style = nextLine.StartsWith(StackFrameLinePrefix) ? RichTextThemeStyle.SecondaryText : RichTextThemeStyle.Text;
                using (this.theme.Apply(output, style))
                {
                    output.AppendText(nextLine);
                }
            }
        }
    }
}
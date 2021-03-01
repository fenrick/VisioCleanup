// -----------------------------------------------------------------------
// <copyright file="NewLineTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Padding = Serilog.Sinks.RichTextWinForm.Rendering.Padding;

    internal class NewLineTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly Alignment? alignment;

        public NewLineTokenRenderer(Alignment? alignment)
        {
            this.alignment = alignment;
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            if (this.alignment.HasValue)
            {
                Padding.Apply(output, Environment.NewLine, this.alignment.Value.Widen(Environment.NewLine.Length));
            }
            else
            {
                using StringWriter buffer = new();
                buffer.WriteLine();
                output.AppendText(buffer.ToString());
            }
        }
    }
}
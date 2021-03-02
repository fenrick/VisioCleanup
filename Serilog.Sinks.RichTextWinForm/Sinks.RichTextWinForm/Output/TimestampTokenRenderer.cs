// -----------------------------------------------------------------------
// <copyright file="TimestampTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System;
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Themes;

    using Padding = Serilog.Sinks.RichTextWinForm.Rendering.Padding;

    internal class TimestampTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly IFormatProvider? formatProvider;

        private readonly RichTextTheme theme;

        private readonly PropertyToken token;

        public TimestampTokenRenderer(RichTextTheme theme, PropertyToken token, IFormatProvider? formatProvider)
        {
            this.theme = theme;
            this.token = token;
            this.formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            // We need access to ScalarValue.Render() to avoid this alloc; just ensures
            // that custom format providers are supported properly.
            ScalarValue sv = new(logEvent.Timestamp);

            using (this.theme.Apply(output, RichTextThemeStyle.SecondaryText))
            {
                if (this.token.Alignment is null)
                {
                    using StringWriter buffer = new();
                    sv.Render(buffer, this.token.Format, this.formatProvider);
                    output.AppendText(buffer.ToString());
                }
                else
                {
                    using StringWriter buffer = new();
                    sv.Render(buffer, this.token.Format, this.formatProvider);
                    var str = buffer.ToString();
                    Padding.Apply(output, str, this.token.Alignment);
                }
            }
        }
    }
}
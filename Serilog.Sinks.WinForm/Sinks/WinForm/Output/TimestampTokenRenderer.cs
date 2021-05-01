// -----------------------------------------------------------------------
// <copyright file="TimestampTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System;
    using System.IO;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.WinForm.Rendering;

    internal class TimestampTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly IFormatProvider? formatProvider;

        private readonly PropertyToken token;

        public TimestampTokenRenderer(PropertyToken token, IFormatProvider? formatProvider)
        {
            this.token = token;
            this.formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            // We need access to ScalarValue.Render() to avoid this alloc; just ensures
            // that custom format providers are supported properly.
            ScalarValue sv = new(logEvent.Timestamp);
            {
                if (this.token.Alignment is null)
                {
                    using StringWriter buffer = new();
                    sv.Render(buffer, this.token.Format, this.formatProvider);
                    output.Write(buffer.ToString());
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

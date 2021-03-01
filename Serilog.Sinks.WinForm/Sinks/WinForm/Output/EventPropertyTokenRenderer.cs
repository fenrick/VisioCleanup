// -----------------------------------------------------------------------
// <copyright file="EventPropertyTokenRenderer.cs" company="Jolyon Suthers">
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

    internal class EventPropertyTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly IFormatProvider? formatProvider;

        private readonly PropertyToken token;

        public EventPropertyTokenRenderer(PropertyToken token, IFormatProvider? formatProvider)
        {
            this.token = token;
            this.formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            // If a property is missing, don't render anything (message templates render the raw token here).
            if (!logEvent.Properties.TryGetValue(this.token.PropertyName, out var propertyValue))
            {
                Padding.Apply(output, string.Empty, this.token.Alignment);
                return;
            }

            using StringWriter writer = new();

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
                output.Write(writer.ToString());
            }
        }
    }
}
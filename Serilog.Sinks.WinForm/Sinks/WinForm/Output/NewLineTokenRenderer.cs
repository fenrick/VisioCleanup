// -----------------------------------------------------------------------
// <copyright file="NewLineTokenRenderer.cs" company="Jolyon Suthers">
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

    internal class NewLineTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly Alignment? alignment;

        public NewLineTokenRenderer(Alignment? alignment)
        {
            this.alignment = alignment;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            if (this.alignment.HasValue)
            {
                Padding.Apply(output, Environment.NewLine, this.alignment.Value.Widen(Environment.NewLine.Length));
            }
            else
            {
                using StringWriter buffer = new();
                buffer.WriteLine();
                output.Write(buffer.ToString());
            }
        }
    }
}
// -----------------------------------------------------------------------
// <copyright file="TextTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System.IO;

    using Serilog.Events;

    internal class TextTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly string text;

        public TextTokenRenderer(string text) => this.text = text;

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            {
                output.Write(this.text);
            }
        }
    }
}
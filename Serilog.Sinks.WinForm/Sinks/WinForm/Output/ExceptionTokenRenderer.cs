// -----------------------------------------------------------------------
// <copyright file="ExceptionTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System.IO;

    using Serilog.Events;

    internal class ExceptionTokenRenderer : OutputTemplateTokenRenderer
    {
        private const string StackFrameLinePrefix = "   ";

        public override void Render(LogEvent logEvent, TextWriter output)
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
                {
                    output.Write(nextLine);
                }
            }
        }
    }
}
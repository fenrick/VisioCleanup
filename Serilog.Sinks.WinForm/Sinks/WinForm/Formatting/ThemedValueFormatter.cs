// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatter.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Formatting
{
    using System.IO;

    using Serilog.Data;
    using Serilog.Events;

    internal abstract class ThemedValueFormatter : LogEventPropertyValueVisitor<ThemedValueFormatterState, int>
    {
        public void Format(LogEventPropertyValue value, TextWriter output, string format, bool literalTopLevel = false)
        {
            this.Visit(new ThemedValueFormatterState { Output = output, Format = format, IsTopLevel = literalTopLevel }, value);
        }
    }
}

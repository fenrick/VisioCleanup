﻿// -----------------------------------------------------------------------
// <copyright file="OutputTemplateTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System.IO;

    using Serilog.Events;

    internal abstract class OutputTemplateTokenRenderer
    {
        public abstract void Render(LogEvent logEvent, TextWriter output);
    }
}
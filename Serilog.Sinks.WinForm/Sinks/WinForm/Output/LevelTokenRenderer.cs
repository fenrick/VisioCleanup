// -----------------------------------------------------------------------
// <copyright file="LevelTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm.Output
{
    using System.IO;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.WinForm.Rendering;

    internal class LevelTokenRenderer : OutputTemplateTokenRenderer
    {
        private readonly PropertyToken levelToken;

        public LevelTokenRenderer(PropertyToken levelToken)
        {
            this.levelToken = levelToken;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, this.levelToken.Format);
            {
                Padding.Apply(output, moniker, this.levelToken.Alignment);
            }
        }
    }
}
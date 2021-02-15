// -----------------------------------------------------------------------
// <copyright file="LevelTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Serilog.Events;
    using Serilog.Parsing;
    using Serilog.Sinks.RichTextWinForm.Themes;

    using Padding = Serilog.Sinks.RichTextWinForm.Rendering.Padding;

    internal class LevelTokenRenderer : OutputTemplateTokenRenderer
    {
        private static readonly Dictionary<LogEventLevel, RichTextThemeStyle> levels = new()
                                                                                           {
                                                                                               { LogEventLevel.Verbose, RichTextThemeStyle.LevelVerbose },
                                                                                               { LogEventLevel.Debug, RichTextThemeStyle.LevelDebug },
                                                                                               { LogEventLevel.Information, RichTextThemeStyle.LevelInformation },
                                                                                               { LogEventLevel.Warning, RichTextThemeStyle.LevelWarning },
                                                                                               { LogEventLevel.Error, RichTextThemeStyle.LevelError },
                                                                                               { LogEventLevel.Fatal, RichTextThemeStyle.LevelFatal },
                                                                                           };

        private readonly PropertyToken levelToken;

        private readonly RichTextTheme theme;

        public LevelTokenRenderer(RichTextTheme theme, PropertyToken levelToken)
        {
            this.theme = theme;
            this.levelToken = levelToken;
        }

        protected LevelTokenRenderer()
        {
        }

        public override void Render(LogEvent logEvent, RichTextBox output)
        {
            var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, this.levelToken.Format);
            if (!levels.TryGetValue(logEvent.Level, out var levelStyle))
            {
                levelStyle = RichTextThemeStyle.Invalid;
            }

            var characterCount = 0;
            using (this.theme.Apply(output, levelStyle, ref characterCount))
            {
                Padding.Apply(output, moniker, this.levelToken.Alignment);
            }
        }
    }
}
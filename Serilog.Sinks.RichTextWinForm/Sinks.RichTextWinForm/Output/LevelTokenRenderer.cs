// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LevelTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Rendering;
using Serilog.Sinks.RichTextWinForm.Themes;

/// <summary>The level token renderer.</summary>
internal sealed class LevelTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The levels.</summary>
    private static readonly Dictionary<LogEventLevel, RichTextThemeStyle> Levels = new ()
    {
        { LogEventLevel.Verbose, RichTextThemeStyle.LevelVerbose },
        { LogEventLevel.Debug, RichTextThemeStyle.LevelDebug },
        { LogEventLevel.Information, RichTextThemeStyle.LevelInformation },
        { LogEventLevel.Warning, RichTextThemeStyle.LevelWarning },
        { LogEventLevel.Error, RichTextThemeStyle.LevelError },
        { LogEventLevel.Fatal, RichTextThemeStyle.LevelFatal },
    };

    /// <summary>The level token.</summary>
    private readonly PropertyToken levelToken;

    /// <summary>The theme.</summary>
    private readonly RichTextTheme theme;

    /// <summary>Initialises a new instance of the <see cref="LevelTokenRenderer"/> class.</summary>
    /// <param name="theme">The theme.</param>
    /// <param name="levelToken">The level token.</param>
    internal LevelTokenRenderer(RichTextTheme theme, PropertyToken levelToken)
    {
        this.theme = theme;
        this.levelToken = levelToken;
    }

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, this.levelToken.Format);
        if (!Levels.TryGetValue(logEvent.Level, out var levelStyle))
        {
            levelStyle = RichTextThemeStyle.Invalid;
        }

        using (this.theme.Apply(output, levelStyle))
        {
            Padding.Apply(output, moniker, this.levelToken.Alignment);
        }
    }
}

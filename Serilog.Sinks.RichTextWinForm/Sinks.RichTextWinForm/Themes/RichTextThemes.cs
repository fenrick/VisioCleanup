// -----------------------------------------------------------------------
// <copyright file="RichTextThemes.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes
{
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>Themes.</summary>
    public static class RichTextThemes
    {
        /// <summary>Gets default theme.</summary>
        /// <value><see cref="Serilog.Sinks.RichTextWinForm.Themes.RichTextThemes.Default" /> theme.</value>
        public static RichTextTheme Default { get; } = new(new Dictionary<RichTextThemeStyle, ThemeColours>
                                                               {
                                                                   [RichTextThemeStyle.Text] = new() { Foreground = Color.White },
                                                                   [RichTextThemeStyle.SecondaryText] = new() { Foreground = Color.Gray },
                                                                   [RichTextThemeStyle.TertiaryText] = new() { Foreground = Color.DarkGray },
                                                                   [RichTextThemeStyle.Invalid] = new() { Foreground = Color.Yellow },
                                                                   [RichTextThemeStyle.Null] = new() { Foreground = Color.Blue },
                                                                   [RichTextThemeStyle.Name] = new() { Foreground = Color.Gray },
                                                                   [RichTextThemeStyle.String] = new() { Foreground = Color.Cyan },
                                                                   [RichTextThemeStyle.Number] = new() { Foreground = Color.Magenta },
                                                                   [RichTextThemeStyle.Boolean] = new() { Foreground = Color.Blue },
                                                                   [RichTextThemeStyle.Scalar] = new() { Foreground = Color.Green },
                                                                   [RichTextThemeStyle.LevelVerbose] = new() { Foreground = Color.Gray },
                                                                   [RichTextThemeStyle.LevelDebug] = new() { Foreground = Color.Gray },
                                                                   [RichTextThemeStyle.LevelInformation] = new() { Foreground = Color.White },
                                                                   [RichTextThemeStyle.LevelWarning] = new() { Foreground = Color.Yellow },
                                                                   [RichTextThemeStyle.LevelError] = new() { Foreground = Color.White, Background = Color.Red },
                                                                   [RichTextThemeStyle.LevelFatal] = new() { Foreground = Color.White, Background = Color.Red },
                                                               });
    }
}

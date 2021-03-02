// -----------------------------------------------------------------------
// <copyright file="RichTextTheme.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>The class for styled rich text output.</summary>
    public class RichTextTheme
    {
        /// <summary>Gets collection of valid styles.</summary>
        /// <value>Collection of valid styles.</value>
        private readonly IReadOnlyDictionary<RichTextThemeStyle, ThemeColours> styles;

        /// <summary>Initialises a new instance of the <see cref="RichTextTheme" /> class.</summary>
        /// <param name="styles">Styles to apply within the theme.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="styles" /> is null.</exception>
        public RichTextTheme(IReadOnlyDictionary<RichTextThemeStyle, ThemeColours> styles)
        {
            if (styles is null)
            {
                throw new ArgumentNullException(nameof(styles));
            }

            this.styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary><see cref="Reset" /> the <paramref name="output" /> to un-styled colors.</summary>
        /// <param name="output">Output destination.</param>
        public static void Reset(RichTextBox output)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.SelectionColor = output.ForeColor;
            output.SelectionBackColor = output.BackColor;
        }

        internal StyleReset Apply(RichTextBox output, RichTextThemeStyle style)
        {
            this.Set(output, style);

            return new StyleReset(output);
        }

        /// <summary>Begin a span of text in the specified <paramref name="style" /> .</summary>
        /// <param name="output">Output destination.</param>
        /// <param name="style">Style to apply.</param>
        private void Set(RichTextBox output, RichTextThemeStyle style)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (!this.styles.TryGetValue(style, out var wcts))
            {
                return;
            }

            if (wcts.Foreground.HasValue)
            {
                output.SelectionColor = wcts.Foreground.Value;
            }

            if (wcts.Background.HasValue)
            {
                output.SelectionBackColor = wcts.Background.Value;
            }
        }
    }
}
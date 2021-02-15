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
        /// <summary>Initialises a new instance of the <see cref="RichTextTheme" /> class.</summary>
        /// <param name="styles"><see cref="Styles" /> to apply within the theme.</param>
        /// <exception cref="System.ArgumentNullException">When <paramref name="styles" /> is null.</exception>
        public RichTextTheme(IReadOnlyDictionary<RichTextThemeStyle, ThemeColours> styles)
        {
            if (styles is null)
            {
                throw new ArgumentNullException(nameof(styles));
            }

            this.Styles = styles.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>Gets a theme based on the Serilog "literate" sink.</summary>
        /// <value>A theme based on the Serilog "literate" sink.</value>
        public static RichTextTheme Default { get; } = RichTextThemes.Default;

        /// <summary>Gets no styling applied.</summary>
        /// <value>No styling applied.</value>
        public static RichTextTheme None { get; } = new EmptyRichTextTheme();

        /// <summary>Gets collection of valid styles.</summary>
        /// <value>Collection of valid styles.</value>
        public IReadOnlyDictionary<RichTextThemeStyle, ThemeColours> Styles { get; }

        /// <summary>Gets the number of characters written by the <see cref="RichTextTheme.Reset" /> method.</summary>
        /// <value>The number of characters written by the <see cref="RichTextTheme.Reset" /> method.</value>
        protected int ResetCharCount { get; }

        /// <summary><see cref="Reset" /> the <paramref name="output" /> to un-styled colors.</summary>
        /// <param name="output">Output destination.</param>
        public void Reset(RichTextBox output)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            output.SelectionColor = output.ForeColor;
            output.SelectionBackColor = output.BackColor;
        }

        /// <summary>Begin a span of text in the specified <paramref name="style" /> .</summary>
        /// <param name="output">Output destination.</param>
        /// <param name="style">Style to apply.</param>
        /// <returns>The number of characters written to <paramref name="output" /> .</returns>
        public int Set(RichTextBox output, RichTextThemeStyle style)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (this.Styles.TryGetValue(style, out var wcts))
            {
                if (wcts.Foreground.HasValue)
                {
                    output.SelectionColor = wcts.Foreground.Value;
                }

                if (wcts.Background.HasValue)
                {
                    output.SelectionBackColor = wcts.Background.Value;
                }
            }

            return 0;
        }

        internal StyleReset Apply(RichTextBox output, RichTextThemeStyle style, ref int invisibleCharacterCount)
        {
            invisibleCharacterCount += this.Set(output, style);
            invisibleCharacterCount += this.ResetCharCount;

            return new StyleReset(this, output);
        }
    }
}
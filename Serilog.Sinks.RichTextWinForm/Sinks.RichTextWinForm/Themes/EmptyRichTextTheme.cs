// -----------------------------------------------------------------------
// <copyright file="EmptyRichTextTheme.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Windows.Forms;

    internal class EmptyRichTextTheme : RichTextTheme
    {
        public EmptyRichTextTheme(IReadOnlyDictionary<RichTextThemeStyle, ThemeColours> styles)
            : base(styles)
        {
        }

        public EmptyRichTextTheme()
            : base(ImmutableDictionary<RichTextThemeStyle, ThemeColours>.Empty)
        {
        }

        protected new int ResetCharCount { get; }

        public new void Reset(RichTextBox output)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }
        }

        public new int Set(RichTextBox output, RichTextThemeStyle style)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            return 0;
        }
    }
}
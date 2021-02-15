// -----------------------------------------------------------------------
// <copyright file="StyleReset.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes
{
    using System;
    using System.Windows.Forms;

    internal struct StyleReset : IDisposable
    {
        private readonly RichTextTheme theme;

        private readonly RichTextBox output;

        public StyleReset(RichTextTheme theme, RichTextBox output)
        {
            this.theme = theme;
            this.output = output;
        }

        public void Dispose()
        {
            this.theme.Reset(this.output);
        }
    }
}
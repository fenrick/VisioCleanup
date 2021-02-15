﻿// -----------------------------------------------------------------------
// <copyright file="ThemedValueFormatterState.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Formatting
{
    using System.Windows.Forms;

    internal struct ThemedValueFormatterState
    {
        public RichTextBox Output;

        public string Format;

        public bool IsTopLevel;

        public ThemedValueFormatterState Nest()
        {
            return new() { Output = this.Output };
        }
    }
}
// -----------------------------------------------------------------------
// <copyright file="StyleReset.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Themes;

using System;
using System.Windows.Forms;

public readonly struct StyleReset : IDisposable
{
    private readonly RichTextBox output;

    public StyleReset(RichTextBox output) => this.output = output;

    public void Dispose() => RichTextTheme.Reset(this.output);
}

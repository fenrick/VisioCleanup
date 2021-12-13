// -----------------------------------------------------------------------
// <copyright file="IOutputTemplateTokenRenderer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using System.Windows.Forms;

using Serilog.Events;

internal interface IOutputTemplateTokenRenderer
{
    public void Render(LogEvent logEvent, RichTextBox output);
}

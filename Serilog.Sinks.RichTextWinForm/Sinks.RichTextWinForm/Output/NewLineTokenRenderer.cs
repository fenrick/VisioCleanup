// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewLineTokenRenderer.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm.Output;

using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.RichTextWinForm.Rendering;

/// <summary>The new line token renderer.</summary>
internal sealed class NewLineTokenRenderer : IOutputTemplateTokenRenderer
{
    /// <summary>The alignment.</summary>
    private readonly Alignment? alignment;

    /// <summary>Initialises a new instance of the <see cref="NewLineTokenRenderer"/> class.</summary>
    /// <param name="alignment">The alignment.</param>
    internal NewLineTokenRenderer(Alignment? alignment) => this.alignment = alignment;

    /// <summary>The render.</summary>
    /// <param name="logEvent">The log event.</param>
    /// <param name="output">The output.</param>
    public void Render(LogEvent logEvent, RichTextBox output)
    {
        if (this.alignment.HasValue)
        {
            Padding.Apply(output, Environment.NewLine, this.alignment.Value.Widen(Environment.NewLine.Length));
        }
        else
        {
            using StringWriter buffer = new ();
            buffer.WriteLine();
            output.AppendText(buffer.ToString());
        }
    }
}

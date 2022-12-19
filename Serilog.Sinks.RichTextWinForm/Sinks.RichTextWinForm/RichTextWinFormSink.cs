// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RichTextWinFormSink.cs" company="Jolyon Suthers">
//   Copyright (c) Jolyon Suthers. All rights reserved.
//                       Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

[assembly: CLSCompliant(true)]

namespace Serilog.Sinks.RichTextWinForm;

using System.Collections.Concurrent;
using System.Collections.ObjectModel;

using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.RichTextWinForm.Output;

/// <summary>Sink log events to the monitoring rich text form element.</summary>
public class RichTextWinFormSink : ILogEventSink
{
    /// <summary>Gets a collection of rich text fields.</summary>
    /// <value>A collection of rich text fields.</value>
    private static readonly Collection<RichTextBox> RichTextFields = new ();

    /// <summary>The formatter.</summary>
    private readonly OutputTemplateRenderer formatter;

    /// <summary>The unprocessed log events.</summary>
    private readonly ConcurrentQueue<LogEvent> unprocessedLogEvents = new ();

    /// <summary>Initialises a new instance of the <see cref="RichTextWinFormSink"/> class. Initialises a new instance of the<see cref="RichTextWinFormSink"/> class.</summary>
    /// <param name="formatter">Formatter.</param>
    internal RichTextWinFormSink(OutputTemplateRenderer formatter) => this.formatter = formatter;

    /// <summary>Add a new rich text box to the sink.</summary>
    /// <param name="richTextBox">RichTextBox to add.</param>
    public static void AddRichTextBox(RichTextBox richTextBox) => RichTextFields.Add(richTextBox);

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        this.unprocessedLogEvents.Enqueue(logEvent);
        this.FlushQueue();
    }

    /// <summary>The flush queue.</summary>
    private void FlushQueue()
    {
        if (RichTextFields.Any(textField => textField.IsDisposed))
        {
            return;
        }

        while (this.unprocessedLogEvents.TryDequeue(out var unprocessedLogEvent))
        {
            foreach (var richTextField in RichTextFields)
            {
                if (richTextField.InvokeRequired)
                {
                    var @event = unprocessedLogEvent;
                    richTextField.Invoke((MethodInvoker)(() => this.formatter.Format(@event, richTextField)));
                    continue;
                }

                this.formatter.Format(unprocessedLogEvent, richTextField);
            }
        }

        Application.DoEvents();
    }
}

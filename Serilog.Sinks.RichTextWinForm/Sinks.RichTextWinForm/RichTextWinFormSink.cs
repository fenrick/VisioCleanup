﻿// -----------------------------------------------------------------------
// <copyright file="RichTextWinFormSink.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.RichTextWinForm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Sinks.RichTextWinForm.Output;

    /// <summary>Sink log events to the monitoring rich text form element.</summary>
    public class RichTextWinFormSink : ILogEventSink
    {
        /// <summary>Gets a collection of rich text fields.</summary>
        /// <value>A collection of rich text fields.</value>
        private static readonly Collection<RichTextBox> RichTextField = new();

        private static readonly Collection<RichTextWinFormSink> Sinks = new();

        private readonly OutputTemplateRenderer formatter;

        private readonly Queue<LogEvent> unprocessedLogEvents = new();

        /// <summary>Initialises a new instance of the <see cref="RichTextWinFormSink" /> class.</summary>
        /// <param name="formatter">Formatter.</param>
        public RichTextWinFormSink(OutputTemplateRenderer formatter)
        {
            this.formatter = formatter;
            Sinks.Add(this);
        }

        /// <summary>Add a new rich text box to the sink.</summary>
        /// <param name="richTextBox">RichTextBox to add.</param>
        public static void AddRichTextBox(RichTextBox richTextBox)
        {
            RichTextField.Add(richTextBox);

            foreach (var sink in Sinks)
            {
                sink.FlushQueue();
            }
        }

        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            this.unprocessedLogEvents.Enqueue(logEvent);
            this.FlushQueue();
        }

        private void FlushQueue()
        {
            if (RichTextField.Count <= 0)
            {
                return;
            }

            while (this.unprocessedLogEvents.TryDequeue(out var unprocessedLogEvent))
            {
                foreach (var richTextBox in RichTextField)
                {
                    if (richTextBox.IsDisposed)
                    {
                        continue;
                    }

                    if (richTextBox.InvokeRequired)
                    {
                        var @event = unprocessedLogEvent;
                        richTextBox.Invoke((MethodInvoker)(() => this.formatter.Format(@event, richTextBox)));
                        continue;
                    }

                    this.formatter.Format(unprocessedLogEvent, richTextBox);
                }
            }

            Application.DoEvents();
        }
    }
}
// -----------------------------------------------------------------------
// <copyright file="RichTextWinFormSink.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Forms;

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Sinks.RichTextWinForm.Output;
    using Serilog.Sinks.RichTextWinForm.Themes;

    /// <summary>Sink log events to the monitoring rich text form element.</summary>
    public class RichTextWinFormSink : ILogEventSink
    {
        private readonly OutputTemplateRenderer formatter;

        private readonly RichTextTheme richTextTheme;

        private readonly Queue<LogEvent> unprocessedLogEvents = new();

        /// <summary>Initialises a new instance of the <see cref="RichTextWinFormSink" /> class.</summary>
        /// <param name="richTextTheme">Theme for formatting rich text.</param>
        /// <param name="formatter">Formatter.</param>
        public RichTextWinFormSink(RichTextTheme richTextTheme, OutputTemplateRenderer formatter)
        {
            this.richTextTheme = richTextTheme ?? throw new ArgumentNullException(nameof(richTextTheme));
            this.formatter = formatter;
            Sinks.Add(this);
        }

        private static Collection<RichTextWinFormSink> Sinks = new();

        /// <summary>Gets a collection of rich text fields.</summary>
        /// <value>A collection of rich text fields.</value>
        private static Collection<RichTextBox> RichTextField { get; } = new();

        public static void AddRichTextBox(RichTextBox richTextBox)
        {
            RichTextField.Add(richTextBox);

            foreach (RichTextWinFormSink sink in Sinks)
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
            if (RichTextField.Count > 0)
            {
                while(this.unprocessedLogEvents.TryDequeue(out var unprocessedLogEvent))
                {
                    foreach (var richTextBox in RichTextField)
                    {
                        if (!richTextBox.IsDisposed)
                        {
                            if (richTextBox.InvokeRequired)
                            {
                                richTextBox.Invoke((MethodInvoker)(() => this.formatter.Format(unprocessedLogEvent, richTextBox)));
                            }
                            else
                            {
                                this.formatter.Format(unprocessedLogEvent, richTextBox);
                            }
                        }
                    }
                }

                Application.DoEvents();
            }
        }
    }
}
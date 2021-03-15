// -----------------------------------------------------------------------
// <copyright file="WinFormSink.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace Serilog.Sinks.WinForm
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows.Forms;

    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Sinks.WinForm.Output;

    /// <summary>Sink log events to the monitoring rich text form element.</summary>
    public class WinFormSink : ILogEventSink
    {
        private static readonly Collection<ListBox> ListBoxes = new();

        private static readonly Collection<WinFormSink> Sinks = new();

        /// <summary>Gets a collection of rich text fields.</summary>
        /// <value>A collection of rich text fields.</value>
        private static readonly Collection<TextBox> TextBoxes = new();

        private readonly OutputTemplateRenderer formatter;

        private readonly FixedQueue<LogEvent> unprocessedLogEvents = new(150);

        /// <summary>Initialises a new instance of the <see cref="WinFormSink" /> class.</summary>
        /// <param name="formatter">Formatter.</param>
        public WinFormSink(OutputTemplateRenderer formatter)
        {
            this.formatter = formatter;
            Sinks.Add(this);
        }

        /// <summary>Add list box to Sinks as output destination. Will flush any outstanding event queue.</summary>
        /// <param name="listBox">List box to output to.</param>
        public static void AddListView(ListBox listBox)
        {
            ListBoxes.Add(listBox);

            foreach (var sinks in Sinks)
            {
                sinks.FlushQueue();
            }
        }

        /// <summary>Add a new rich text box to the sink.</summary>
        /// <param name="textBox">TextBox to add.</param>
        public static void AddTextBox(TextBox textBox)
        {
            TextBoxes.Add(textBox);

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
            if ((TextBoxes.Count + ListBoxes.Count) == 0)
            {
                return;
            }

            while (this.unprocessedLogEvents.TryDequeue(out var unprocessedLogEvent))
            {
                if (unprocessedLogEvent is null)
                {
                    continue;
                }

                StringWriter buffer = new();
                this.formatter.Format(unprocessedLogEvent, buffer);

                // textBoxes
                foreach (var textBox in TextBoxes)
                {
                    if (textBox.IsDisposed)
                    {
                        continue;
                    }

                    if (textBox.InvokeRequired)
                    {
                        textBox.Invoke((MethodInvoker)(() => { textBox.AppendText(buffer.ToString()); }));
                        continue;
                    }

                    textBox.AppendText(buffer.ToString());
                }

                // listViews
                foreach (var listBox in ListBoxes)
                {
                    if (listBox.IsDisposed)
                    {
                        continue;
                    }

                    if (listBox.InvokeRequired)
                    {
                        listBox.Invoke((MethodInvoker)(() => { listBox.Items.Add(buffer.ToString()); }));
                        continue;
                    }

                    listBox.Items.Add(buffer.ToString());
                }
            }

            Application.DoEvents();
        }
    }
}
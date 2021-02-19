// -----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms
{
    using System;
    using System.Windows.Forms;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Serilog.Sinks.RichTextWinForm;

    using VisioCleanup.Core.Models.Config;

    /// <summary>Main application form.</summary>
    public partial class MainForm : Form
    {
        /// <summary>The app config.</summary>
        private readonly AppConfig appConfig;

        /// <summary>The logger.</summary>
        private readonly ILogger<MainForm> logger;

        /// <summary>Initialises a new instance of the <see cref="MainForm" /> class.</summary>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The app config.</param>
        public MainForm(ILogger<MainForm> logger, IOptions<AppConfig> options)
        {
            if (options == null)
            {
                throw new InvalidOperationException("AppConfig can not be null.");
            }

            this.appConfig = options.Value;
            this.logger = logger;
            this.InitializeComponent();
            RichTextWinFormSink.AddRichTextBox(this.logTextBox);
        }

        /// <summary>TODO The button 1_ click.</summary>
        /// <param name="sender">TODO The sender.</param>
        /// <param name="e">TODO The e.</param>
        private void ProcessExcelDataSet_Click(object sender, EventArgs e)
        {
            this.logger.LogDebug("woot!");
        }
    }
}
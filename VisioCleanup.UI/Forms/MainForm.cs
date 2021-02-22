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
        /// <param name="logger">The <paramref name="logger"/>.</param>
        /// <param name="options">The app config.</param>
        public MainForm(ILogger<MainForm> logger, IOptions<AppConfig> options)
        {
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.logger.LogDebug("Initialising form components.");
            this.InitializeComponent();

            this.logger.LogDebug("Setting log output.");
            RichTextWinFormSink.AddRichTextBox(this.logTextBox);

            this.logger.LogDebug("Binding appConfig to data grid.");
            this.parametersBindingSource.Add(this.appConfig);
            this.parametersDataGridView.AutoGenerateColumns = true;
            this.parametersDataGridView.DataSource = this.parametersBindingSource;
        }

        /// <summary>Activate the processing of Excel data set.</summary>
        /// <param name="sender">The <paramref name="sender"/>.</param>
        /// <param name="eventArgs">The <paramref name="eventArgs"/>.</param>
        private void ProcessExcelDataSet_Click(object sender, EventArgs eventArgs)
        {
            this.logger.LogDebug("woot!");
        }
    }
}
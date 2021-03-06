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

    using Serilog.Sinks.WinForm;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models.Config;

    /// <summary>Main application form.</summary>
    public partial class MainForm : Form
    {
        private readonly IExcelService excelService;

        /// <summary>The logger.</summary>
        private readonly ILogger<MainForm> logger;

        private readonly IVisioService visioService;

        private IProcessingService? processingService;

        /// <summary>Initialises a new instance of the <see cref="MainForm" /> class.</summary>
        /// <param name="logger">The <paramref name="logger" />.</param>
        /// <param name="options">The app config.</param>
        /// <param name="excelService">The excel service.</param>
        /// <param name="visioService">The visio service.</param>
        public MainForm(ILogger<MainForm> logger, IOptions<AppConfig> options, IExcelService excelService, IVisioService visioService)
        {
            var appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
            this.visioService = visioService ?? throw new ArgumentNullException(nameof(visioService));

            this.logger.LogDebug("Initialising form components.");
            this.InitializeComponent();

            this.logger.LogDebug("Setting log output.");
            WinFormSink.AddListView(this.listBox);

            this.logger.LogDebug("Binding appConfig to data grid.");
            this.parametersBindingSource.Add(appConfig);
            this.parametersDataGridView.AutoGenerateColumns = true;
            this.parametersDataGridView.DataSource = this.parametersBindingSource;

            this.logger.LogDebug("Preparing data set to data grid binding.");
            this.dataGridView1.DataSource = this.dataSetBindingSource;
            this.dataGridView1.AutoGenerateColumns = true;
        }

        private async void LayoutDataSet_Click(object sender, EventArgs eventArgs)
        {
            this.controlsFlowPanel.Enabled = false;

            if (this.processingService is null)
            {
                this.logger.LogDebug("Processing Service is not defined.");
                MessageBox.Show(
                    "Unable to layout dataset, none is loaded.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1,
                    MessageBoxOptions.ServiceNotification,
                    displayHelpButton: false);
                return;
            }

            this.logger.LogDebug("Laying out data set.");
            await this.processingService.LayoutDataSet();

            this.dataSetBindingSource.ResetBindings(true);

            this.controlsFlowPanel.Enabled = true;
        }

        /// <summary>Load Visio Object Model.</summary>
        /// <param name="sender">The <paramref name="sender" />.</param>
        /// <param name="eventArgs">The <paramref name="eventArgs" />.</param>
        private async void LoadVisioObjects_Click(object sender, EventArgs eventArgs)
        {
            this.controlsFlowPanel.Enabled = false;

            this.logger.LogDebug("Loading objects from visio.");
            await this.visioService.LoadVisioObjectModel();

            this.logger.LogDebug("Updating data set.");
            this.dataSetBindingSource.DataSource = this.visioService.AllShapes;
            this.processingService = this.visioService;

            this.controlsFlowPanel.Enabled = true;
        }

        /// <summary>Activate the processing of Excel data set.</summary>
        /// <param name="sender">The <paramref name="sender" />.</param>
        /// <param name="eventArgs">The <paramref name="eventArgs" />.</param>
        private async void ProcessExcelDataSet_Click(object sender, EventArgs eventArgs)
        {
            this.controlsFlowPanel.Enabled = false;

            this.logger.LogDebug("Loading objects from excel.");
            await this.excelService.ProcessDataSet();

            this.logger.LogDebug("Updating dataset.");
            this.dataSetBindingSource.DataSource = this.excelService.AllShapes;
            this.processingService = this.excelService;

            this.controlsFlowPanel.Enabled = true;
        }

        private async void updateVisioDrawing_Click(object sender, EventArgs e)
        {
            this.controlsFlowPanel.Enabled = false;

            this.logger.LogDebug("Drawing visio.");
            await this.processingService.UpdateVisio();

            this.controlsFlowPanel.Enabled = true;
        }
    }
}
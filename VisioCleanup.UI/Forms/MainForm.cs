// -----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Serilog.Sinks.RichTextWinForm;

    using VisioCleanup.Core.Contracts;
    using VisioCleanup.Core.Models.Config;

    public partial class MainForm : Form
    {
        private readonly AppConfig appConfig;

        /// <summary>The database service.</summary>
        private readonly IDatabaseService databaseService;

        /// <summary>The excel service.</summary>
        private readonly IExcelService excelService;

        /// <summary>The logger.</summary>
        private readonly ILogger<MainForm> logger;

        /// <summary>The visio service.</summary>
        private readonly IVisioService visioService;

        /// <summary>The processing service.</summary>
        private IProcessingService? processingService;

        /// <summary>Initialises a new instance of the <see cref="MainForm" /> class.</summary>
        /// <param name="logger">The <paramref name="logger" /> .</param>
        /// <param name="options">The app config.</param>
        /// <param name="excelService">The excel service.</param>
        /// <param name="visioService">The visio service.</param>
        /// <param name="databaseService">The database management service.</param>
        public MainForm(ILogger<MainForm> logger, IOptions<AppConfig> options, IExcelService excelService, IVisioService visioService, IDatabaseService databaseService)
        {
            this.appConfig = options.Value ?? throw new ArgumentNullException(nameof(options));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
            this.visioService = visioService ?? throw new ArgumentNullException(nameof(visioService));
            this.databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

            this.logger.LogDebug("Initialising form components");
            this.InitializeComponent();

            this.logger.LogDebug("Setting log output");
            RichTextWinFormSink.AddRichTextBox(this.richTextLogBox);

            this.logger.LogDebug("Binding appConfig to data grid");
            this.parametersBindingSource.Add(this.appConfig);
            this.parametersDataGridView.AutoGenerateColumns = true;
            this.parametersDataGridView.DataSource = this.parametersBindingSource;

            this.logger.LogDebug("Preparing data set to data grid binding");
            this.dataGridView1.DataSource = this.dataSetBindingSource;
            this.dataGridView1.AutoGenerateColumns = true;

            //this.controlSplitContainer.SplitterDistance = this.updateVisioDrawing.Width + this.controlsFlowPanel.Padding.Left + this.controlsFlowPanel.Padding.Right;

            if (this.appConfig.DatabaseQueries is not null)
            {
                foreach (var queryName in this.appConfig.DatabaseQueries.Select(databaseQuery => databaseQuery["Name"]))
                {
                    this.selectSQLStatementComboBox.Items.Add(queryName);
                }
            }

            this.selectSQLStatementComboBox.SelectedIndex = 0;

            this.SelectSqlStatementComboBoxSelectionChangeCommitted(null, null);
        }

        private bool CheckProcessingService()
        {
            if (this.processingService is null)
            {
                this.logger.LogDebug("Processing Service is not defined");
                this.Invoke(
                    (MethodInvoker)(() => MessageBox.Show(
                                           @"Unable to layout dataset, none is loaded.",
                                           @"Error",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Error,
                                           MessageBoxDefaultButton.Button1,
                                           MessageBoxOptions.ServiceNotification)));
                return true;
            }

            return false;
        }

        private void HandleException(Exception exception, string messageText)
        {
            this.logger.LogError(exception, exception.Message);
            this.Invoke(
                (MethodInvoker)(() =>
                                       {
                                           this.processingService = null;
                                           this.dataSetBindingSource.DataSource = null;
                                           this.controlsFlowPanel.Enabled = true;
                                           MessageBox.Show(
                                               messageText,
                                               @"Error",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error,
                                               MessageBoxDefaultButton.Button1,
                                               MessageBoxOptions.ServiceNotification);
                                       }));
        }

        /// <summary>The layout data set_ click.</summary>
        /// <param name="sender">The <paramref name="sender" /> .</param>
        /// <param name="eventArgs">The event args.</param>
        private async void LayoutDataSet_Click(object sender, EventArgs eventArgs)
        {
            if (this.CheckProcessingService())
            {
                return;
            }

            try
            {
                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.controlsFlowPanel.Enabled = false;
                                               this.dataSetBindingSource.DataSource = null;
                                           }));

                this.logger.LogDebug("Laying out data set");

                await this.processingService!.LayoutDataSet().ConfigureAwait(false);

                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.dataSetBindingSource.DataSource = this.processingService!.AllShapes;

                                               this.controlsFlowPanel.Enabled = true;
                                           }));
            }
            catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
            {
                this.HandleException(e, "@Processing error.");
            }
        }

        /// <summary>sThe load from iserver database.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The click event.</param>
        private async void LoadFromIServer_Click(object sender, EventArgs e)
        {
            try
            {
                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.controlsFlowPanel.Enabled = false;
                                               this.dataSetBindingSource.DataSource = null;
                                               this.processingService = null;
                                           }));

                this.logger.LogDebug("Loading objects from database");

                await this.databaseService.ProcessDataSet(this.sqlStatementTextBox.Text).ConfigureAwait(false);

                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.logger.LogDebug("Updating dataset");
                                               this.dataSetBindingSource.DataSource = this.databaseService.AllShapes;
                                               this.processingService = this.databaseService;

                                               this.controlsFlowPanel.Enabled = true;
                                           }));
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentNullException)
            {
                this.HandleException(exception, @"Database and visio need to be setup for this to work.");
            }
        }

        /// <summary>Load Visio Object Model.</summary>
        /// <param name="sender">The <paramref name="sender" /> .</param>
        /// <param name="eventArgs">The <paramref name="eventArgs" /> .</param>
        private async void LoadVisioObjects_Click(object sender, EventArgs eventArgs)
        {
            try
            {
                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.controlsFlowPanel.Enabled = false;
                                               this.dataSetBindingSource.DataSource = null;
                                               this.processingService = null;
                                           }));

                this.logger.LogDebug("Loading objects from visio");
                await this.visioService.LoadVisioObjectModel().ConfigureAwait(false);

                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.logger.LogDebug("Updating data set");
                                               this.dataSetBindingSource.DataSource = this.visioService.AllShapes;
                                               this.processingService = this.visioService;

                                               this.controlsFlowPanel.Enabled = true;
                                           }));
            }
            catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
            {
                this.HandleException(e, @"Visio need to be setup for this to work.");
            }
        }

        /// <summary>The main form_ dpi changed.</summary>
        /// <param name="sender">The <paramref name="sender" /> .</param>
        /// <param name="dpiChangedEventArgs">The dpi Changed Event Args.</param>
        private void MainForm_DpiChanged(object sender, DpiChangedEventArgs dpiChangedEventArgs)
        {
            //this.controlSplitContainer.SplitterDistance = this.updateVisioDrawing.Width + this.controlsFlowPanel.Padding.Left + this.controlsFlowPanel.Padding.Right;
        }

        /// <summary>Activate the processing of Excel data set.</summary>
        /// <param name="sender">The <paramref name="sender" /> .</param>
        /// <param name="eventArgs">The <paramref name="eventArgs" /> .</param>
        private async void ProcessExcelDataSet_Click(object sender, EventArgs eventArgs)
        {
            try
            {
                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.controlsFlowPanel.Enabled = false;
                                               this.dataSetBindingSource.DataSource = null;
                                               this.processingService = null;
                                           }));

                this.logger.LogDebug("Loading objects from excel");

                await this.excelService.ProcessDataSet().ConfigureAwait(false);

                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.logger.LogDebug("Updating dataset");
                                               this.dataSetBindingSource.DataSource = this.excelService.AllShapes;
                                               this.processingService = this.excelService;

                                               this.controlsFlowPanel.Enabled = true;
                                           }));
            }
            catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
            {
                this.HandleException(e, @"Excel and visio need to be setup for this to work.");
            }
        }

        private void SelectSqlStatementComboBoxSelectionChangeCommitted(object sender, EventArgs e)
        {
            if (this.appConfig.DatabaseQueries is null)
            {
                return;
            }

            foreach (var databaseQuery in this.appConfig.DatabaseQueries.Where(databaseQuery => databaseQuery["Name"] == this.selectSQLStatementComboBox.Text))
            {
                this.sqlStatementTextBox.Text = databaseQuery["Query"];

                this.sqlStatementTextBox.ReadOnly = this.sqlStatementTextBox.Text.Length > 0;
            }
        }

        /// <summary>sThe update visio drawing_ click.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The DPI change details.</param>
        private async void UpdateVisioDrawing_Click(object sender, EventArgs e)
        {
            if (this.CheckProcessingService())
            {
                return;
            }

            try
            {
                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.controlsFlowPanel.Enabled = false;
                                               this.dataSetBindingSource.DataSource = null;
                                           }));

                this.logger.LogDebug("Drawing visio");

                await this.processingService!.UpdateVisio().ConfigureAwait(false);

                this.Invoke(
                    (MethodInvoker)(() =>
                                           {
                                               this.dataSetBindingSource.DataSource = this.processingService!.AllShapes;

                                               this.controlsFlowPanel.Enabled = true;
                                           }));
            }
            catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentNullException)
            {
                this.HandleException(exception, @"Visio need to be setup for this to work.");
            }
        }
    }
}

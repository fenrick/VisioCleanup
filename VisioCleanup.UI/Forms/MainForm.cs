// -----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Serilog.Sinks.RichTextWinForm;

using VisioCleanup.Core.Contracts;
using VisioCleanup.Core.Models.Config;

/// <summary>Main form for system.</summary>
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
        this.appConfig = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        this.visioService = visioService ?? throw new ArgumentNullException(nameof(visioService));
        this.databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));

        this.logger.LogDebug("Initializing form components");
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

        if (this.appConfig.DatabaseQueries is not null)
        {
            foreach (var queryName in this.appConfig.DatabaseQueries.Select(databaseQuery => databaseQuery["Name"]))
            {
                this.selectSqlStatementComboBox.Items.Add(queryName);
            }
        }

        this.selectSqlStatementComboBox.SelectedIndex = 0;

        this.SelectSqlStatementComboBoxSelectionChangeCommitted(sender: null, e: null);

        this.logger.LogInformation("Application fully loaded.");
    }

    private bool CheckProcessingService()
    {
        if (this.processingService is not null)
        {
            return false;
        }

        this.logger.LogDebug("Processing Service is not defined");
        MessageBox.Show(
            @"Unable to layout dataset, none is loaded.",
            @"Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            MessageBoxDefaultButton.Button1,
            MessageBoxOptions.ServiceNotification);
        return true;
    }

    private void HandleException(Exception exception, string messageText)
    {
        this.logger.LogError(exception, "Exception");
        this.processingService = null;
        this.dataSetBindingSource.DataSource = null;
        this.controlsFlowPanel.Enabled = true;
        MessageBox.Show(messageText, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
    }

    /// <summary>The layout data set_ click.</summary>
    private async Task LayoutDataSet_ClickAsync()
    {
        if (this.CheckProcessingService())
        {
            return;
        }

        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;

            await Task.Run(
                () =>
                    {
                        this.logger.LogDebug("Laying out data set");
                        this.processingService!.LayoutDataSet();
                    });

            this.dataSetBindingSource.DataSource = this.processingService!.AllShapes;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
        {
            this.HandleException(e, "@Processing error.");
        }
    }

    private void DrawBitmapButton_Click(object sender, EventArgs e)
    {
        if (this.CheckProcessingService())
        {
            return;
        }

        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;
            this.logger.LogDebug("Generating bitmap");

            this.processingService!.DrawBitmapStructure();
            this.dataSetBindingSource.DataSource = this.processingService!.AllShapes;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentNullException)
        {
            this.HandleException(exception, "@Processing error.");
        }
    }

    /// <summary>sThe load from iserver database.</summary>
    private async Task LoadFromIServer_ClickAsync()
    {
        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;
            this.processingService = null;

            await Task.Run(
                () =>
                    {
                        this.logger.LogDebug("Loading objects from database");

                        this.databaseService.ProcessDataSet(this.sqlStatementTextBox.Text);
                    });

            this.logger.LogDebug("Updating dataset");
            this.dataSetBindingSource.DataSource = this.databaseService.AllShapes;
            this.processingService = this.databaseService;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentNullException)
        {
            this.HandleException(exception, @"Database and visio need to be setup for this to work.");
        }
    }

    /// <summary>Load Visio Object Model.</summary>
    private async Task LoadVisioObjects_ClickAsync()
    {
        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;
            this.processingService = null;

            await Task.Run(
                () =>
                    {
                        this.logger.LogDebug("Loading objects from visio");
                        this.visioService.LoadVisioObjectModel();
                    });

            this.logger.LogDebug("Updating data set");
            this.dataSetBindingSource.DataSource = this.visioService.AllShapes;
            this.processingService = this.visioService;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
        {
            this.HandleException(e, @"Visio need to be setup for this to work.");
        }
    }

    /// <summary>Activate the processing of Excel data set.</summary>
    private async Task ProcessExcelDataSet_ClickAsync()
    {
        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;
            this.processingService = null;

            await Task.Run(
                () =>
                    {
                        this.logger.LogDebug("Loading objects from excel");
                        this.excelService.ProcessDataSet();
                    });

            this.logger.LogDebug("Updating dataset");
            this.dataSetBindingSource.DataSource = this.excelService.AllShapes;
            this.processingService = this.excelService;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception e) when (e is InvalidOperationException || e is ArgumentNullException)
        {
            this.HandleException(e, @"Excel and visio need to be setup for this to work.");
        }
    }

    private void SelectSqlStatementComboBoxSelectionChangeCommitted(object? sender, EventArgs? e)
    {
        if (this.appConfig.DatabaseQueries is null)
        {
            return;
        }

        foreach (var databaseQuery in this.appConfig.DatabaseQueries.Where(
                     databaseQuery => string.Equals(databaseQuery["Name"], this.selectSqlStatementComboBox.Text, StringComparison.Ordinal)))
        {
            this.sqlStatementTextBox.Text = databaseQuery["Query"];

            this.sqlStatementTextBox.ReadOnly = this.sqlStatementTextBox.Text.Length > 0;
        }
    }

    /// <summary>sThe update visio drawing_ click.</summary>
    private async Task UpdateVisioDrawing_ClickAsync()
    {
        if (this.CheckProcessingService())
        {
            return;
        }

        try
        {
            this.controlsFlowPanel.Enabled = false;
            this.dataSetBindingSource.DataSource = null;

            await Task.Run(
                () =>
                    {
                        this.logger.LogDebug("Drawing visio");
                        this.processingService!.UpdateVisio();
                    });

            this.dataSetBindingSource.DataSource = this.processingService!.AllShapes;

            this.controlsFlowPanel.Enabled = true;
        }
        catch (Exception exception) when (exception is InvalidOperationException || exception is ArgumentNullException)
        {
            this.HandleException(exception, @"Visio need to be setup for this to work.");
        }
    }
}

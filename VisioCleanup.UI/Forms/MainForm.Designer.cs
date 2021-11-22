// -----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms;

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

/// <summary>Main application form.</summary>
public partial class MainForm
{
    /// <summary>Required designer variable.</summary>
    private IContainer components;

    private FlowLayoutPanel controlsFlowPanel;

    private DataGridView dataGridView1;

    private BindingSource dataSetBindingSource;

    private BindingSource parametersBindingSource;

    private DataGridView parametersDataGridView;

    private RichTextBox richTextLogBox;

    private ComboBox selectSQLStatementComboBox;

    private TextBox sqlStatementTextBox;

    /// <summary>Clean up any resources being used.</summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (this.components != null))
        {
            this.components.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
    private void InitializeComponent()
    {
        this.components = new Container();
        Button processExcelDataSet;
        Button loadVisioObjects;
        SplitContainer logSplitContainer;
        TableLayoutPanel controlsTableLayoutPanel;
        GroupBox groupBox;
        Button loadFromIServerButton;
        Button layoutDataSet;
        Button updateVisioDrawing;
        Button drawBitmapButton;
        TabControl tabControl1;
        TabPage dataSetTab;
        TabPage parametersTab;
        var dataGridViewCellStyle1 = new DataGridViewCellStyle();
        var dataGridViewCellStyle2 = new DataGridViewCellStyle();
        TabPage tabPage1;
        TableLayoutPanel sqlTableLayoutPanel;
        GroupBox logGroupBox;
        this.controlsFlowPanel = new FlowLayoutPanel();
        this.dataGridView1 = new DataGridView();
        this.parametersDataGridView = new DataGridView();
        this.selectSQLStatementComboBox = new ComboBox();
        this.sqlStatementTextBox = new TextBox();
        this.richTextLogBox = new RichTextBox();
        this.dataSetBindingSource = new BindingSource(this.components);
        this.parametersBindingSource = new BindingSource(this.components);
        processExcelDataSet = new Button();
        loadVisioObjects = new Button();
        logSplitContainer = new SplitContainer();
        controlsTableLayoutPanel = new TableLayoutPanel();
        groupBox = new GroupBox();
        loadFromIServerButton = new Button();
        layoutDataSet = new Button();
        updateVisioDrawing = new Button();
        drawBitmapButton = new Button();
        tabControl1 = new TabControl();
        dataSetTab = new TabPage();
        parametersTab = new TabPage();
        tabPage1 = new TabPage();
        sqlTableLayoutPanel = new TableLayoutPanel();
        logGroupBox = new GroupBox();
        ((ISupportInitialize)logSplitContainer).BeginInit();
        logSplitContainer.Panel1.SuspendLayout();
        logSplitContainer.Panel2.SuspendLayout();
        logSplitContainer.SuspendLayout();
        controlsTableLayoutPanel.SuspendLayout();
        groupBox.SuspendLayout();
        this.controlsFlowPanel.SuspendLayout();
        tabControl1.SuspendLayout();
        dataSetTab.SuspendLayout();
        ((ISupportInitialize)this.dataGridView1).BeginInit();
        parametersTab.SuspendLayout();
        ((ISupportInitialize)this.parametersDataGridView).BeginInit();
        tabPage1.SuspendLayout();
        sqlTableLayoutPanel.SuspendLayout();
        logGroupBox.SuspendLayout();
        ((ISupportInitialize)this.dataSetBindingSource).BeginInit();
        ((ISupportInitialize)this.parametersBindingSource).BeginInit();
        this.SuspendLayout();

        var buttonSize = new Size(130, 25);

        // processExcelDataSet
        processExcelDataSet.AutoSize = true;
        processExcelDataSet.Name = "processExcelDataSet";
        processExcelDataSet.Size = buttonSize;
        processExcelDataSet.TabIndex = 2;
        processExcelDataSet.Text = "Process Excel DataSet";
        processExcelDataSet.Click += this.ProcessExcelDataSet_Click;

        // loadVisioObjects
        loadVisioObjects.AutoSize = true;
        loadVisioObjects.Name = "loadVisioObjects";
        loadVisioObjects.Size = buttonSize;
        loadVisioObjects.TabIndex = 3;
        loadVisioObjects.Text = "Load Visio Objects";
        loadVisioObjects.Click += this.LoadVisioObjects_Click;

        // drawBitMapButton
        drawBitmapButton.AutoSize = true;
        drawBitmapButton.Name = "drawBitmapButton";
        drawBitmapButton.Size = buttonSize;
        drawBitmapButton.TabIndex = 6;
        drawBitmapButton.Text = "Draw Bitmap";
        drawBitmapButton.Click += this.DrawBitmapButton_Click;

        // logSplitContainer
        logSplitContainer.Cursor = Cursors.HSplit;
        logSplitContainer.Dock = DockStyle.Fill;
        logSplitContainer.Name = "logSplitContainer";
        logSplitContainer.Orientation = Orientation.Horizontal;

        // logSplitContainer.Panel1
        logSplitContainer.Panel1.Controls.Add(controlsTableLayoutPanel);

        // logSplitContainer.Panel2
        logSplitContainer.Panel2.Controls.Add(logGroupBox);
        logSplitContainer.TabIndex = 99;

        // controlsTableLayoutPanel
        controlsTableLayoutPanel.AutoSize = true;
        controlsTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        controlsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        controlsTableLayoutPanel.Controls.Add(groupBox, 0, 0);
        controlsTableLayoutPanel.Controls.Add(tabControl1, 0, 1);
        controlsTableLayoutPanel.Cursor = Cursors.HSplit;
        controlsTableLayoutPanel.Dock = DockStyle.Fill;
        controlsTableLayoutPanel.RowStyles.Add(new RowStyle());
        controlsTableLayoutPanel.RowStyles.Add(new RowStyle());
        controlsTableLayoutPanel.TabIndex = 3;

        // groupBox
        groupBox.AutoSize = true;
        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupBox.Controls.Add(this.controlsFlowPanel);
        groupBox.Dock = DockStyle.Fill;
        groupBox.TabIndex = 0;
        groupBox.TabStop = false;
        groupBox.Text = "Controls";

        // controlsFlowPanel
        this.controlsFlowPanel.AutoSize = true;
        this.controlsFlowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.controlsFlowPanel.Controls.Add(loadFromIServerButton);
        this.controlsFlowPanel.Controls.Add(processExcelDataSet);
        this.controlsFlowPanel.Controls.Add(loadVisioObjects);
        this.controlsFlowPanel.Controls.Add(layoutDataSet);
        this.controlsFlowPanel.Controls.Add(updateVisioDrawing);
        this.controlsFlowPanel.Controls.Add(drawBitmapButton);
        this.controlsFlowPanel.Dock = DockStyle.Fill;
        this.controlsFlowPanel.TabIndex = 0;

        // loadFromIServerButton
        loadFromIServerButton.AutoSize = true;
        loadFromIServerButton.Size = buttonSize;
        loadFromIServerButton.TabIndex = 1;
        loadFromIServerButton.Text = "Load from iServer";
        loadFromIServerButton.Click += this.LoadFromIServer_Click;

        // layoutDataSet
        layoutDataSet.AutoSize = true;
        layoutDataSet.Size = buttonSize;
        layoutDataSet.TabIndex = 4;
        layoutDataSet.Text = "Layout Data Set";
        layoutDataSet.Click += this.LayoutDataSet_Click;

        // updateVisioDrawing
        updateVisioDrawing.AutoSize = true;
        updateVisioDrawing.Size = buttonSize;
        updateVisioDrawing.TabIndex = 5;
        updateVisioDrawing.Text = "Update Visio Drawing";
        updateVisioDrawing.Click += this.UpdateVisioDrawing_Click;

        // tabControl1
        tabControl1.Controls.Add(dataSetTab);
        tabControl1.Controls.Add(parametersTab);
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.SelectedIndex = 0;
        tabControl1.TabIndex = 0;

        // dataSetTab
        dataSetTab.Controls.Add(this.dataGridView1);
        dataSetTab.TabIndex = 1;
        dataSetTab.Text = "Current Data Set";
        dataSetTab.UseVisualStyleBackColor = true;

        // dataGridView1
        this.dataGridView1.AllowUserToAddRows = false;
        this.dataGridView1.AllowUserToDeleteRows = false;
        this.dataGridView1.AllowUserToOrderColumns = true;
        this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dataGridView1.Dock = DockStyle.Fill;
        this.dataGridView1.ReadOnly = true;
        this.dataGridView1.RowHeadersWidth = 82;
        this.dataGridView1.RowTemplate.Height = 25;
        this.dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
        this.dataGridView1.ShowEditingIcon = false;
        this.dataGridView1.TabIndex = 0;

        // parametersTab
        parametersTab.Controls.Add(this.parametersDataGridView);
        parametersTab.TabIndex = 0;
        parametersTab.Text = "Parameters";
        parametersTab.UseVisualStyleBackColor = true;

        // parametersDataGridView
        this.parametersDataGridView.AllowUserToAddRows = false;
        this.parametersDataGridView.AllowUserToDeleteRows = false;
        this.parametersDataGridView.AllowUserToOrderColumns = true;
        this.parametersDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        this.parametersDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle1.BackColor = SystemColors.Control;
        dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
        this.parametersDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
        this.parametersDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle2.BackColor = SystemColors.Window;
        dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
        dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
        this.parametersDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
        this.parametersDataGridView.Dock = DockStyle.Fill;
        this.parametersDataGridView.Name = "parametersDataGridView";
        this.parametersDataGridView.RowHeadersWidth = 82;
        this.parametersDataGridView.RowTemplate.Height = 25;
        this.parametersDataGridView.TabIndex = 0;

        // tabPage1
        tabPage1.Controls.Add(sqlTableLayoutPanel);
        tabPage1.Name = "tabPage1";
        tabPage1.TabIndex = 2;
        tabPage1.Text = "Database Queries";
        tabPage1.UseVisualStyleBackColor = true;

        // sqlTableLayoutPanel
        sqlTableLayoutPanel.AutoSize = true;
        sqlTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        sqlTableLayoutPanel.ColumnCount = 1;
        sqlTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        sqlTableLayoutPanel.Controls.Add(this.selectSQLStatementComboBox, 0, 0);
        sqlTableLayoutPanel.Controls.Add(this.sqlStatementTextBox, 0, 1);
        sqlTableLayoutPanel.Dock = DockStyle.Fill;
        sqlTableLayoutPanel.Location = new Point(0, 0);
        sqlTableLayoutPanel.Name = "sqlTableLayoutPanel";
        sqlTableLayoutPanel.RowStyles.Add(new RowStyle());
        sqlTableLayoutPanel.RowStyles.Add(new RowStyle());
        sqlTableLayoutPanel.TabIndex = 0;

        // selectSQLStatementComboBox
        this.selectSQLStatementComboBox.Dock = DockStyle.Fill;
        this.selectSQLStatementComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        this.selectSQLStatementComboBox.TabIndex = 0;
        this.selectSQLStatementComboBox.SelectionChangeCommitted += this.SelectSqlStatementComboBoxSelectionChangeCommitted;

        // sqlStatementTextBox
        this.sqlStatementTextBox.AcceptsReturn = true;
        this.sqlStatementTextBox.AllowDrop = true;
        this.sqlStatementTextBox.Dock = DockStyle.Fill;
        this.sqlStatementTextBox.Multiline = true;
        this.sqlStatementTextBox.ScrollBars = ScrollBars.Both;
        this.sqlStatementTextBox.TabIndex = 1;

        // logGroupBox
        logGroupBox.Controls.Add(this.richTextLogBox);
        logGroupBox.Dock = DockStyle.Fill;
        logGroupBox.TabIndex = 0;
        logGroupBox.TabStop = false;
        logGroupBox.Text = "Log";

        // richTextLogBox
        this.richTextLogBox.BackColor = SystemColors.WindowText;
        this.richTextLogBox.Dock = DockStyle.Fill;
        this.richTextLogBox.ForeColor = SystemColors.Window;
        this.richTextLogBox.TabIndex = 1;
        this.richTextLogBox.Text = string.Empty;

        // parametersBindingSource
        this.parametersBindingSource.AllowNew = false;

        // MainForm
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.AutoSize = true;
        this.ClientSize = new Size(743, 480);
        this.Controls.Add(logSplitContainer);
        this.MinimumSize = new Size(759, 519);
        this.Name = "MainForm";
        this.Padding = new Padding(9);
        logSplitContainer.Panel1.ResumeLayout(false);
        logSplitContainer.Panel1.PerformLayout();
        logSplitContainer.Panel2.ResumeLayout(false);
        ((ISupportInitialize)logSplitContainer).EndInit();
        logSplitContainer.ResumeLayout(false);

        controlsTableLayoutPanel.ResumeLayout(false);
        controlsTableLayoutPanel.PerformLayout();
        groupBox.ResumeLayout(false);
        groupBox.PerformLayout();
        this.controlsFlowPanel.ResumeLayout(false);
        this.controlsFlowPanel.PerformLayout();
        tabControl1.ResumeLayout(false);
        dataSetTab.ResumeLayout(false);
        ((ISupportInitialize)this.dataGridView1).EndInit();
        parametersTab.ResumeLayout(false);
        ((ISupportInitialize)this.parametersDataGridView).EndInit();
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        sqlTableLayoutPanel.ResumeLayout(false);
        sqlTableLayoutPanel.PerformLayout();
        logGroupBox.ResumeLayout(false);
        ((ISupportInitialize)this.dataSetBindingSource).EndInit();
        ((ISupportInitialize)this.parametersBindingSource).EndInit();
        this.ResumeLayout(false);
    }
}

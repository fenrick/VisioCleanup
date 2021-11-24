// -----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms;

using System;
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

    private Button drawBitmapButton;

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
        TabControl tabControl1;
        TabPage dataSetTab;
        TabPage parametersTab;
        var dataGridViewCellStyle3 = new DataGridViewCellStyle();
        var dataGridViewCellStyle4 = new DataGridViewCellStyle();
        TabPage tabPage1;
        TableLayoutPanel sqlTableLayoutPanel;
        GroupBox logGroupBox;
        this.controlsFlowPanel = new FlowLayoutPanel();
        this.drawBitmapButton = new Button();
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
        // 
        // processExcelDataSet
        // 
        processExcelDataSet.AutoSize = true;
        processExcelDataSet.Location = new Point(139, 3);
        processExcelDataSet.Name = "processExcelDataSet";
        processExcelDataSet.Size = new Size(130, 25);
        processExcelDataSet.TabIndex = 2;
        processExcelDataSet.Text = "Process Excel DataSet";
        processExcelDataSet.Click += new EventHandler(this.ProcessExcelDataSet_Click);
        // 
        // loadVisioObjects
        // 
        loadVisioObjects.AutoSize = true;
        loadVisioObjects.Location = new Point(275, 3);
        loadVisioObjects.Name = "loadVisioObjects";
        loadVisioObjects.Size = new Size(130, 25);
        loadVisioObjects.TabIndex = 3;
        loadVisioObjects.Text = "Load Visio Objects";
        loadVisioObjects.Click += new EventHandler(this.LoadVisioObjects_Click);
        // 
        // logSplitContainer
        // 
        logSplitContainer.Cursor = Cursors.HSplit;
        logSplitContainer.Dock = DockStyle.Fill;
        logSplitContainer.Location = new Point(9, 9);
        logSplitContainer.Name = "logSplitContainer";
        logSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // logSplitContainer.Panel1
        // 
        logSplitContainer.Panel1.Controls.Add(controlsTableLayoutPanel);
        // 
        // logSplitContainer.Panel2
        // 
        logSplitContainer.Panel2.Controls.Add(logGroupBox);
        logSplitContainer.Size = new Size(725, 462);
        logSplitContainer.SplitterDistance = 231;
        logSplitContainer.TabIndex = 99;
        // 
        // controlsTableLayoutPanel
        // 
        controlsTableLayoutPanel.AutoSize = true;
        controlsTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        controlsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        controlsTableLayoutPanel.Controls.Add(groupBox, 0, 0);
        controlsTableLayoutPanel.Controls.Add(tabControl1, 0, 1);
        controlsTableLayoutPanel.Cursor = Cursors.HSplit;
        controlsTableLayoutPanel.Dock = DockStyle.Fill;
        controlsTableLayoutPanel.Location = new Point(0, 0);
        controlsTableLayoutPanel.Name = "controlsTableLayoutPanel";
        controlsTableLayoutPanel.RowStyles.Add(new RowStyle());
        controlsTableLayoutPanel.RowStyles.Add(new RowStyle());
        controlsTableLayoutPanel.Size = new Size(725, 231);
        controlsTableLayoutPanel.TabIndex = 3;
        // 
        // groupBox
        // 
        groupBox.AutoSize = true;
        groupBox.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        groupBox.Controls.Add(this.controlsFlowPanel);
        groupBox.Dock = DockStyle.Fill;
        groupBox.Location = new Point(3, 3);
        groupBox.Name = "groupBox";
        groupBox.Size = new Size(1334, 53);
        groupBox.TabIndex = 0;
        groupBox.TabStop = false;
        groupBox.Text = "Controls";
        // 
        // controlsFlowPanel
        // 
        this.controlsFlowPanel.AutoSize = true;
        this.controlsFlowPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        this.controlsFlowPanel.Controls.Add(loadFromIServerButton);
        this.controlsFlowPanel.Controls.Add(processExcelDataSet);
        this.controlsFlowPanel.Controls.Add(loadVisioObjects);
        this.controlsFlowPanel.Controls.Add(layoutDataSet);
        this.controlsFlowPanel.Controls.Add(updateVisioDrawing);
        this.controlsFlowPanel.Controls.Add(this.drawBitmapButton);
        this.controlsFlowPanel.Dock = DockStyle.Fill;
        this.controlsFlowPanel.Location = new Point(3, 19);
        this.controlsFlowPanel.Name = "controlsFlowPanel";
        this.controlsFlowPanel.Size = new Size(1328, 31);
        this.controlsFlowPanel.TabIndex = 0;
        // 
        // loadFromIServerButton
        // 
        loadFromIServerButton.AutoSize = true;
        loadFromIServerButton.Location = new Point(3, 3);
        loadFromIServerButton.Name = "loadFromIServerButton";
        loadFromIServerButton.Size = new Size(130, 25);
        loadFromIServerButton.TabIndex = 1;
        loadFromIServerButton.Text = "Load from iServer";
        loadFromIServerButton.Click += new EventHandler(this.LoadFromIServer_Click);
        // 
        // layoutDataSet
        // 
        layoutDataSet.AutoSize = true;
        layoutDataSet.Location = new Point(411, 3);
        layoutDataSet.Name = "layoutDataSet";
        layoutDataSet.Size = new Size(130, 25);
        layoutDataSet.TabIndex = 4;
        layoutDataSet.Text = "Layout Data Set";
        layoutDataSet.Click += new EventHandler(this.LayoutDataSet_Click);
        // 
        // updateVisioDrawing
        // 
        updateVisioDrawing.AutoSize = true;
        updateVisioDrawing.Location = new Point(547, 3);
        updateVisioDrawing.Name = "updateVisioDrawing";
        updateVisioDrawing.Size = new Size(130, 25);
        updateVisioDrawing.TabIndex = 5;
        updateVisioDrawing.Text = "Update Visio Drawing";
        updateVisioDrawing.Click += new EventHandler(this.UpdateVisioDrawing_Click);
        // 
        // drawBitmapButton
        // 
        this.drawBitmapButton.AutoSize = true;
        this.drawBitmapButton.Location = new Point(683, 3);
        this.drawBitmapButton.Name = "drawBitmapButton";
        this.drawBitmapButton.Size = new Size(130, 25);
        this.drawBitmapButton.TabIndex = 6;
        this.drawBitmapButton.Text = "Draw Bitmap";
        this.drawBitmapButton.Click += new EventHandler(this.DrawBitmapButton_Click);
        // 
        // tabControl1
        // 
        tabControl1.Controls.Add(dataSetTab);
        tabControl1.Controls.Add(parametersTab);
        tabControl1.Controls.Add(tabPage1);
        tabControl1.Dock = DockStyle.Fill;
        tabControl1.Location = new Point(3, 62);
        tabControl1.Name = "tabControl1";
        tabControl1.SelectedIndex = 0;
        tabControl1.Size = new Size(1334, 174);
        tabControl1.TabIndex = 0;
        // 
        // dataSetTab
        // 
        dataSetTab.Controls.Add(this.dataGridView1);
        dataSetTab.Location = new Point(4, 24);
        dataSetTab.Name = "dataSetTab";
        dataSetTab.Size = new Size(1326, 146);
        dataSetTab.TabIndex = 1;
        dataSetTab.Text = "Current Data Set";
        dataSetTab.UseVisualStyleBackColor = true;
        // 
        // dataGridView1
        // 
        this.dataGridView1.AllowUserToAddRows = false;
        this.dataGridView1.AllowUserToDeleteRows = false;
        this.dataGridView1.AllowUserToOrderColumns = true;
        this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        this.dataGridView1.Dock = DockStyle.Fill;
        this.dataGridView1.Location = new Point(0, 0);
        this.dataGridView1.Name = "dataGridView1";
        this.dataGridView1.ReadOnly = true;
        this.dataGridView1.RowHeadersWidth = 82;
        this.dataGridView1.RowTemplate.Height = 25;
        this.dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
        this.dataGridView1.ShowEditingIcon = false;
        this.dataGridView1.Size = new Size(1326, 146);
        this.dataGridView1.TabIndex = 0;
        // 
        // parametersTab
        // 
        parametersTab.Controls.Add(this.parametersDataGridView);
        parametersTab.Location = new Point(4, 24);
        parametersTab.Name = "parametersTab";
        parametersTab.Size = new Size(1326, 146);
        parametersTab.TabIndex = 0;
        parametersTab.Text = "Parameters";
        parametersTab.UseVisualStyleBackColor = true;
        // 
        // parametersDataGridView
        // 
        this.parametersDataGridView.AllowUserToAddRows = false;
        this.parametersDataGridView.AllowUserToDeleteRows = false;
        this.parametersDataGridView.AllowUserToOrderColumns = true;
        this.parametersDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        this.parametersDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle3.BackColor = SystemColors.Control;
        dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
        dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
        this.parametersDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
        this.parametersDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dataGridViewCellStyle4.BackColor = SystemColors.Window;
        dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        dataGridViewCellStyle4.ForeColor = SystemColors.ControlText;
        dataGridViewCellStyle4.SelectionBackColor = SystemColors.Highlight;
        dataGridViewCellStyle4.SelectionForeColor = SystemColors.HighlightText;
        dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
        this.parametersDataGridView.DefaultCellStyle = dataGridViewCellStyle4;
        this.parametersDataGridView.Dock = DockStyle.Fill;
        this.parametersDataGridView.Location = new Point(0, 0);
        this.parametersDataGridView.Name = "parametersDataGridView";
        this.parametersDataGridView.RowHeadersWidth = 82;
        this.parametersDataGridView.RowTemplate.Height = 25;
        this.parametersDataGridView.Size = new Size(1326, 146);
        this.parametersDataGridView.TabIndex = 0;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(sqlTableLayoutPanel);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Name = "tabPage1";
        tabPage1.Size = new Size(1326, 146);
        tabPage1.TabIndex = 2;
        tabPage1.Text = "Database Queries";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // sqlTableLayoutPanel
        // 
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
        sqlTableLayoutPanel.Size = new Size(1326, 146);
        sqlTableLayoutPanel.TabIndex = 0;
        // 
        // selectSQLStatementComboBox
        // 
        this.selectSQLStatementComboBox.Dock = DockStyle.Fill;
        this.selectSQLStatementComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        this.selectSQLStatementComboBox.Location = new Point(3, 3);
        this.selectSQLStatementComboBox.Name = "selectSQLStatementComboBox";
        this.selectSQLStatementComboBox.Size = new Size(1326, 23);
        this.selectSQLStatementComboBox.TabIndex = 0;
        this.selectSQLStatementComboBox.SelectionChangeCommitted += new EventHandler(this.SelectSqlStatementComboBoxSelectionChangeCommitted);
        // 
        // sqlStatementTextBox
        // 
        this.sqlStatementTextBox.AcceptsReturn = true;
        this.sqlStatementTextBox.AllowDrop = true;
        this.sqlStatementTextBox.Dock = DockStyle.Fill;
        this.sqlStatementTextBox.Location = new Point(3, 32);
        this.sqlStatementTextBox.Multiline = true;
        this.sqlStatementTextBox.Name = "sqlStatementTextBox";
        this.sqlStatementTextBox.ScrollBars = ScrollBars.Both;
        this.sqlStatementTextBox.Size = new Size(1326, 138);
        this.sqlStatementTextBox.TabIndex = 1;
        // 
        // logGroupBox
        // 
        logGroupBox.Controls.Add(this.richTextLogBox);
        logGroupBox.Dock = DockStyle.Fill;
        logGroupBox.Location = new Point(0, 0);
        logGroupBox.Name = "logGroupBox";
        logGroupBox.Size = new Size(725, 227);
        logGroupBox.TabIndex = 0;
        logGroupBox.TabStop = false;
        logGroupBox.Text = "Log";
        // 
        // richTextLogBox
        // 
        this.richTextLogBox.BackColor = SystemColors.WindowText;
        this.richTextLogBox.Dock = DockStyle.Fill;
        this.richTextLogBox.ForeColor = SystemColors.Window;
        this.richTextLogBox.Location = new Point(3, 19);
        this.richTextLogBox.Name = "richTextLogBox";
        this.richTextLogBox.Size = new Size(719, 205);
        this.richTextLogBox.TabIndex = 1;
        this.richTextLogBox.Text = "";
        // 
        // parametersBindingSource
        // 
        this.parametersBindingSource.AllowNew = false;
        // 
        // MainForm
        // 
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.AutoSize = true;
        this.ClientSize = new Size(743, 480);
        this.Controls.Add(logSplitContainer);
        this.MinimumSize = new Size(754, 494);
        this.Name = "MainForm";
        this.Padding = new Padding(9, 9, 9, 9);
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

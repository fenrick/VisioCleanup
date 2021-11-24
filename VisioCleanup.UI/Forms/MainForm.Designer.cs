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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Button processExcelDataSet;
            System.Windows.Forms.Button loadVisioObjects;
            System.Windows.Forms.SplitContainer logSplitContainer;
            System.Windows.Forms.TableLayoutPanel controlsTableLayoutPanel;
            System.Windows.Forms.GroupBox groupBox;
            System.Windows.Forms.Button loadFromIServerButton;
            System.Windows.Forms.Button layoutDataSet;
            System.Windows.Forms.Button updateVisioDrawing;
            System.Windows.Forms.TabControl tabControl1;
            System.Windows.Forms.TabPage dataSetTab;
            System.Windows.Forms.TabPage parametersTab;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.TabPage tabPage1;
            System.Windows.Forms.TableLayoutPanel sqlTableLayoutPanel;
            System.Windows.Forms.GroupBox logGroupBox;
            this.controlsFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.drawBitmapButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.parametersDataGridView = new System.Windows.Forms.DataGridView();
            this.selectSQLStatementComboBox = new System.Windows.Forms.ComboBox();
            this.sqlStatementTextBox = new System.Windows.Forms.TextBox();
            this.richTextLogBox = new System.Windows.Forms.RichTextBox();
            this.dataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.parametersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            processExcelDataSet = new System.Windows.Forms.Button();
            loadVisioObjects = new System.Windows.Forms.Button();
            logSplitContainer = new System.Windows.Forms.SplitContainer();
            controlsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            groupBox = new System.Windows.Forms.GroupBox();
            loadFromIServerButton = new System.Windows.Forms.Button();
            layoutDataSet = new System.Windows.Forms.Button();
            updateVisioDrawing = new System.Windows.Forms.Button();
            tabControl1 = new System.Windows.Forms.TabControl();
            dataSetTab = new System.Windows.Forms.TabPage();
            parametersTab = new System.Windows.Forms.TabPage();
            tabPage1 = new System.Windows.Forms.TabPage();
            sqlTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            logGroupBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(logSplitContainer)).BeginInit();
            logSplitContainer.Panel1.SuspendLayout();
            logSplitContainer.Panel2.SuspendLayout();
            logSplitContainer.SuspendLayout();
            controlsTableLayoutPanel.SuspendLayout();
            groupBox.SuspendLayout();
            this.controlsFlowPanel.SuspendLayout();
            tabControl1.SuspendLayout();
            dataSetTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            parametersTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parametersDataGridView)).BeginInit();
            tabPage1.SuspendLayout();
            sqlTableLayoutPanel.SuspendLayout();
            logGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.parametersBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // processExcelDataSet
            // 
            processExcelDataSet.AutoSize = true;
            processExcelDataSet.Location = new System.Drawing.Point(139, 3);
            processExcelDataSet.Name = "processExcelDataSet";
            processExcelDataSet.Size = new System.Drawing.Size(130, 25);
            processExcelDataSet.TabIndex = 2;
            processExcelDataSet.Text = "Process Excel DataSet";
            processExcelDataSet.Click += new System.EventHandler(this.ProcessExcelDataSet_Click);
            // 
            // loadVisioObjects
            // 
            loadVisioObjects.AutoSize = true;
            loadVisioObjects.Location = new System.Drawing.Point(275, 3);
            loadVisioObjects.Name = "loadVisioObjects";
            loadVisioObjects.Size = new System.Drawing.Size(130, 25);
            loadVisioObjects.TabIndex = 3;
            loadVisioObjects.Text = "Load Visio Objects";
            loadVisioObjects.Click += new System.EventHandler(this.LoadVisioObjects_Click);
            // 
            // logSplitContainer
            // 
            logSplitContainer.Cursor = System.Windows.Forms.Cursors.HSplit;
            logSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            logSplitContainer.Location = new System.Drawing.Point(9, 9);
            logSplitContainer.Name = "logSplitContainer";
            logSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // logSplitContainer.Panel1
            // 
            logSplitContainer.Panel1.Controls.Add(controlsTableLayoutPanel);
            // 
            // logSplitContainer.Panel2
            // 
            logSplitContainer.Panel2.Controls.Add(logGroupBox);
            logSplitContainer.Size = new System.Drawing.Size(725, 462);
            logSplitContainer.SplitterDistance = 231;
            logSplitContainer.TabIndex = 99;
            // 
            // controlsTableLayoutPanel
            // 
            controlsTableLayoutPanel.AutoSize = true;
            controlsTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            controlsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            controlsTableLayoutPanel.Controls.Add(groupBox, 0, 0);
            controlsTableLayoutPanel.Controls.Add(tabControl1, 0, 1);
            controlsTableLayoutPanel.Cursor = System.Windows.Forms.Cursors.HSplit;
            controlsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            controlsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            controlsTableLayoutPanel.Name = "controlsTableLayoutPanel";
            controlsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            controlsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            controlsTableLayoutPanel.Size = new System.Drawing.Size(725, 231);
            controlsTableLayoutPanel.TabIndex = 3;
            // 
            // groupBox
            // 
            groupBox.AutoSize = true;
            groupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            groupBox.Controls.Add(this.controlsFlowPanel);
            groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox.Location = new System.Drawing.Point(3, 3);
            groupBox.Name = "groupBox";
            groupBox.Size = new System.Drawing.Size(1334, 53);
            groupBox.TabIndex = 0;
            groupBox.TabStop = false;
            groupBox.Text = "Controls";
            // 
            // controlsFlowPanel
            // 
            this.controlsFlowPanel.AutoSize = true;
            this.controlsFlowPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.controlsFlowPanel.Controls.Add(loadFromIServerButton);
            this.controlsFlowPanel.Controls.Add(processExcelDataSet);
            this.controlsFlowPanel.Controls.Add(loadVisioObjects);
            this.controlsFlowPanel.Controls.Add(layoutDataSet);
            this.controlsFlowPanel.Controls.Add(updateVisioDrawing);
            this.controlsFlowPanel.Controls.Add(this.drawBitmapButton);
            this.controlsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsFlowPanel.Location = new System.Drawing.Point(3, 19);
            this.controlsFlowPanel.Name = "controlsFlowPanel";
            this.controlsFlowPanel.Size = new System.Drawing.Size(1328, 31);
            this.controlsFlowPanel.TabIndex = 0;
            // 
            // loadFromIServerButton
            // 
            loadFromIServerButton.AutoSize = true;
            loadFromIServerButton.Location = new System.Drawing.Point(3, 3);
            loadFromIServerButton.Name = "loadFromIServerButton";
            loadFromIServerButton.Size = new System.Drawing.Size(130, 25);
            loadFromIServerButton.TabIndex = 1;
            loadFromIServerButton.Text = "Load from iServer";
            loadFromIServerButton.Click += new System.EventHandler(this.LoadFromIServer_Click);
            // 
            // layoutDataSet
            // 
            layoutDataSet.AutoSize = true;
            layoutDataSet.Location = new System.Drawing.Point(411, 3);
            layoutDataSet.Name = "layoutDataSet";
            layoutDataSet.Size = new System.Drawing.Size(130, 25);
            layoutDataSet.TabIndex = 4;
            layoutDataSet.Text = "Layout Data Set";
            layoutDataSet.Click += new System.EventHandler(this.LayoutDataSet_Click);
            // 
            // updateVisioDrawing
            // 
            updateVisioDrawing.AutoSize = true;
            updateVisioDrawing.Location = new System.Drawing.Point(547, 3);
            updateVisioDrawing.Name = "updateVisioDrawing";
            updateVisioDrawing.Size = new System.Drawing.Size(130, 25);
            updateVisioDrawing.TabIndex = 5;
            updateVisioDrawing.Text = "Update Visio Drawing";
            updateVisioDrawing.Click += new System.EventHandler(this.UpdateVisioDrawing_Click);
            // 
            // drawBitmapButton
            // 
            this.drawBitmapButton.AutoSize = true;
            this.drawBitmapButton.Location = new System.Drawing.Point(683, 3);
            this.drawBitmapButton.Name = "drawBitmapButton";
            this.drawBitmapButton.Size = new System.Drawing.Size(130, 25);
            this.drawBitmapButton.TabIndex = 6;
            this.drawBitmapButton.Text = "Draw Bitmap";
            this.drawBitmapButton.Click += new System.EventHandler(this.DrawBitmapButton_Click);
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(dataSetTab);
            tabControl1.Controls.Add(parametersTab);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(3, 62);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(1334, 174);
            tabControl1.TabIndex = 0;
            // 
            // dataSetTab
            // 
            dataSetTab.Controls.Add(this.dataGridView1);
            dataSetTab.Location = new System.Drawing.Point(4, 24);
            dataSetTab.Name = "dataSetTab";
            dataSetTab.Size = new System.Drawing.Size(1326, 146);
            dataSetTab.TabIndex = 1;
            dataSetTab.Text = "Current Data Set";
            dataSetTab.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new System.Drawing.Size(1326, 146);
            this.dataGridView1.TabIndex = 0;
            // 
            // parametersTab
            // 
            parametersTab.Controls.Add(this.parametersDataGridView);
            parametersTab.Location = new System.Drawing.Point(4, 24);
            parametersTab.Name = "parametersTab";
            parametersTab.Size = new System.Drawing.Size(1326, 146);
            parametersTab.TabIndex = 0;
            parametersTab.Text = "Parameters";
            parametersTab.UseVisualStyleBackColor = true;
            // 
            // parametersDataGridView
            // 
            this.parametersDataGridView.AllowUserToAddRows = false;
            this.parametersDataGridView.AllowUserToDeleteRows = false;
            this.parametersDataGridView.AllowUserToOrderColumns = true;
            this.parametersDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.parametersDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.parametersDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.parametersDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.parametersDataGridView.DefaultCellStyle = dataGridViewCellStyle4;
            this.parametersDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parametersDataGridView.Location = new System.Drawing.Point(0, 0);
            this.parametersDataGridView.Name = "parametersDataGridView";
            this.parametersDataGridView.RowHeadersWidth = 82;
            this.parametersDataGridView.RowTemplate.Height = 25;
            this.parametersDataGridView.Size = new System.Drawing.Size(1326, 146);
            this.parametersDataGridView.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(sqlTableLayoutPanel);
            tabPage1.Location = new System.Drawing.Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new System.Drawing.Size(1326, 146);
            tabPage1.TabIndex = 2;
            tabPage1.Text = "Database Queries";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // sqlTableLayoutPanel
            // 
            sqlTableLayoutPanel.AutoSize = true;
            sqlTableLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            sqlTableLayoutPanel.ColumnCount = 1;
            sqlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            sqlTableLayoutPanel.Controls.Add(this.selectSQLStatementComboBox, 0, 0);
            sqlTableLayoutPanel.Controls.Add(this.sqlStatementTextBox, 0, 1);
            sqlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            sqlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            sqlTableLayoutPanel.Name = "sqlTableLayoutPanel";
            sqlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            sqlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            sqlTableLayoutPanel.Size = new System.Drawing.Size(1326, 146);
            sqlTableLayoutPanel.TabIndex = 0;
            // 
            // selectSQLStatementComboBox
            // 
            this.selectSQLStatementComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectSQLStatementComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.selectSQLStatementComboBox.Location = new System.Drawing.Point(3, 3);
            this.selectSQLStatementComboBox.Name = "selectSQLStatementComboBox";
            this.selectSQLStatementComboBox.Size = new System.Drawing.Size(1326, 23);
            this.selectSQLStatementComboBox.TabIndex = 0;
            this.selectSQLStatementComboBox.SelectionChangeCommitted += new System.EventHandler(this.SelectSqlStatementComboBoxSelectionChangeCommitted);
            // 
            // sqlStatementTextBox
            // 
            this.sqlStatementTextBox.AcceptsReturn = true;
            this.sqlStatementTextBox.AllowDrop = true;
            this.sqlStatementTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sqlStatementTextBox.Location = new System.Drawing.Point(3, 32);
            this.sqlStatementTextBox.Multiline = true;
            this.sqlStatementTextBox.Name = "sqlStatementTextBox";
            this.sqlStatementTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.sqlStatementTextBox.Size = new System.Drawing.Size(1326, 138);
            this.sqlStatementTextBox.TabIndex = 1;
            // 
            // logGroupBox
            // 
            logGroupBox.Controls.Add(this.richTextLogBox);
            logGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            logGroupBox.Location = new System.Drawing.Point(0, 0);
            logGroupBox.Name = "logGroupBox";
            logGroupBox.Size = new System.Drawing.Size(725, 227);
            logGroupBox.TabIndex = 0;
            logGroupBox.TabStop = false;
            logGroupBox.Text = "Log";
            // 
            // richTextLogBox
            // 
            this.richTextLogBox.BackColor = System.Drawing.SystemColors.WindowText;
            this.richTextLogBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextLogBox.ForeColor = System.Drawing.SystemColors.Window;
            this.richTextLogBox.Location = new System.Drawing.Point(3, 19);
            this.richTextLogBox.Name = "richTextLogBox";
            this.richTextLogBox.Size = new System.Drawing.Size(719, 205);
            this.richTextLogBox.TabIndex = 1;
            this.richTextLogBox.Text = "";
            // 
            // parametersBindingSource
            // 
            this.parametersBindingSource.AllowNew = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(743, 480);
            this.Controls.Add(logSplitContainer);
            this.MinimumSize = new System.Drawing.Size(754, 494);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(9, 9, 9, 9);
            logSplitContainer.Panel1.ResumeLayout(false);
            logSplitContainer.Panel1.PerformLayout();
            logSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(logSplitContainer)).EndInit();
            logSplitContainer.ResumeLayout(false);
            controlsTableLayoutPanel.ResumeLayout(false);
            controlsTableLayoutPanel.PerformLayout();
            groupBox.ResumeLayout(false);
            groupBox.PerformLayout();
            this.controlsFlowPanel.ResumeLayout(false);
            this.controlsFlowPanel.PerformLayout();
            tabControl1.ResumeLayout(false);
            dataSetTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            parametersTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.parametersDataGridView)).EndInit();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            sqlTableLayoutPanel.ResumeLayout(false);
            sqlTableLayoutPanel.PerformLayout();
            logGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataSetBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.parametersBindingSource)).EndInit();
            this.ResumeLayout(false);

    }

    private Button drawBitmapButton;
}

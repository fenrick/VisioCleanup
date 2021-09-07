// -----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>Main application form.</summary>
    public partial class MainForm
    {
        /// <summary>Required designer variable.</summary>
        private IContainer components;

        private FlowLayoutPanel controlsFlowPanel;

        private SplitContainer controlSplitContainer;

        private DataGridView dataGridView1;

        private BindingSource dataSetBindingSource;

        private TabPage dataSetTab;

        private Button layoutDataSet;

        private Button loadFromIServerButton;

        private BindingSource parametersBindingSource;

        private DataGridView parametersDataGridView;

        private TabPage parametersTab;

        private RichTextBox richTextLogBox;

        private ComboBox selectSQLStatementComboBox;

        private TextBox sqlStatementTextBox;

        private TabControl tabControl1;

        private TabPage tabPage1;

        private Button updateVisioDrawing;

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
            var dataGridViewCellStyle1 = new DataGridViewCellStyle();
            var dataGridViewCellStyle2 = new DataGridViewCellStyle();
            this.controlSplitContainer = new SplitContainer();
            this.controlsFlowPanel = new FlowLayoutPanel();
            this.loadFromIServerButton = new Button();
            this.layoutDataSet = new Button();
            this.updateVisioDrawing = new Button();
            this.tabControl1 = new TabControl();
            this.dataSetTab = new TabPage();
            this.dataGridView1 = new DataGridView();
            this.parametersTab = new TabPage();
            this.parametersDataGridView = new DataGridView();
            this.tabPage1 = new TabPage();
            this.sqlStatementTextBox = new TextBox();
            this.selectSQLStatementComboBox = new ComboBox();
            this.richTextLogBox = new RichTextBox();
            this.parametersBindingSource = new BindingSource(this.components);
            this.dataSetBindingSource = new BindingSource(this.components);
            processExcelDataSet = new Button();
            loadVisioObjects = new Button();
            logSplitContainer = new SplitContainer();
            ((ISupportInitialize)logSplitContainer).BeginInit();
            logSplitContainer.Panel1.SuspendLayout();
            logSplitContainer.Panel2.SuspendLayout();
            logSplitContainer.SuspendLayout();
            ((ISupportInitialize)this.controlSplitContainer).BeginInit();
            this.controlSplitContainer.Panel1.SuspendLayout();
            this.controlSplitContainer.Panel2.SuspendLayout();
            this.controlSplitContainer.SuspendLayout();
            this.controlsFlowPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.dataSetTab.SuspendLayout();
            ((ISupportInitialize)this.dataGridView1).BeginInit();
            this.parametersTab.SuspendLayout();
            ((ISupportInitialize)this.parametersDataGridView).BeginInit();
            this.tabPage1.SuspendLayout();
            ((ISupportInitialize)this.parametersBindingSource).BeginInit();
            ((ISupportInitialize)this.dataSetBindingSource).BeginInit();
            this.SuspendLayout();
            var buttonPadding = new Padding(0, 0, 0, 26);

            // processExcelDataSet
            processExcelDataSet.Location = new Point(22, 100);
            processExcelDataSet.Margin = buttonPadding;
            processExcelDataSet.Name = "processExcelDataSet";
            processExcelDataSet.Size = new Size(268, 48);
            processExcelDataSet.TabIndex = 2;
            processExcelDataSet.Text = "Process Excel DataSet";
            processExcelDataSet.UseVisualStyleBackColor = true;
            processExcelDataSet.Click += this.ProcessExcelDataSet_Click;

            // loadVisioObjects
            loadVisioObjects.Location = new Point(22, 174);
            loadVisioObjects.Margin = buttonPadding;
            loadVisioObjects.Name = "loadVisioObjects";
            loadVisioObjects.Size = new Size(268, 48);
            loadVisioObjects.TabIndex = 3;
            loadVisioObjects.Text = "Load Visio Objects";
            loadVisioObjects.UseVisualStyleBackColor = true;
            loadVisioObjects.Click += this.LoadVisioObjects_Click;

            // logSplitContainer
            logSplitContainer.Dock = DockStyle.Fill;
            logSplitContainer.Location = new Point(0, 0);
            logSplitContainer.Margin = new Padding(0);
            logSplitContainer.Name = "logSplitContainer";
            logSplitContainer.Orientation = Orientation.Horizontal;

            // logSplitContainer.Panel1
            logSplitContainer.Panel1.Controls.Add(this.controlSplitContainer);

            // logSplitContainer.Panel2
            logSplitContainer.Panel2.Controls.Add(this.richTextLogBox);
            logSplitContainer.Panel2.Padding = new Padding(22, 26, 22, 26);
            logSplitContainer.Size = new Size(1486, 960);
            logSplitContainer.SplitterDistance = 468;
            logSplitContainer.SplitterWidth = 8;
            logSplitContainer.TabIndex = 5;

            // controlSplitContainer
            this.controlSplitContainer.Dock = DockStyle.Fill;
            this.controlSplitContainer.FixedPanel = FixedPanel.Panel1;
            this.controlSplitContainer.Location = new Point(0, 0);
            this.controlSplitContainer.Margin = new Padding(0);
            this.controlSplitContainer.Name = "controlSplitContainer";

            // controlSplitContainer.Panel1
            this.controlSplitContainer.Panel1.Controls.Add(this.controlsFlowPanel);

            // controlSplitContainer.Panel2
            this.controlSplitContainer.Panel2.Controls.Add(this.tabControl1);
            this.controlSplitContainer.Panel2.Padding = new Padding(22, 26, 22, 26);
            this.controlSplitContainer.Size = new Size(1486, 468);
            this.controlSplitContainer.SplitterDistance = 169;
            this.controlSplitContainer.SplitterWidth = 8;
            this.controlSplitContainer.TabIndex = 3;

            // controlsFlowPanel
            this.controlsFlowPanel.Controls.Add(this.loadFromIServerButton);
            this.controlsFlowPanel.Controls.Add(processExcelDataSet);
            this.controlsFlowPanel.Controls.Add(loadVisioObjects);
            this.controlsFlowPanel.Controls.Add(this.layoutDataSet);
            this.controlsFlowPanel.Controls.Add(this.updateVisioDrawing);
            this.controlsFlowPanel.Dock = DockStyle.Fill;
            this.controlsFlowPanel.Location = new Point(0, 0);
            this.controlsFlowPanel.Margin = new Padding(0);
            this.controlsFlowPanel.Name = "controlsFlowPanel";
            this.controlsFlowPanel.Padding = new Padding(22, 26, 22, 26);
            this.controlsFlowPanel.Size = new Size(169, 468);
            this.controlsFlowPanel.TabIndex = 0;

            // loadFromIServerButton
            this.loadFromIServerButton.Location = new Point(22, 26);
            this.loadFromIServerButton.Margin = buttonPadding;
            this.loadFromIServerButton.Name = "loadFromIServerButton";
            this.loadFromIServerButton.Size = new Size(268, 48);
            this.loadFromIServerButton.TabIndex = 1;
            this.loadFromIServerButton.Text = "Load from iServer";
            this.loadFromIServerButton.UseVisualStyleBackColor = true;
            this.loadFromIServerButton.Click += this.LoadFromIServer_Click;

            // layoutDataSet
            this.layoutDataSet.Location = new Point(22, 248);
            this.layoutDataSet.Margin = buttonPadding;
            this.layoutDataSet.Name = "layoutDataSet";
            this.layoutDataSet.Size = new Size(268, 48);
            this.layoutDataSet.TabIndex = 4;
            this.layoutDataSet.Text = "Layout Data Set";
            this.layoutDataSet.UseVisualStyleBackColor = true;
            this.layoutDataSet.Click += this.LayoutDataSet_Click;

            // updateVisioDrawing
            this.updateVisioDrawing.Location = new Point(22, 322);
            this.updateVisioDrawing.Margin = buttonPadding;
            this.updateVisioDrawing.Name = "updateVisioDrawing";
            this.updateVisioDrawing.Size = new Size(268, 48);
            this.updateVisioDrawing.TabIndex = 5;
            this.updateVisioDrawing.Text = "Update Visio Drawing";
            this.updateVisioDrawing.UseVisualStyleBackColor = true;
            this.updateVisioDrawing.Click += this.UpdateVisioDrawing_Click;

            // tabControl1
            this.tabControl1.Controls.Add(this.dataSetTab);
            this.tabControl1.Controls.Add(this.parametersTab);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = DockStyle.Fill;
            this.tabControl1.Location = new Point(22, 26);
            this.tabControl1.Margin = new Padding(6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new Size(1265, 416);
            this.tabControl1.TabIndex = 0;

            // dataSetTab
            this.dataSetTab.Controls.Add(this.dataGridView1);
            this.dataSetTab.Location = new Point(8, 46);
            this.dataSetTab.Margin = new Padding(6);
            this.dataSetTab.Name = "dataSetTab";
            this.dataSetTab.Padding = new Padding(6);
            this.dataSetTab.Size = new Size(1249, 362);
            this.dataSetTab.TabIndex = 1;
            this.dataSetTab.Text = "Current Data Set";
            this.dataSetTab.UseVisualStyleBackColor = true;

            // dataGridView1
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = DockStyle.Fill;
            this.dataGridView1.Location = new Point(6, 6);
            this.dataGridView1.Margin = new Padding(6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 82;
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.SelectionMode = DataGridViewSelectionMode.CellSelect;
            this.dataGridView1.ShowEditingIcon = false;
            this.dataGridView1.Size = new Size(1237, 350);
            this.dataGridView1.TabIndex = 0;

            // parametersTab
            this.parametersTab.Controls.Add(this.parametersDataGridView);
            this.parametersTab.Location = new Point(8, 46);
            this.parametersTab.Margin = new Padding(6);
            this.parametersTab.Name = "parametersTab";
            this.parametersTab.Padding = new Padding(6);
            this.parametersTab.Size = new Size(1249, 362);
            this.parametersTab.TabIndex = 0;
            this.parametersTab.Text = "Parameters";
            this.parametersTab.UseVisualStyleBackColor = true;

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
            this.parametersDataGridView.Location = new Point(6, 6);
            this.parametersDataGridView.Margin = new Padding(6);
            this.parametersDataGridView.Name = "parametersDataGridView";
            this.parametersDataGridView.RowHeadersWidth = 82;
            this.parametersDataGridView.RowTemplate.Height = 25;
            this.parametersDataGridView.Size = new Size(1237, 350);
            this.parametersDataGridView.TabIndex = 0;

            // tabPage1
            this.tabPage1.Controls.Add(this.sqlStatementTextBox);
            this.tabPage1.Controls.Add(this.selectSQLStatementComboBox);
            this.tabPage1.Location = new Point(8, 46);
            this.tabPage1.Margin = new Padding(6);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new Padding(6);
            this.tabPage1.Size = new Size(1249, 362);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Database Queries";
            this.tabPage1.UseVisualStyleBackColor = true;

            // sqlStatementTextBox
            this.sqlStatementTextBox.AcceptsReturn = true;
            this.sqlStatementTextBox.AllowDrop = true;
            this.sqlStatementTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.sqlStatementTextBox.Location = new Point(8, 68);
            this.sqlStatementTextBox.Margin = new Padding(6);
            this.sqlStatementTextBox.Multiline = true;
            this.sqlStatementTextBox.Name = "sqlStatementTextBox";
            this.sqlStatementTextBox.ScrollBars = ScrollBars.Both;
            this.sqlStatementTextBox.Size = new Size(1235, 288);
            this.sqlStatementTextBox.TabIndex = 1;

            // selectSQLStatementComboBox
            this.selectSQLStatementComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.selectSQLStatementComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.selectSQLStatementComboBox.Location = new Point(8, 8);
            this.selectSQLStatementComboBox.Margin = new Padding(6);
            this.selectSQLStatementComboBox.Name = "selectSQLStatementComboBox";
            this.selectSQLStatementComboBox.Size = new Size(1215, 40);
            this.selectSQLStatementComboBox.TabIndex = 0;
            this.selectSQLStatementComboBox.SelectionChangeCommitted += this.SelectSqlStatementComboBoxSelectionChangeCommitted;

            // richTextLogBox
            this.richTextLogBox.BackColor = SystemColors.WindowText;
            this.richTextLogBox.Dock = DockStyle.Fill;
            this.richTextLogBox.ForeColor = SystemColors.Window;
            this.richTextLogBox.Location = new Point(22, 26);
            this.richTextLogBox.Margin = new Padding(6);
            this.richTextLogBox.Name = "richTextLogBox";
            this.richTextLogBox.Size = new Size(1442, 432);
            this.richTextLogBox.TabIndex = 1;
            this.richTextLogBox.Text = string.Empty;

            // parametersBindingSource
            this.parametersBindingSource.AllowNew = false;

            // MainForm
            this.AutoScaleDimensions = new SizeF(192F, 192F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(1486, 960);
            this.Controls.Add(logSplitContainer);
            this.Margin = new Padding(6);
            this.Name = "MainForm";
            this.DpiChanged += this.MainForm_DpiChanged;
            logSplitContainer.Panel1.ResumeLayout(false);
            logSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)logSplitContainer).EndInit();
            logSplitContainer.ResumeLayout(false);
            this.controlSplitContainer.Panel1.ResumeLayout(false);
            this.controlSplitContainer.Panel2.ResumeLayout(false);
            ((ISupportInitialize)this.controlSplitContainer).EndInit();
            this.controlSplitContainer.ResumeLayout(false);
            this.controlsFlowPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.dataSetTab.ResumeLayout(false);
            ((ISupportInitialize)this.dataGridView1).EndInit();
            this.parametersTab.ResumeLayout(false);
            ((ISupportInitialize)this.parametersDataGridView).EndInit();
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((ISupportInitialize)this.parametersBindingSource).EndInit();
            ((ISupportInitialize)this.dataSetBindingSource).EndInit();
            this.ResumeLayout(false);
        }
    }
}

﻿// -----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms
{
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>Main application form.</summary>
    public partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            Button processExcelDataSet;
            Button loadVisioObjects;
            SplitContainer logSplitContainer;
            SplitContainer controlSplitContainer;
            DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.controlsFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.layoutDataSet = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.dataSetTab = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.parametersTab = new System.Windows.Forms.TabPage();
            this.parametersDataGridView = new System.Windows.Forms.DataGridView();
            this.listBox = new System.Windows.Forms.ListBox();
            this.parametersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.updateVisioDrawing = new System.Windows.Forms.Button();
            processExcelDataSet = new System.Windows.Forms.Button();
            loadVisioObjects = new System.Windows.Forms.Button();
            logSplitContainer = new System.Windows.Forms.SplitContainer();
            controlSplitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(logSplitContainer)).BeginInit();
            logSplitContainer.Panel1.SuspendLayout();
            logSplitContainer.Panel2.SuspendLayout();
            logSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(controlSplitContainer)).BeginInit();
            controlSplitContainer.Panel1.SuspendLayout();
            controlSplitContainer.Panel2.SuspendLayout();
            controlSplitContainer.SuspendLayout();
            this.controlsFlowPanel.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.dataSetTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.parametersTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.parametersDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.parametersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetBindingSource)).BeginInit();
            this.SuspendLayout();

            // processExcelDataSet
            processExcelDataSet.Location = new System.Drawing.Point(12, 12);
            processExcelDataSet.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            processExcelDataSet.Name = "processExcelDataSet";
            processExcelDataSet.Size = new System.Drawing.Size(145, 23);
            processExcelDataSet.TabIndex = 1;
            processExcelDataSet.Text = "Process Excel DataSet";
            processExcelDataSet.UseVisualStyleBackColor = true;
            processExcelDataSet.Click += new System.EventHandler(this.ProcessExcelDataSet_Click);

            // loadVisioObjects
            loadVisioObjects.Location = new System.Drawing.Point(12, 47);
            loadVisioObjects.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            loadVisioObjects.Name = "loadVisioObjects";
            loadVisioObjects.Size = new System.Drawing.Size(145, 23);
            loadVisioObjects.TabIndex = 2;
            loadVisioObjects.Text = "Load Visio Objects";
            loadVisioObjects.UseVisualStyleBackColor = true;
            loadVisioObjects.Click += new System.EventHandler(this.LoadVisioObjects_Click);

            // logSplitContainer
            logSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            logSplitContainer.Location = new System.Drawing.Point(0, 0);
            logSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            logSplitContainer.Name = "logSplitContainer";
            logSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;

            // logSplitContainer.Panel1
            logSplitContainer.Panel1.Controls.Add(controlSplitContainer);

            // logSplitContainer.Panel2
            logSplitContainer.Panel2.Controls.Add(this.listBox);
            logSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(12);
            logSplitContainer.Size = new System.Drawing.Size(800, 450);
            logSplitContainer.SplitterDistance = 220;
            logSplitContainer.TabIndex = 5;

            // controlSplitContainer
            controlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            controlSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            controlSplitContainer.Location = new System.Drawing.Point(0, 0);
            controlSplitContainer.Margin = new System.Windows.Forms.Padding(0);
            controlSplitContainer.Name = "controlSplitContainer";

            // controlSplitContainer.Panel1
            controlSplitContainer.Panel1.Controls.Add(this.controlsFlowPanel);

            // controlSplitContainer.Panel2
            controlSplitContainer.Panel2.Controls.Add(this.tabControl1);
            controlSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(12);
            controlSplitContainer.Size = new System.Drawing.Size(800, 220);
            controlSplitContainer.SplitterDistance = 169;
            controlSplitContainer.TabIndex = 3;

            // controlsFlowPanel
            this.controlsFlowPanel.Controls.Add(processExcelDataSet);
            this.controlsFlowPanel.Controls.Add(loadVisioObjects);
            this.controlsFlowPanel.Controls.Add(this.layoutDataSet);
            this.controlsFlowPanel.Controls.Add(this.updateVisioDrawing);
            this.controlsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.controlsFlowPanel.Location = new System.Drawing.Point(0, 0);
            this.controlsFlowPanel.Margin = new System.Windows.Forms.Padding(0);
            this.controlsFlowPanel.Name = "controlsFlowPanel";
            this.controlsFlowPanel.Padding = new System.Windows.Forms.Padding(12);
            this.controlsFlowPanel.Size = new System.Drawing.Size(169, 220);
            this.controlsFlowPanel.TabIndex = 0;

            // layoutDataSet
            this.layoutDataSet.Location = new System.Drawing.Point(12, 82);
            this.layoutDataSet.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.layoutDataSet.Name = "layoutDataSet";
            this.layoutDataSet.Size = new System.Drawing.Size(145, 23);
            this.layoutDataSet.TabIndex = 3;
            this.layoutDataSet.Text = "Layout Data Set";
            this.layoutDataSet.UseVisualStyleBackColor = true;
            this.layoutDataSet.Click += new System.EventHandler(this.LayoutDataSet_Click);

            // tabControl1
            this.tabControl1.Controls.Add(this.dataSetTab);
            this.tabControl1.Controls.Add(this.parametersTab);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(603, 196);
            this.tabControl1.TabIndex = 0;

            // dataSetTab
            this.dataSetTab.Controls.Add(this.dataGridView1);
            this.dataSetTab.Location = new System.Drawing.Point(4, 24);
            this.dataSetTab.Name = "dataSetTab";
            this.dataSetTab.Padding = new System.Windows.Forms.Padding(3);
            this.dataSetTab.Size = new System.Drawing.Size(595, 168);
            this.dataSetTab.TabIndex = 1;
            this.dataSetTab.Text = "Current Data Set";
            this.dataSetTab.UseVisualStyleBackColor = true;

            // dataGridView1
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 25;
            this.dataGridView1.Size = new System.Drawing.Size(589, 162);
            this.dataGridView1.TabIndex = 0;

            // parametersTab
            this.parametersTab.Controls.Add(this.parametersDataGridView);
            this.parametersTab.Location = new System.Drawing.Point(4, 24);
            this.parametersTab.Name = "parametersTab";
            this.parametersTab.Padding = new System.Windows.Forms.Padding(3);
            this.parametersTab.Size = new System.Drawing.Size(595, 168);
            this.parametersTab.TabIndex = 0;
            this.parametersTab.Text = "Parameters";
            this.parametersTab.UseVisualStyleBackColor = true;

            // parametersDataGridView
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
            this.parametersDataGridView.Location = new System.Drawing.Point(3, 3);
            this.parametersDataGridView.Name = "parametersDataGridView";
            this.parametersDataGridView.RowTemplate.Height = 25;
            this.parametersDataGridView.Size = new System.Drawing.Size(589, 162);
            this.parametersDataGridView.TabIndex = 0;

            // listBox
            this.listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox.ItemHeight = 15;
            this.listBox.Location = new System.Drawing.Point(12, 12);
            this.listBox.Name = "listBox";
            this.listBox.ScrollAlwaysVisible = true;
            this.listBox.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listBox.Size = new System.Drawing.Size(776, 202);
            this.listBox.TabIndex = 1;

            // parametersBindingSource
            this.parametersBindingSource.AllowNew = false;

            // updateVisioDrawing
            this.updateVisioDrawing.Location = new System.Drawing.Point(12, 117);
            this.updateVisioDrawing.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.updateVisioDrawing.Name = "updateVisioDrawing";
            this.updateVisioDrawing.Size = new System.Drawing.Size(145, 23);
            this.updateVisioDrawing.TabIndex = 4;
            this.updateVisioDrawing.Text = "Update Visio Drawing";
            this.updateVisioDrawing.UseVisualStyleBackColor = true;
            this.updateVisioDrawing.Click += new System.EventHandler(this.UpdateVisioDrawing_Click);

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(logSplitContainer);
            this.Name = "MainForm";
            logSplitContainer.Panel1.ResumeLayout(false);
            logSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(logSplitContainer)).EndInit();
            logSplitContainer.ResumeLayout(false);
            controlSplitContainer.Panel1.ResumeLayout(false);
            controlSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(controlSplitContainer)).EndInit();
            controlSplitContainer.ResumeLayout(false);
            this.controlsFlowPanel.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.dataSetTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.parametersTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.parametersDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.parametersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataSetBindingSource)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
        private FlowLayoutPanel controlsFlowPanel;
        private TabControl tabControl1;
        private TabPage parametersTab;
        private TabPage dataSetTab;
        private DataGridView parametersDataGridView;
        private BindingSource parametersBindingSource;
        private ListBox listBox;
        private DataGridView dataGridView1;
        private BindingSource dataSetBindingSource;
        private Button layoutDataSet;
        private Button updateVisioDrawing;
    }
}
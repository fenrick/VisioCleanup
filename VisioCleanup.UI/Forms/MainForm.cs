// -----------------------------------------------------------------------
// <copyright file="MainForm.cs" company="Jolyon Suthers">
// Copyright (c) Jolyon Suthers. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// -----------------------------------------------------------------------

namespace VisioCleanup.UI.Forms
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using Serilog;

    using VisioCleanup.Core.Models.Config;

    /// <summary>Main application form.</summary>
    public class MainForm : Form
    {
        /// <summary>The app config.</summary>
        private readonly AppConfig appConfig;

        /// <summary>The logger.</summary>
        private readonly ILogger<MainForm> logger;
        private Button processExcelDataSet;
        private Button layoutVisioDiagram;
        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private RichTextBox simpleLogTextBox1;

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
            RichTextWinFormSink.AddRichTextBox(this.simpleLogTextBox1);
        }

        /// <summary>TODO The button 1_ click.</summary>
        /// <param name="sender">TODO The sender.</param>
        /// <param name="e">TODO The e.</param>
        private void ProcessExcelDataSet_Click(object sender, EventArgs e)
        {
            this.logger.LogDebug("woot!");
        }

        private void InitializeComponent()
        {
            this.simpleLogTextBox1 = new System.Windows.Forms.RichTextBox();
            this.processExcelDataSet = new System.Windows.Forms.Button();
            this.layoutVisioDiagram = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // simpleLogTextBox1
            // 
            this.simpleLogTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.simpleLogTextBox1.BackColor = System.Drawing.SystemColors.Desktop;
            this.simpleLogTextBox1.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.simpleLogTextBox1.Location = new System.Drawing.Point(13, 308);
            this.simpleLogTextBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.simpleLogTextBox1.Name = "simpleLogTextBox1";
            this.simpleLogTextBox1.Size = new System.Drawing.Size(870, 170);
            this.simpleLogTextBox1.TabIndex = 0;
            this.simpleLogTextBox1.Text = "";
            // 
            // processExcelDataSet
            // 
            this.processExcelDataSet.Location = new System.Drawing.Point(3, 3);
            this.processExcelDataSet.Name = "processExcelDataSet";
            this.processExcelDataSet.Size = new System.Drawing.Size(145, 23);
            this.processExcelDataSet.TabIndex = 1;
            this.processExcelDataSet.Text = "Process Excel DataSet";
            this.processExcelDataSet.UseVisualStyleBackColor = true;
            this.processExcelDataSet.Click += new System.EventHandler(this.ProcessExcelDataSet_Click);
            // 
            // layoutVisioDiagram
            // 
            this.layoutVisioDiagram.Location = new System.Drawing.Point(154, 3);
            this.layoutVisioDiagram.Name = "layoutVisioDiagram";
            this.layoutVisioDiagram.Size = new System.Drawing.Size(145, 23);
            this.layoutVisioDiagram.TabIndex = 2;
            this.layoutVisioDiagram.Text = "Layout Visio Diagram";
            this.layoutVisioDiagram.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.processExcelDataSet);
            this.flowLayoutPanel1.Controls.Add(this.layoutVisioDiagram);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(13, 12);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(715, 233);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.simpleLogTextBox1);
            this.panel1.Controls.Add(this.flowLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(896, 490);
            this.panel1.TabIndex = 4;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(896, 490);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
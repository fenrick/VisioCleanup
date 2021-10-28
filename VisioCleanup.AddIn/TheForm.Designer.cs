namespace VisioCleanup.AddIn;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

partial class TheForm
{
    /// <summary>Required designer variable.</summary>
    private readonly IContainer components = null;

    private Button button1;

    private Label label1;

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
        this.label1 = new Label();
        this.button1 = new Button();
        this.SuspendLayout();

        // label1
        this.label1.AutoSize = true;
        this.label1.Location = new Point(61, 58);
        this.label1.Name = "label1";
        this.label1.Size = new Size(93, 13);
        this.label1.TabIndex = 0;
        this.label1.Text = "Put controls here !";

        // button1
        this.button1.Location = new Point(64, 117);
        this.button1.Name = "button1";
        this.button1.Size = new Size(175, 23);
        this.button1.TabIndex = 1;
        this.button1.Text = "Show document name";
        this.button1.UseVisualStyleBackColor = true;
        this.button1.Click += new EventHandler(this.button1_Click);

        // TheForm
        this.AutoScaleDimensions = new SizeF(6F, 13F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ClientSize = new Size(321, 250);
        this.ControlBox = false;
        this.Controls.Add(this.button1);
        this.Controls.Add(this.label1);
        this.FormBorderStyle = FormBorderStyle.None;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "TheForm";
        this.ShowIcon = false;
        this.ShowInTaskbar = false;
        this.Text = "Panel";
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
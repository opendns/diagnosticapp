namespace OpenDnsDiagnostic
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonRunTests = new System.Windows.Forms.Button();
            this.labelDomain = new System.Windows.Forms.Label();
            this.textBoxDomain = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.labelDomainExample = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(514, 129);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 4;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonRunTests
            // 
            this.buttonRunTests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunTests.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRunTests.Location = new System.Drawing.Point(501, 10);
            this.buttonRunTests.Name = "buttonRunTests";
            this.buttonRunTests.Size = new System.Drawing.Size(90, 46);
            this.buttonRunTests.TabIndex = 3;
            this.buttonRunTests.Text = "Run tests";
            this.buttonRunTests.UseVisualStyleBackColor = true;
            this.buttonRunTests.Click += new System.EventHandler(this.buttonRunTests_Click);
            // 
            // labelDomain
            // 
            this.labelDomain.AutoSize = true;
            this.labelDomain.Location = new System.Drawing.Point(35, 39);
            this.labelDomain.Name = "labelDomain";
            this.labelDomain.Size = new System.Drawing.Size(124, 13);
            this.labelDomain.TabIndex = 2;
            this.labelDomain.Text = "Domain to test (optional):";
            // 
            // textBoxDomain
            // 
            this.textBoxDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDomain.Location = new System.Drawing.Point(165, 36);
            this.textBoxDomain.Name = "textBoxDomain";
            this.textBoxDomain.Size = new System.Drawing.Size(328, 20);
            this.textBoxDomain.TabIndex = 2;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(12, 13);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(147, 13);
            this.labelUserName.TabIndex = 4;
            this.labelUserName.Text = "OpenDNS account (optional):";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(165, 10);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(328, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // labelDomainExample
            // 
            this.labelDomainExample.AutoSize = true;
            this.labelDomainExample.ForeColor = System.Drawing.Color.Gray;
            this.labelDomainExample.Location = new System.Drawing.Point(162, 59);
            this.labelDomainExample.Name = "labelDomainExample";
            this.labelDomainExample.Size = new System.Drawing.Size(116, 13);
            this.labelDomainExample.TabIndex = 6;
            this.labelDomainExample.Text = "(e.g. www.google.com)";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(601, 164);
            this.Controls.Add(this.labelDomainExample);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.labelUserName);
            this.Controls.Add(this.textBoxDomain);
            this.Controls.Add(this.labelDomain);
            this.Controls.Add(this.buttonRunTests);
            this.Controls.Add(this.buttonExit);
            this.Name = "Form1";
            this.Text = "OpenDNS diagnostic tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonRunTests;
        private System.Windows.Forms.Label labelDomain;
        private System.Windows.Forms.TextBox textBoxDomain;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelDomainExample;
    }
}


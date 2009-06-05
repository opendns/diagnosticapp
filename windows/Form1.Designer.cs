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
            this.labelAccount = new System.Windows.Forms.Label();
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.labelDomainExample = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTicket = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonExit
            // 
            this.buttonExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExit.Location = new System.Drawing.Point(507, 145);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(75, 23);
            this.buttonExit.TabIndex = 5;
            this.buttonExit.Text = "Exit";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonRunTests
            // 
            this.buttonRunTests.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRunTests.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRunTests.Location = new System.Drawing.Point(492, 10);
            this.buttonRunTests.Name = "buttonRunTests";
            this.buttonRunTests.Size = new System.Drawing.Size(90, 72);
            this.buttonRunTests.TabIndex = 4;
            this.buttonRunTests.Text = "Run tests";
            this.buttonRunTests.UseVisualStyleBackColor = true;
            this.buttonRunTests.Click += new System.EventHandler(this.buttonRunTests_Click);
            // 
            // labelDomain
            // 
            this.labelDomain.AutoSize = true;
            this.labelDomain.Location = new System.Drawing.Point(35, 65);
            this.labelDomain.Name = "labelDomain";
            this.labelDomain.Size = new System.Drawing.Size(124, 13);
            this.labelDomain.TabIndex = 2;
            this.labelDomain.Text = "Domain to test (optional):";
            // 
            // textBoxDomain
            // 
            this.textBoxDomain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDomain.Location = new System.Drawing.Point(165, 62);
            this.textBoxDomain.Name = "textBoxDomain";
            this.textBoxDomain.Size = new System.Drawing.Size(321, 20);
            this.textBoxDomain.TabIndex = 3;
            // 
            // labelAccount
            // 
            this.labelAccount.AutoSize = true;
            this.labelAccount.Location = new System.Drawing.Point(12, 13);
            this.labelAccount.Name = "labelAccount";
            this.labelAccount.Size = new System.Drawing.Size(147, 13);
            this.labelAccount.TabIndex = 4;
            this.labelAccount.Text = "OpenDNS account (optional):";
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUserName.Location = new System.Drawing.Point(165, 10);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.Size = new System.Drawing.Size(321, 20);
            this.textBoxUserName.TabIndex = 1;
            // 
            // labelDomainExample
            // 
            this.labelDomainExample.AutoSize = true;
            this.labelDomainExample.ForeColor = System.Drawing.Color.Gray;
            this.labelDomainExample.Location = new System.Drawing.Point(162, 85);
            this.labelDomainExample.Name = "labelDomainExample";
            this.labelDomainExample.Size = new System.Drawing.Size(116, 13);
            this.labelDomainExample.TabIndex = 6;
            this.labelDomainExample.Text = "(e.g. www.google.com)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Ticket # (optional):";
            // 
            // textBoxTicket
            // 
            this.textBoxTicket.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTicket.Location = new System.Drawing.Point(165, 36);
            this.textBoxTicket.Name = "textBoxTicket";
            this.textBoxTicket.Size = new System.Drawing.Size(321, 20);
            this.textBoxTicket.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(594, 180);
            this.Controls.Add(this.textBoxTicket);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelDomainExample);
            this.Controls.Add(this.textBoxUserName);
            this.Controls.Add(this.labelAccount);
            this.Controls.Add(this.textBoxDomain);
            this.Controls.Add(this.labelDomain);
            this.Controls.Add(this.buttonRunTests);
            this.Controls.Add(this.buttonExit);
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(547, 209);
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
        private System.Windows.Forms.Label labelAccount;
        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.Label labelDomainExample;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTicket;
    }
}


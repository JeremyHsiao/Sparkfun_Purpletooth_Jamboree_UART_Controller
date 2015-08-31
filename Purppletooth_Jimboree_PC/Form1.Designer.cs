namespace Purppletooth_Jimboree_PC
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
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnFreshCOMNo = new System.Windows.Forms.Button();
            this.btnConnectionControl = new System.Windows.Forms.Button();
            this.txtStringToSerial = new System.Windows.Forms.TextBox();
            this.btnSendStringToUART = new System.Windows.Forms.Button();
            this.lstBaudRate = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(76, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(69, 40);
            this.listBox1.TabIndex = 0;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.richTextBox1.Location = new System.Drawing.Point(12, 64);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(470, 327);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // btnFreshCOMNo
            // 
            this.btnFreshCOMNo.Location = new System.Drawing.Point(12, 12);
            this.btnFreshCOMNo.Name = "btnFreshCOMNo";
            this.btnFreshCOMNo.Size = new System.Drawing.Size(58, 40);
            this.btnFreshCOMNo.TabIndex = 2;
            this.btnFreshCOMNo.Text = "Refresh COM";
            this.btnFreshCOMNo.UseVisualStyleBackColor = true;
            this.btnFreshCOMNo.Click += new System.EventHandler(this.RefreshCOMPortNumber_Click);
            // 
            // btnConnectionControl
            // 
            this.btnConnectionControl.Location = new System.Drawing.Point(151, 12);
            this.btnConnectionControl.Name = "btnConnectionControl";
            this.btnConnectionControl.Size = new System.Drawing.Size(67, 40);
            this.btnConnectionControl.TabIndex = 3;
            this.btnConnectionControl.Text = "Connect";
            this.btnConnectionControl.UseVisualStyleBackColor = true;
            this.btnConnectionControl.Click += new System.EventHandler(this.ConnectionButton_Click);
            // 
            // txtStringToSerial
            // 
            this.txtStringToSerial.BackColor = System.Drawing.SystemColors.Window;
            this.txtStringToSerial.Location = new System.Drawing.Point(12, 403);
            this.txtStringToSerial.Name = "txtStringToSerial";
            this.txtStringToSerial.Size = new System.Drawing.Size(399, 22);
            this.txtStringToSerial.TabIndex = 4;
            this.txtStringToSerial.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtStringToSerial_KeyDown);
            // 
            // btnSendStringToUART
            // 
            this.btnSendStringToUART.Location = new System.Drawing.Point(417, 403);
            this.btnSendStringToUART.Name = "btnSendStringToUART";
            this.btnSendStringToUART.Size = new System.Drawing.Size(65, 22);
            this.btnSendStringToUART.TabIndex = 5;
            this.btnSendStringToUART.Text = "Send";
            this.btnSendStringToUART.UseVisualStyleBackColor = true;
            this.btnSendStringToUART.Click += new System.EventHandler(this.btnSendStringToUART_Click);
            // 
            // lstBaudRate
            // 
            this.lstBaudRate.DisplayMember = "int";
            this.lstBaudRate.FormattingEnabled = true;
            this.lstBaudRate.ItemHeight = 12;
            this.lstBaudRate.Items.AddRange(new object[] {
            "9600",
            "115200"});
            this.lstBaudRate.Location = new System.Drawing.Point(224, 12);
            this.lstBaudRate.Name = "lstBaudRate";
            this.lstBaudRate.Size = new System.Drawing.Size(69, 40);
            this.lstBaudRate.TabIndex = 6;
            this.lstBaudRate.ValueMember = "int";
            this.lstBaudRate.SelectedIndexChanged += new System.EventHandler(this.lstBaudRate_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 433);
            this.Controls.Add(this.lstBaudRate);
            this.Controls.Add(this.btnSendStringToUART);
            this.Controls.Add(this.txtStringToSerial);
            this.Controls.Add(this.btnConnectionControl);
            this.Controls.Add(this.btnFreshCOMNo);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.listBox1);
            this.Name = "Form1";
            this.Text = "PurpleTooth Jamboree";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button btnFreshCOMNo;
        private System.Windows.Forms.Button btnConnectionControl;
        private System.Windows.Forms.TextBox txtStringToSerial;
        private System.Windows.Forms.Button btnSendStringToUART;
        private System.Windows.Forms.ListBox lstBaudRate;
    }
}


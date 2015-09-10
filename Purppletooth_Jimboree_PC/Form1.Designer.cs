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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.btnFreshCOMNo = new System.Windows.Forms.Button();
            this.btnConnectionControl = new System.Windows.Forms.Button();
            this.txtStringToSerial = new System.Windows.Forms.TextBox();
            this.btnSendStringToUART = new System.Windows.Forms.Button();
            this.lstBaudRate = new System.Windows.Forms.ListBox();
            this.btnGetConfig = new System.Windows.Forms.Button();
            this.dgvProfileView = new System.Windows.Forms.DataGridView();
            this.Profile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Connected = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfileView)).BeginInit();
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
            this.lstBaudRate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lstBaudRate.ItemHeight = 12;
            this.lstBaudRate.Items.AddRange(new object[] {
            "9600",
            "115200"});
            this.lstBaudRate.Location = new System.Drawing.Point(224, 12);
            this.lstBaudRate.Name = "lstBaudRate";
            this.lstBaudRate.Size = new System.Drawing.Size(43, 40);
            this.lstBaudRate.TabIndex = 6;
            this.lstBaudRate.ValueMember = "int";
            this.lstBaudRate.SelectedIndexChanged += new System.EventHandler(this.lstBaudRate_SelectedIndexChanged);
            // 
            // btnGetConfig
            // 
            this.btnGetConfig.Location = new System.Drawing.Point(273, 12);
            this.btnGetConfig.Name = "btnGetConfig";
            this.btnGetConfig.Size = new System.Drawing.Size(48, 40);
            this.btnGetConfig.TabIndex = 7;
            this.btnGetConfig.Text = "Get Config";
            this.btnGetConfig.UseVisualStyleBackColor = true;
            this.btnGetConfig.Click += new System.EventHandler(this.btnGetConfig_click);
            // 
            // dgvProfileView
            // 
            this.dgvProfileView.AllowUserToAddRows = false;
            this.dgvProfileView.AllowUserToDeleteRows = false;
            this.dgvProfileView.AllowUserToResizeColumns = false;
            this.dgvProfileView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("新細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvProfileView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvProfileView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProfileView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Profile,
            this.Connected});
            this.dgvProfileView.Location = new System.Drawing.Point(498, 64);
            this.dgvProfileView.MultiSelect = false;
            this.dgvProfileView.Name = "dgvProfileView";
            this.dgvProfileView.ReadOnly = true;
            this.dgvProfileView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvProfileView.RowTemplate.Height = 24;
            this.dgvProfileView.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.dgvProfileView.Size = new System.Drawing.Size(242, 166);
            this.dgvProfileView.TabIndex = 9;
            this.dgvProfileView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // Profile
            // 
            this.Profile.Frozen = true;
            this.Profile.HeaderText = "Profile";
            this.Profile.Name = "Profile";
            this.Profile.ReadOnly = true;
            // 
            // Connected
            // 
            this.Connected.Frozen = true;
            this.Connected.HeaderText = "Connected";
            this.Connected.Name = "Connected";
            this.Connected.ReadOnly = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 584);
            this.Controls.Add(this.dgvProfileView);
            this.Controls.Add(this.btnGetConfig);
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
            ((System.ComponentModel.ISupportInitialize)(this.dgvProfileView)).EndInit();
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
        private System.Windows.Forms.Button btnGetConfig;
        private System.Windows.Forms.DataGridView dgvProfileView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Profile;
        private System.Windows.Forms.DataGridViewTextBoxColumn Connected;
    }
}


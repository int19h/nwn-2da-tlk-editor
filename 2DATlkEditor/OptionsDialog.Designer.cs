namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    partial class OptionsDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.browseTwoDAPathButton = new System.Windows.Forms.Button();
            this.browseTlkPathButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.autoLoad2DAsCheckBox = new System.Windows.Forms.CheckBox();
            this.twoDAPathTextBox = new System.Windows.Forms.TextBox();
            this.tlkPathTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.changeGridFontButton = new System.Windows.Forms.Button();
            this.gridFontTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gridFontDialog = new System.Windows.Forms.FontDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Folder with standard 2DAs:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Folder with standard TLKs:";
            // 
            // browseTwoDAPathButton
            // 
            this.browseTwoDAPathButton.Location = new System.Drawing.Point(410, 39);
            this.browseTwoDAPathButton.Name = "browseTwoDAPathButton";
            this.browseTwoDAPathButton.Size = new System.Drawing.Size(75, 23);
            this.browseTwoDAPathButton.TabIndex = 5;
            this.browseTwoDAPathButton.Text = "Browse";
            this.browseTwoDAPathButton.UseVisualStyleBackColor = true;
            this.browseTwoDAPathButton.Click += new System.EventHandler(this.browseTwoDAPathButton_Click);
            // 
            // browseTlkPathButton
            // 
            this.browseTlkPathButton.Location = new System.Drawing.Point(410, 74);
            this.browseTlkPathButton.Name = "browseTlkPathButton";
            this.browseTlkPathButton.Size = new System.Drawing.Size(75, 23);
            this.browseTlkPathButton.TabIndex = 6;
            this.browseTlkPathButton.Text = "Browse";
            this.browseTlkPathButton.UseVisualStyleBackColor = true;
            this.browseTlkPathButton.Click += new System.EventHandler(this.browseTlkPathButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(431, 185);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(350, 185);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.autoLoad2DAsCheckBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.twoDAPathTextBox);
            this.groupBox1.Controls.Add(this.browseTlkPathButton);
            this.groupBox1.Controls.Add(this.tlkPathTextBox);
            this.groupBox1.Controls.Add(this.browseTwoDAPathButton);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(6, 3, 6, 6);
            this.groupBox1.Size = new System.Drawing.Size(494, 110);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Files";
            // 
            // autoLoad2DAsCheckBox
            // 
            this.autoLoad2DAsCheckBox.AutoSize = true;
            this.autoLoad2DAsCheckBox.Checked = global::Int19h.NeverwinterNights.TwoDATlkEditor.Properties.Settings.Default.AutoLoad2DA;
            this.autoLoad2DAsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoLoad2DAsCheckBox.Location = new System.Drawing.Point(12, 19);
            this.autoLoad2DAsCheckBox.Name = "autoLoad2DAsCheckBox";
            this.autoLoad2DAsCheckBox.Size = new System.Drawing.Size(214, 17);
            this.autoLoad2DAsCheckBox.TabIndex = 0;
            this.autoLoad2DAsCheckBox.Text = "Automatically open referenced 2DA files";
            this.autoLoad2DAsCheckBox.UseVisualStyleBackColor = true;
            // 
            // twoDAPathTextBox
            // 
            this.twoDAPathTextBox.Location = new System.Drawing.Point(149, 42);
            this.twoDAPathTextBox.Name = "twoDAPathTextBox";
            this.twoDAPathTextBox.Size = new System.Drawing.Size(255, 20);
            this.twoDAPathTextBox.TabIndex = 2;
            this.twoDAPathTextBox.Text = global::Int19h.NeverwinterNights.TwoDATlkEditor.Properties.Settings.Default.TwoDAPath;
            // 
            // tlkPathTextBox
            // 
            this.tlkPathTextBox.Location = new System.Drawing.Point(148, 76);
            this.tlkPathTextBox.Name = "tlkPathTextBox";
            this.tlkPathTextBox.Size = new System.Drawing.Size(256, 20);
            this.tlkPathTextBox.TabIndex = 3;
            this.tlkPathTextBox.Text = global::Int19h.NeverwinterNights.TwoDATlkEditor.Properties.Settings.Default.TlkPath;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.changeGridFontButton);
            this.groupBox2.Controls.Add(this.gridFontTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Location = new System.Drawing.Point(12, 128);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(494, 51);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Appearance";
            // 
            // changeGridFontButton
            // 
            this.changeGridFontButton.Location = new System.Drawing.Point(410, 17);
            this.changeGridFontButton.Name = "changeGridFontButton";
            this.changeGridFontButton.Size = new System.Drawing.Size(75, 23);
            this.changeGridFontButton.TabIndex = 7;
            this.changeGridFontButton.Text = "Change";
            this.changeGridFontButton.UseVisualStyleBackColor = true;
            this.changeGridFontButton.Click += new System.EventHandler(this.changeGridFontButton_Click);
            // 
            // gridFontTextBox
            // 
            this.gridFontTextBox.Location = new System.Drawing.Point(65, 19);
            this.gridFontTextBox.Name = "gridFontTextBox";
            this.gridFontTextBox.ReadOnly = true;
            this.gridFontTextBox.Size = new System.Drawing.Size(339, 20);
            this.gridFontTextBox.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Grid font:";
            // 
            // OptionsDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(519, 217);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDialog";
            this.Text = "2DA/TLK Editor Options";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox autoLoad2DAsCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox twoDAPathTextBox;
        private System.Windows.Forms.TextBox tlkPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button browseTwoDAPathButton;
        private System.Windows.Forms.Button browseTlkPathButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button changeGridFontButton;
        private System.Windows.Forms.TextBox gridFontTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FontDialog gridFontDialog;
    }
}
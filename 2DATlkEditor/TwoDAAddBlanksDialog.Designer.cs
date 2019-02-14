namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    partial class TwoDAAddBlanksDialog
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
            this.countNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.indexNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.countNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.indexNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of new blank rows to add:";
            // 
            // countNumericUpDown
            // 
            this.countNumericUpDown.Location = new System.Drawing.Point(187, 12);
            this.countNumericUpDown.Name = "countNumericUpDown";
            this.countNumericUpDown.Size = new System.Drawing.Size(97, 20);
            this.countNumericUpDown.TabIndex = 1;
            this.countNumericUpDown.ValueChanged += new System.EventHandler(this.countNumericUpDown_ValueChanged);
            // 
            // indexNumericUpDown
            // 
            this.indexNumericUpDown.Location = new System.Drawing.Point(187, 38);
            this.indexNumericUpDown.Name = "indexNumericUpDown";
            this.indexNumericUpDown.Size = new System.Drawing.Size(97, 20);
            this.indexNumericUpDown.TabIndex = 2;
            this.indexNumericUpDown.ValueChanged += new System.EventHandler(this.indexNumericUpDown_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Row index to pad up to:";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(209, 64);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(128, 64);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // TwoDAAddBlanksDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(301, 96);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.indexNumericUpDown);
            this.Controls.Add(this.countNumericUpDown);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TwoDAAddBlanksDialog";
            this.Text = "Add Blank Rows";
            ((System.ComponentModel.ISupportInitialize)(this.countNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.indexNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown countNumericUpDown;
        private System.Windows.Forms.NumericUpDown indexNumericUpDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Int19h.NeverwinterNights.TwoDATlkEditor.Properties;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public partial class OptionsDialog : Form
    {
        private static readonly TypeConverter fontConverter =
            TypeDescriptor.GetConverter(typeof(Font));

        public OptionsDialog()
        {
            InitializeComponent();

            twoDAPathTextBox.Text = Settings.Default.TwoDAPath;
            tlkPathTextBox.Text = Settings.Default.TlkPath;
            autoLoad2DAsCheckBox.Checked = Settings.Default.AutoLoad2DA;
            gridFontDialog.Font = Settings.Default.GridFont;
            gridFontTextBox.Text = fontConverter.ConvertToString(gridFontDialog.Font);
        }

        private void browseTwoDAPathButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(twoDAPathTextBox.Text))
            {
                folderBrowserDialog.SelectedPath = twoDAPathTextBox.Text;
            }

            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                twoDAPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void browseTlkPathButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(tlkPathTextBox.Text))
            {
                folderBrowserDialog.SelectedPath = tlkPathTextBox.Text;
            }

            if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK)
            {
                tlkPathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void changeGridFontButton_Click(object sender, EventArgs e)
        {
            if (gridFontDialog.ShowDialog(this) == DialogResult.OK)
            {
                gridFontTextBox.Text = fontConverter.ConvertToString(gridFontDialog.Font);
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Settings.Default.AutoLoad2DA = autoLoad2DAsCheckBox.Checked;
            Settings.Default.TwoDAPath = twoDAPathTextBox.Text;
            Settings.Default.TlkPath = tlkPathTextBox.Text;
            Settings.Default.GridFont = gridFontDialog.Font;
            Settings.Default.Save();
        }
    }
}

/*
 * Neverwinter Nights 2DA/TLK Editor.
 * Copyright (C) 2008 Pavel Minaev
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public partial class TwoDADocumentWindow : DocumentWindow
    {
        private TwoDADocument document;
        private DataView dataView;

        public TwoDADocumentWindow()
        {
            InitializeComponent();

            filterErrorProvider.SetIconPadding(filterToolStripTextBox.Control, -18);
        }

        public TwoDADocument Document
        {
            get
            {
                return document;
            }
            set
            {
                if (document != null)
                {
                    document.PropertyChanged -= document_PropertyChanged;
                    document.Data.ColumnChanged -= Data_ColumnChanged;
                }

                document = value;

                if (document != null)
                {
                    dataGridView.AutoGenerateColumns = true;
                    dataView = new DataView(document.Data);
                    dataView.RowStateFilter |= DataViewRowState.Deleted;
                    bindingSource.DataSource = dataView;
                }
                else
                {
                    bindingSource.DataSource = null;
                }

                dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);

                if (document != null)
                {
                    document.PropertyChanged += document_PropertyChanged;
                    document.Data.ColumnChanged += Data_ColumnChanged;
                }

                EnableMenuItems();

                OnPropertyChanged(new PropertyChangedEventArgs("Document"));
            }
        }

        private void Save()
        {
            var tmp = new StringWriter();
            document.Save(tmp);
            File.WriteAllText(FileName, tmp.ToString());
        }

        private void EnableMenuItems()
        {
            if (document != null)
            {
                menuStrip.Enabled = true;
                saveToolStripMenuItem.Enabled = document.IsModified;
                saveAsToolStripMenuItem.Enabled = document.IsModified;
                undoToolStripMenuItem.Enabled = document.CanUndo;
                redoToolStripMenuItem.Enabled = document.CanRedo;
            }
            else
            {
                menuStrip.Enabled = false;
            }
        }

        private bool ConfirmRowDelete(DataRow row)
        {
            var text =
                "WARNING! Deleting this row will result in renumbering " +
                "of all rows below it. This is a dangerous operation for " +
                "most 2DAs! Consider blanking out the row instead.\r\n\r\n" +
                "Are you sure you really want to delete the row?";
            var dr = MessageBox.Show(this, text, "Confirm Row Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return dr == DialogResult.Yes;
        }

        private void document_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case null:
                case "IsModified":
                case "CanUndo":
                case "CanRedo":
                    EnableMenuItems();
                    break;
            }
        }

        private void Data_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            propertyGridRefreshTimer.Start();
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var rowView = (DataRowView)e.Row.DataBoundItem;
            var row = rowView.Row;
            e.Cancel = !ConfirmRowDelete(row);
        }

        private void bindingSource_CurrentChanged(object sender, EventArgs e)
        {
            //DataRowView rowView = (DataRowView)bindingSource.Current;
            //if (rowView != null)
            //{
            //    DataRow row = rowView.Row;
            //    propertyGrid.SelectedObject = new TwoDARowTypeDescriptor(row);
            //}
            //else
            //{
            //    propertyGrid.SelectedObject = null;
            //}
        }

        private void dataGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            if (e.Column.Index == 0)
            {
                e.Column.ReadOnly = true;
            }

            if (e.Column.Index < 2)
            {
                e.Column.Frozen = true;

                Color original = dataGridView.DefaultCellStyle.BackColor;
                e.Column.DefaultCellStyle.BackColor = Color.FromArgb(
                    (byte)(original.R * 0.9m),
                    (byte)(original.G * 0.95m),
                    original.B);
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var schemaColumn = document.SchemaColumns[e.ColumnIndex];

            if (e.Value is DBNull)
            {
                e.CellStyle.ForeColor = SystemColors.GrayText;
            }

            if (schemaColumn != null)
            {
                if (schemaColumn.DataType == TwoDASchema.DataType.HexInteger && e.Value is int)
                {
                    e.Value = "0x" + ((int)e.Value).ToString("X" + schemaColumn.Digits, CultureInfo.InvariantCulture);
                }
            }

            //var rowView = (DataRowView)dataGridView.Rows[e.RowIndex].DataBoundItem;
            //if (rowView == null)
            //{
            //    return;
            //}

            //var row = rowView.Row;
            //switch (row.RowState)
            //{
            //    case DataRowState.Added:
            //    case DataRowState.Detached:
            //        e.CellStyle.BackColor = Color.Yellow;
            //        break;

            //    case DataRowState.Deleted:
            //        e.CellStyle.BackColor = Color.Yellow;
            //        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Strikeout);
            //        break;

            //    default:
            //        object oldValue = row[e.ColumnIndex, DataRowVersion.Original];
            //        object newValue = row[
            //            e.ColumnIndex,
            //            row.HasVersion(DataRowVersion.Proposed) ? DataRowVersion.Proposed : DataRowVersion.Current];
            //        if (!object.Equals(oldValue, newValue))
            //        {
            //            e.CellStyle.BackColor = Color.Yellow;
            //        }
            //        break;
            //}
        }

        private void dataGridView_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var schemaColumn = document.SchemaColumns[e.ColumnIndex];

            if (schemaColumn != null)
            {
                if (schemaColumn.DataType == TwoDASchema.DataType.HexInteger && e.Value is string)
                {
                    string s = (string)e.Value;
                    int value;

                    if (s.StartsWith("0x", StringComparison.Ordinal))
                    {
                        e.ParsingApplied = int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
                    }
                    else
                    {
                        e.ParsingApplied = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
                    }

                    if (e.ParsingApplied)
                    {
                        e.Value = value;
                    }
                }
            }
        }

        private void dataGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            dataGridView[e.ColumnIndex, e.RowIndex].ErrorText = e.Exception.Message;
        }

        private void dataGridView_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView[e.ColumnIndex, e.RowIndex].ErrorText = null;
        }

        private void filterToolStripTextBox_Leave(object sender, EventArgs e)
        {
            try
            {
                var filter = filterToolStripTextBox.Text;
                dataView.RowFilter = filter;
                filterToolStripTextBox.AutoCompleteCustomSource.Add(filter);
                filterErrorProvider.SetError(filterToolStripTextBox.Control, null);
            }
            catch (Exception ex)
            {
                filterErrorProvider.SetError(filterToolStripTextBox.Control, ex.Message);
            }
        }

        private void filterToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                dataGridView.Focus();
            }
        }

        private void propertyGridRefreshTimer_Tick(object sender, EventArgs e)
        {
            propertyGridRefreshTimer.Stop();
            propertyGrid.Refresh();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save2DAFileDialog.FileName = FileName;
            if (save2DAFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileName = save2DAFileDialog.FileName;
                Save();
            }
        }

        private void blankOutToolStripButton_Click(object sender, EventArgs e)
        {
            var selectedGridRows = (from DataGridViewCell cell in dataGridView.SelectedCells
                                    select dataGridView.Rows[cell.RowIndex]).Distinct();

            foreach (DataGridViewRow gridRow in selectedGridRows)
            {
                var rowView = (DataRowView)gridRow.DataBoundItem;
                var row = rowView.Row;

                if (row.RowState == DataRowState.Deleted)
                {
                    return;
                }

                foreach (DataColumn column in document.Data.Columns)
                {
                    if (column.Ordinal > 0 && column.AllowDBNull)
                    {
                        row[column] = DBNull.Value;
                    }
                }
            }
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            var selectedGridRows = (from DataGridViewCell cell in dataGridView.SelectedCells
                                    select dataGridView.Rows[cell.RowIndex]).Distinct();

            foreach (DataGridViewRow gridRow in selectedGridRows)
            {
                var rowView = (DataRowView)gridRow.DataBoundItem;
                var row = rowView.Row;

                if (row.RowState == DataRowState.Deleted)
                {
                    return;
                }

                if (ConfirmRowDelete(row))
                {
                    row.Delete();
                }
            }
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (document.CanUndo)
            {
                document.Undo();
            }
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (document.CanRedo)
            {
                document.Redo();
            }
        }

        private void DocumentWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (document.IsModified)
            {
                string text =
                    "The following document contains unsaved changes:\r\n\r\n" +
                    FileName + "\r\n\r\n" +
                    "Do you want to save changes to this document?";

                var dr = MessageBox.Show(
                    this,
                    text,
                    "Confirm Save",
                    MessageBoxButtons.YesNoCancel);

                switch (dr)
                {
                    case DialogResult.Yes:
                        Save();
                        break;

                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void addBlanksToolStripButton_Click(object sender, EventArgs e)
        {
            var addDlg = new TwoDAAddBlanksDialog();
            addDlg.ExistingRowCount = document.Data.Rows.Count;
            if (addDlg.ShowDialog(this) == DialogResult.OK)
            {
                document.BeginChangeSet();

                for (int i = 0; i < addDlg.NewRowCount; ++i)
                {
                    var row = document.Data.NewRow();
                    document.Data.Rows.Add(row);
                }

                document.EndChangeSet();
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            var selectedRows = (from DataGridViewCell cell in dataGridView.SelectedCells
                                let rowView = (DataRowView)dataGridView.Rows[cell.RowIndex].DataBoundItem
                                where rowView != null
                                select rowView.Row
                               ).Distinct();
            propertyGrid.SelectedObjects = selectedRows.Select(row => new TwoDARowTypeDescriptor(row)).ToArray();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 1)
            {
                Clipboard.SetText(Convert.ToString(dataGridView.SelectedCells[0].Value));
                return;
            }

            var cells = dataGridView.SelectedCells.Cast<DataGridViewCell>();
            int xmin = cells.Min(cell => cell.ColumnIndex);
            int xmax = cells.Max(cell => cell.ColumnIndex);
            int ymin = cells.Min(cell => cell.RowIndex);
            int ymax = cells.Max(cell => cell.RowIndex);
            int w = xmax - xmin + 1, h = ymax - ymin + 1;

            if (w * h != dataGridView.SelectedCells.Count)
            {
                MessageBox.Show(
                    this,
                    "Cannot copy a non-rectangular cell selection to clipboard.",
                    null,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var data = new object[h, w];
            for (int y = ymin; y <= ymax; ++y)
            {
                for (int x = xmin; x <= xmax; ++x)
                {
                    data[y - ymin, x - xmin] = dataGridView[x, y].Value;
                }
            }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                CSV.Write(data, writer);
                writer.Flush();

                stream.Position = 0;
                Clipboard.SetDataObject(new DataObject("Csv", stream), true);
            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedCells.Count == 1 && Clipboard.ContainsText())
            {
                dataGridView.SelectedCells[0].Value = Clipboard.GetText();
                return;
            }

            if (!Clipboard.ContainsData("Csv"))
            {
                return;
            }

            var cells = dataGridView.SelectedCells.Cast<DataGridViewCell>();
            int xmin = cells.Min(cell => cell.ColumnIndex);
            int xmax = cells.Max(cell => cell.ColumnIndex);
            int ymin = cells.Min(cell => cell.RowIndex);
            int ymax = cells.Max(cell => cell.RowIndex);
            int w = xmax - xmin + 1, h = ymax - ymin + 1;

            if (w * h != dataGridView.SelectedCells.Count)
            {
                MessageBox.Show(
                    this,
                    "Cannot paste from clipboard into a non-rectangular cell selection.",
                    null,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            object[,] data;

            object rawData = Clipboard.GetData("Csv");
            using (var stream = rawData as Stream)
            using (var reader = (stream == null) ? (TextReader)new StringReader(rawData.ToString()) : new StreamReader(stream))
            {
                data = CSV.Parse(reader);
            }

            if (data.GetLength(0) != h || data.GetLength(1) != w)
            {
                MessageBox.Show(
                    this,
                    string.Format(
                        "Cannot paste block of cells of size {0}x{1} from clipboard into cell selection of size {2}x{3}.",
                        data.GetLength(0), data.GetLength(1),
                        w, h),
                    null,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            int i = 0, j = 0;
            try
            {
                for (i = 0; i < h; ++i)
                {
                    for (j = 0; j < w; ++j)
                    {
                        Document.Data.Rows[ymin + i][xmin + j] = data[i, j];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    string.Format("Error while pasting cell at row {0}, column {1}: {2}", i, j, ex.Message),
                    null,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}

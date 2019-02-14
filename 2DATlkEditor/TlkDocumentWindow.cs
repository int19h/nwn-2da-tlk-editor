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
using Int19h.NeverwinterNights.Tlk;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public partial class TlkDocumentWindow : DocumentWindow
    {
        private TlkDocument document;

        public TlkDocumentWindow()
        {
            InitializeComponent();
        }

        public TlkDocument Document
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
                }

                document = value;

                if (document != null)
                {
                    bindingSource.DataSource = document.Entries;

                    document.PropertyChanged += document_PropertyChanged;
                }
                else
                {
                    bindingSource.DataSource = null;
                }

                EnableMenuItems();

                OnPropertyChanged(new PropertyChangedEventArgs("Document"));
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            switch (e.PropertyName)
            {
                case null:
                case "FileName":
                case "Document":
                    if (FileName != null && Document != null)
                    {
                        dataGridView.Columns[0].DataPropertyName =
                            TlkDocument.IsAlternate(FileName) ?
                            "StrRefAlt" :
                            "StrRef";
                    }
                    break;
            }
        }

        private void Save()
        {
            using (var tmp = new MemoryStream())
            {
                using (var writer = new BinaryWriter(tmp))
                {
                    document.Save(writer);
                }

                File.WriteAllBytes(FileName, tmp.ToArray());
            }
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

        private bool ConfirmRowDelete()
        {
            var text =
                "WARNING! Deleting this row will result in renumbering " +
                "of all rows below it. This is a dangerous operation for " +
                "most TLK files!\r\n\r\n" +
                "Are you sure you really want to delete the row?";
            var dr = MessageBox.Show(this, text, "Confirm Row Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return dr == DialogResult.Yes;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveTlkFileDialog.FileName = FileName;
            if (saveTlkFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                FileName = saveTlkFileDialog.FileName;
                Save();
            }
        }

        private void bindingSource_CurrentChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject =
                bindingSource.Current != null ?
                new TlkEntryView((TlkEntry)bindingSource.Current, TlkDocument.IsAlternate(FileName)) :
                null;
        }

        private void bindingSource_CurrentItemChanged(object sender, EventArgs e)
        {
            propertyGridRefreshTimer.Start();
        }

        private void propertyGridRefreshTimer_Tick(object sender, EventArgs e)
        {
            propertyGrid.Refresh();
            propertyGridRefreshTimer.Stop();
        }

        private void bindingNavigatorDeleteItem_Click(object sender, EventArgs e)
        {
            var selectedGridRows = (from DataGridViewCell cell in dataGridView.SelectedCells
                                    select dataGridView.Rows[cell.RowIndex]).Distinct();

            foreach (DataGridViewRow gridRow in selectedGridRows)
            {
                var entry = (TlkEntry)gridRow.DataBoundItem;

                if (ConfirmRowDelete())
                {
                    document.Entries.Remove(entry);
                }
            }
        }

        private void dataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            e.Cancel = !ConfirmRowDelete();
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

        private void TlkDocumentWindow_FormClosing(object sender, FormClosingEventArgs e)
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
    }
}

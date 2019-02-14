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
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Globalization;
using Int19h.NeverwinterNights.Tlk;
using System.Diagnostics;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class StrRefEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (service == null)
            {
                return value;
            }

            var column = TwoDARowTypeDescriptor.GetColumn(context);
            var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
            if (column == null || schemaColumn == null)
            {
                return value;
            }

            var twoDA = (TwoDADocument)column.Table.ExtendedProperties[typeof(TwoDADocument)];
            var tlk = MainWindow.Instance.GetReferencedTlkDocument(twoDA, false);
            var tlkAlt = MainWindow.Instance.GetReferencedTlkDocument(twoDA, true);
            if (tlk == null && tlkAlt == null)
            {
                string text =
                    "StrRef lookup is currently disabled, because there are no TLK files opened.";
                MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return value;
            }

            var tlkEntries =
                tlk != null ?
                tlk.Entries.Select(entry => new TlkEntryView(entry, false)).ToArray() :
                new TlkEntryView[0];

            var tlkAltEntries =
                tlkAlt != null ?
                tlkAlt.Entries.Select(entry => new TlkEntryView(entry, true)).ToArray() :
                new TlkEntryView[0];

            var entries = tlkEntries.Concat(tlkAltEntries).ToArray();

            int? strref = value as int?;
            int? result = strref;

            TlkEntryView selectedEntry = null;
            if (strref != null)
            {
                int i = (int)strref & 0x00FFFFFF;
                if ((strref & 0x01000000) == 0)
                {
                    selectedEntry = tlkEntries.ElementAtOrDefault(i);
                }
                else
                {
                    selectedEntry = tlkAltEntries.ElementAtOrDefault(i);
                }
            }

            var panel = new Panel { Width = 350, Height = 300 };

            var searchTextBox = new TextBox
            {
                Dock = DockStyle.Top
            };

            var bindingSource = new BindingSource { DataSource = entries };

            var grid = new DataGridView
            {
                AutoGenerateColumns = false,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                ColumnHeadersVisible = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                DataSource = bindingSource
            };

            var timer = new Timer { Interval = 800 };

            grid.Columns.AddRange(
                new[]
                {
                    new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = "StrRef",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                        Width = 75,
                        DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
                    },
                    new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = "Text",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                    }
                });

            panel.Controls.Add(grid);
            panel.Controls.Add(searchTextBox);

            searchTextBox.TextChanged +=
                delegate
                {
                    timer.Stop();
                    timer.Start();
                };

            timer.Tick +=
                delegate
                {
                    timer.Stop();

                    var oldCursor = grid.Cursor;
                    panel.Cursor = Cursors.AppStarting;

                    bindingSource.DataSource = (from entry in entries
                                                where entry.Text != null &&
                                                      entry.Text.IndexOf(searchTextBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0
                                                select entry).ToArray();

                    panel.Cursor = oldCursor;
                };

            bindingSource.CurrentChanged +=
                delegate
                {
                    selectedEntry = (TlkEntryView)bindingSource.Current;
                };

            grid.CellDoubleClick +=
                delegate
                {
                    result = selectedEntry != null ? selectedEntry.StrRef : strref;
                    service.CloseDropDown();
                };

            grid.KeyDown +=
                delegate(object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        result = selectedEntry != null ? selectedEntry.StrRef : strref;
                        service.CloseDropDown();
                        e.Handled = true;
                    }
                };

            grid.KeyPress +=
                delegate(object sender, KeyPressEventArgs e)
                {
                    if (!char.IsControl(e.KeyChar))
                    {
                        searchTextBox.Focus();
                        searchTextBox.SelectAll();
                        SendKeys.Send(e.KeyChar.ToString());
                    }
                };

            grid.DataBindingComplete +=
                delegate
                {
                    var selectedIndex = bindingSource.IndexOf(selectedEntry);
                    if (selectedIndex >= 0)
                    {
                        grid.FirstDisplayedScrollingRowIndex = selectedIndex;
                        grid.CurrentCell = grid.Rows[selectedIndex].Cells[0];
                    }
                };

            panel.GotFocus +=
                delegate
                {
                    grid.Focus();
                };

            bindingSource.DataSource = entries;
            service.DropDownControl(panel);

            return result ?? (object)DBNull.Value;
        }
    }
}

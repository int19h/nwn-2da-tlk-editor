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
using System.Diagnostics;
using System.Data;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class RowRefEditor : UITypeEditor
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
            if (column == null || schemaColumn == null || schemaColumn.RowSource == null)
            {
                return value;
            }

            var refDoc = (TwoDADocument)column.Table.ExtendedProperties[typeof(TwoDADocument)];
            var srcDoc = MainWindow.Instance.GetReferencedTwoDADocument(refDoc, schemaColumn.RowSource);
            if (srcDoc == null)
            {
                string text = string.Format(
                    "RowRef lookup is currently disabled for this property, because '{0}' is not opened.",
                    schemaColumn.RowSource);
                MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return value;
            }

            if (!srcDoc.Data.Columns.Contains(schemaColumn.RowSourceKeyField))
            {
                return value;
            }

            var keyColumn = srcDoc.Data.Columns[schemaColumn.RowSourceKeyField];

            object result = value;
            var view = new DataView(srcDoc.Data);
            var selectedRowView = (from rowView in view.Cast<DataRowView>()
                                   where object.Equals(rowView.Row[keyColumn], value)
                                   select rowView
                                  ).FirstOrDefault();

            var panel = new Panel { Width = 350, Height = 300 };

            var searchTextBox = new TextBox
            {
                Dock = DockStyle.Top
            };

            var bindingSource = new BindingSource { DataSource = view };

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
                        DataPropertyName = keyColumn.ColumnName,
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                        Width = 75,
                        DefaultCellStyle = { WrapMode = DataGridViewTriState.True }
                    },
                    new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = srcDoc.Data.Columns[1].ColumnName,
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

                    if (string.IsNullOrEmpty(searchTextBox.Text))
                    {
                        view.RowFilter = null;
                    }
                    else
                    {
                        var pattern = new StringBuilder();
                        foreach (char c in searchTextBox.Text)
                        {
                            switch (c)
                            {
                                case '\'':
                                    pattern.Append(c);
                                    pattern.Append(c);
                                    break;

                                case '*':
                                case '%':
                                case '[':
                                case ']':
                                    pattern.Append('[');
                                    pattern.Append(c);
                                    pattern.Append(']');
                                    break;

                                default:
                                    pattern.Append(c);
                                    break;
                            }

                            view.RowFilter = string.Format(
                                "[{0}] LIKE '*{1}*'",
                                srcDoc.Data.Columns[1].ColumnName,
                                pattern);
                        }
                    }

                    panel.Cursor = oldCursor;
                };

            bindingSource.CurrentChanged +=
                delegate
                {
                    selectedRowView = (DataRowView)bindingSource.Current;
                };

            grid.CellDoubleClick +=
                delegate
                {
                    result =
                        selectedRowView != null ?
                        selectedRowView.Row[keyColumn] :
                        value;
                    service.CloseDropDown();
                };

            grid.KeyDown +=
                delegate(object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        result =
                            selectedRowView != null ?
                            selectedRowView.Row[keyColumn] :
                            value;
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
                    var selectedIndex = bindingSource.IndexOf(selectedRowView);
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

            bindingSource.DataSource = view;
            service.DropDownControl(panel);

            return result ?? (object)DBNull.Value;
        }
    }
}

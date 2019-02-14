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

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class HexNumberEditor : UITypeEditor
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

            var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
            if (schemaColumn == null)
            {
                return value;
            }

            int result = (int)(value ?? 0);

            var list =
                new CheckedListBox
                {
                    BorderStyle = BorderStyle.None
                };

            foreach (var flag in schemaColumn.Flags)
            {

                string text =
                    "0x" +
                    flag.Bits.ToString("X" + schemaColumn.Digits, CultureInfo.InvariantCulture) +
                    " - " +
                    flag.Name;
                list.Items.Add(text);
            }

            MethodInvoker updateListItems =
                delegate
                {
                    for (int i = 0; i < list.Items.Count; ++i)
                    {
                        var flag = schemaColumn.Flags[i];
                        int bits = (result & flag.Bits);
                        var check =
                            bits == flag.Bits ? CheckState.Checked :
                            bits != 0 ? CheckState.Indeterminate :
                            CheckState.Unchecked;
                        list.SetItemCheckState(i, check);
                    }
                };

            updateListItems();

            bool checkingItem = false;
            list.ItemCheck += 
                delegate(object sender, ItemCheckEventArgs e)
                {
                    if (checkingItem)
                    {
                        return;
                    }
                    else
                    {
                        checkingItem = true;
                    }

                    int bits = schemaColumn.Flags[e.Index].Bits;
                    if (e.NewValue == CheckState.Checked)
                    {
                        result |= bits;
                    }
                    else
                    {
                        result &= ~bits;
                    }

                    updateListItems();

                    checkingItem = false;
                };

            service.DropDownControl(list);

            return result;
        }
    }
}

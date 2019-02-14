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

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public partial class TwoDAAddBlanksDialog : Form
    {
        private int existingRowCount;

        public TwoDAAddBlanksDialog()
        {
            InitializeComponent();

            indexNumericUpDown.Maximum = int.MaxValue;
            countNumericUpDown.Maximum = int.MaxValue;
        }

        public int ExistingRowCount
        {
            get
            {
                return existingRowCount;
            }
            set
            {
                existingRowCount = value;
                indexNumericUpDown.Minimum = existingRowCount;
                UpdateIndex();
            }
        }

        public int NewRowCount
        {
            get
            {
                return (int)countNumericUpDown.Value;
            }
        }

        private void UpdateIndex()
        {
            var newIndex = existingRowCount + countNumericUpDown.Value;
            if (indexNumericUpDown.Value != newIndex)
            {
                indexNumericUpDown.Value = newIndex;
            }
        }

        private void UpdateCount()
        {
            var newCount = indexNumericUpDown.Value - existingRowCount;
            if (countNumericUpDown.Value != newCount)
            {
                countNumericUpDown.Value = newCount;
            }
        }

        private void countNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateIndex();
        }

        private void indexNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateCount();
        }
    }
}

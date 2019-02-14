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
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class DocumentWindow : Form, INotifyPropertyChanged
    {
        private string fileName;

        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
                Text = Path.GetFileName(fileName);

                OnPropertyChanged(new PropertyChangedEventArgs("FileName"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}

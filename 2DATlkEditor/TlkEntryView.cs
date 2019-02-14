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
using Int19h.NeverwinterNights.Tlk;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class TlkEntryView : INotifyPropertyChanged
    {
        private readonly TlkEntry entry;
        private readonly bool showAltStrRef;

        public TlkEntryView(TlkEntry entry, bool showAltStrRef)
        {
            this.entry = entry;
            this.showAltStrRef = showAltStrRef;

            entry.PropertyChanged +=
                delegate(object sender, PropertyChangedEventArgs e)
                {
                    OnPropertyChanged(e);
                };
        }

        [ParenthesizePropertyName(true)]
        [Description("StrRef of the entry.")]
        public int StrRef
        {
            get { return showAltStrRef ? (entry.StrRef | 0x01000000) : entry.StrRef; }
        }

        [Description("Text of the entry.")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Text
        {
            get { return entry.Text; }
            set { entry.Text = value; }
        }

        [Description("ResRef of the sound wave file associated with the entry.")]
        public string SoundResRef
        {
            get { return entry.SoundResRef; }
            set { entry.SoundResRef = value; }
        }

        public int VolumeVariance
        {
            get { return entry.VolumeVariance; }
            set { entry.VolumeVariance = value; }
        }

        public int PitchVariance
        {
            get { return entry.PitchVariance; }
            set { entry.PitchVariance = value; }
        }

        [Description("Duration of the associated sound wave file, in seconds.")]
        public float SoundLength
        {
            get { return entry.SoundLength; }
            set { entry.SoundLength = value; }
        }

        public bool ShouldSerializeText()
        {
            return false;
        }

        public bool ShouldSerializeSoundResRef()
        {
            return false;
        }

        public bool ShouldSerializeVolumeVariance()
        {
            return false;
        }

        public bool ShouldSerializePitchVariance()
        {
            return false;
        }

        public bool ShouldSerializeSoundLength()
        {
            return false;
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

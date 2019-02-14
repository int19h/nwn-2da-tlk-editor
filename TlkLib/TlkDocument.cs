/*
 * Neverwinter Nights TLK Manipulation Library.
 * Copyright (C) 2008 Pavel Minaev
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Diagnostics;

namespace Int19h.NeverwinterNights.Tlk
{
    public enum TlkLanguageID
    {
        English = 0,
        French = 1,
        German = 2,
        Italian = 3,
        Spanish = 4,
        Polish = 5,
        Korean = 128,
        ChineseTraditional = 129,
        ChineseSimplified = 130,
        Japanese = 131
    }

    public class TlkEntry : INotifyPropertyChanging, INotifyPropertyChanged
    {
        private int strRef;

        private string text;

        private string soundResRef;

        private int volumeVariance;

        private int pitchVariance;

        private float soundLength;

        public int StrRef
        {
            get
            {
                return strRef;
            }
            internal set
            {
                OnPropertyChanging(new PropertyChangingEventArgs("StrRef"));
                OnPropertyChanging(new PropertyChangingEventArgs("StrRefAlt"));

                strRef = value;

                OnPropertyChanged(new PropertyChangedEventArgs("StrRef"));
                OnPropertyChanged(new PropertyChangedEventArgs("StrRefAlt"));
            }
        }

        public int StrRefAlt
        {
            get { return strRef | 0x01000000; }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                OnPropertyChanging(new PropertyChangingEventArgs("Text"));

                text = value;

                OnPropertyChanged(new PropertyChangedEventArgs("Text"));
            }
        }

        public string SoundResRef
        {
            get
            {
                return soundResRef;
            }
            set
            {
                if (value != null && value.Length > 16)
                {
                    throw new ArgumentException("SoundResRef cannot be longer than 16 characters", "value");
                }

                OnPropertyChanging(new PropertyChangingEventArgs("SoundResRef"));

                soundResRef = value;

                OnPropertyChanged(new PropertyChangedEventArgs("SoundResRef"));
            }
        }

        public int VolumeVariance
        {
            get
            {
                return volumeVariance;
            }
            set
            {
                OnPropertyChanging(new PropertyChangingEventArgs("VolumeVariance"));

                volumeVariance = value;

                OnPropertyChanged(new PropertyChangedEventArgs("VolumeVariance"));
            }
        }

        public int PitchVariance
        {
            get
            {
                return pitchVariance;
            }
            set
            {
                OnPropertyChanging(new PropertyChangingEventArgs("PitchVariance"));

                pitchVariance = value;

                OnPropertyChanged(new PropertyChangedEventArgs("PitchVariance"));
            }
        }

        public float SoundLength
        {
            get
            {
                return soundLength;
            }
            set
            {
                OnPropertyChanging(new PropertyChangingEventArgs("SoundLength"));

                soundLength = value;

                OnPropertyChanged(new PropertyChangedEventArgs("SoundLength"));
            }
        }

        public event PropertyChangingEventHandler PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, e);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }

    public class TlkDocument : INotifyPropertyChanged
    {
        private abstract class Change
        {
            public abstract void Undo(TlkDocument doc);
        }

        private class PropertyModifiedChange : Change
        {
            private int entryIndex;
            private string propertyName;
            private object oldValue;

            public PropertyModifiedChange(int entryIndex, string propertyName, object oldValue)
            {
                this.entryIndex = entryIndex;
                this.propertyName = propertyName;
                this.oldValue = oldValue;
            }

            public override void Undo(TlkDocument doc)
            {
                var entry = doc.Entries[entryIndex];
                entry.GetType().GetProperty(propertyName).SetValue(entry, oldValue, null);
            }
        }

        private class EntryAddedChange : Change
        {
            private int entryIndex;

            public EntryAddedChange(int entryIndex)
            {
                this.entryIndex = entryIndex;
            }

            public override void Undo(TlkDocument doc)
            {
                doc.Entries.RemoveAt(entryIndex);
            }
        }

        private class EntryRemovedChange : Change
        {
            private int entryIndex;
            private TlkEntry entry;

            public EntryRemovedChange(int entryIndex, TlkEntry entry)
            {
                this.entryIndex = entryIndex;
                this.entry = entry;
            }

            public override void Undo(TlkDocument doc)
            {
                doc.Entries.Insert(entryIndex, entry);
            }
        }

        private class EntryCollection : BindingList<TlkEntry>
        {
            private readonly TlkDocument doc;
            private object oldValue;

            public EntryCollection(TlkDocument doc)
            {
                this.doc = doc;
            }

            protected override void SetItem(int index, TlkEntry item)
            {
                throw new NotSupportedException("Unsupported operation");
            }

            protected override void ClearItems()
            {
                doc.BeginChangeSet();

                for (int i = 0; i < Count; ++i)
                {
                    var item = this[i];

                    doc.RegisterChange(new EntryRemovedChange(i, item));

                    item.PropertyChanging -= item_PropertyChanging;
                }

                doc.EndChangeSet();
            }

            protected override void InsertItem(int index, TlkEntry item)
            {
                base.InsertItem(index, item);
                doc.RegisterChange(new EntryAddedChange(index));

                item.PropertyChanging += item_PropertyChanging;
            }

            protected override void RemoveItem(int index)
            {
                var item = this[index];

                doc.RegisterChange(new EntryRemovedChange(index, item));
                base.RemoveItem(index);

                item.PropertyChanging -= item_PropertyChanging;
            }

            protected override void OnListChanged(ListChangedEventArgs e)
            {
                base.OnListChanged(e);

                if (e.ListChangedType == ListChangedType.ItemChanged)
                {
                    doc.RegisterChange(new PropertyModifiedChange(e.NewIndex, e.PropertyDescriptor.Name, oldValue));
                }

                doc.IsModified = true;
            }

            private void item_PropertyChanging(object sender, PropertyChangingEventArgs e)
            {
                oldValue = sender.GetType().GetProperty(e.PropertyName).GetValue(sender, null);
            }
        }

        private const string properSignature = "TLK V3.0";
        private static readonly Encoding latin1 = Encoding.GetEncoding(1252);

        private TlkLanguageID languageID;
        private bool isLoading;
        private bool isModified;
        private Stack<Stack<Change>> undoStack = new Stack<Stack<Change>>();
        private Stack<Stack<Change>> redoStack = new Stack<Stack<Change>>();
        private int changeSetDepth;

        public TlkDocument()
        {
            Entries = new EntryCollection(this);
            Entries.ListChanged += Entries_ListChanged;
            Entries.AddingNew += Entries_AddingNew;
        }

        public TlkLanguageID LanguageID
        {
            get { return languageID; }
            set { languageID = value; }
        }

        public BindingList<TlkEntry> Entries
        {
            get;
            private set;
        }

        public bool IsModified
        {
            get
            {
                return isModified;
            }
            private set
            {
                if (isModified != value)
                {
                    isModified = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsModified"));
                }
            }
        }

        public bool CanUndo
        {
            get { return undoStack.Count != 0; }
        }

        public bool CanRedo
        {
            get { return redoStack.Count != 0; }
        }

        public static bool IsAlternate(string fileName)
        {
            var name = Path.GetFileName(fileName);
            return
                !"dialog.tlk".Equals(name, StringComparison.OrdinalIgnoreCase) &&
                !"dialogf.tlk".Equals(name, StringComparison.OrdinalIgnoreCase);
        }

        public void Load(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("Stream must be readable and seekable", "stream");
            }

            using (var reader = new BinaryReader(stream))
            {
                Load(reader);
            }
        }

        public void Load(BinaryReader reader)
        {
            Load(reader, null);
        }

        public void Load(BinaryReader reader, Action<int, int> progressCallback)
        {
            var stream = reader.BaseStream;
            if (!stream.CanRead || !stream.CanSeek)
            {
                throw new ArgumentException("BaseStream must be readable and seekable", "reader");
            }

            string signature = Encoding.ASCII.GetString(reader.ReadBytes(properSignature.Length));
            if (signature != properSignature)
            {
                throw new InvalidDataException("File does not have a proper TLK V3.0 signature");
            }

            LanguageID = (TlkLanguageID)reader.ReadInt32();

            int entriesCount = reader.ReadInt32();
            int entriesOffset = reader.ReadInt32();

            isLoading = true;
            try
            {
                Entries.Clear();
                for (int i = 0; i < entriesCount; ++i)
                {
                    var entry = new TlkEntry { StrRef = i };

                    int flags = reader.ReadInt32();

                    byte[] soundResRef = reader.ReadBytes(16);
                    if ((flags & 0x2) != 0)
                    {
                        entry.SoundResRef = Encoding.ASCII.GetString(soundResRef).TrimEnd('\0');
                    }

                    entry.VolumeVariance = reader.ReadInt32();
                    entry.PitchVariance = reader.ReadInt32();

                    int offset = reader.ReadInt32();
                    int length = reader.ReadInt32();

                    float soundLength = reader.ReadSingle();
                    if ((flags & 0x4) != 0)
                    {
                        entry.SoundLength = soundLength;
                    }

                    if ((flags & 0x1) != 0)
                    {
                        long bookmark = stream.Position;

                        stream.Position = entriesOffset + offset;
                        entry.Text = latin1.GetString(reader.ReadBytes(length));

                        stream.Position = bookmark;
                    }

                    Entries.Add(entry);

                    if (progressCallback != null)
                    {
                        progressCallback(i + 1, entriesCount);
                    }
                }

                IsModified = false;
            }
            finally
            {
                isLoading = false;
            }

            undoStack.Clear();
            redoStack.Clear();

            OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes(properSignature));
            writer.Write((int)LanguageID);
            writer.Write(Entries.Count);

            int entriesOffset = (8 + 4 + 4 + 4) + Entries.Count * (4 + 16 + 4 + 4 + 4 + 4 + 4);
            writer.Write(entriesOffset);

            int offset = 0;
            foreach (var entry in Entries)
            {
                int flags = 0;
                if (entry.Text != null)
                {
                    flags |= 0x1;
                }
                if (entry.SoundResRef != null)
                {
                    flags |= 0x2;
                }
                if (entry.SoundLength != 0)
                {
                    flags |= 0x4;
                }
                writer.Write(flags);

                if (entry.SoundResRef != null)
                {
                    string soundResRef = entry.SoundResRef.PadRight(16, '\0');
                    writer.Write(Encoding.ASCII.GetBytes(soundResRef));
                }
                else
                {
                    writer.Write(new byte[16]);
                }

                writer.Write(entry.VolumeVariance);
                writer.Write(entry.PitchVariance);

                if (entry.Text != null)
                {
                    writer.Write(offset);
                    writer.Write(entry.Text.Length);
                }
                else
                {
                    writer.Write((int)0);
                    writer.Write((int)0);
                }

                writer.Write(entry.SoundLength);

                if (entry.Text != null)
                {
                    offset += entry.Text.Length;
                }
            }

            Trace.Assert(writer.BaseStream.Position == entriesOffset);

            foreach (var entry in Entries)
            {
                if (entry.Text != null)
                {
                    writer.Write(latin1.GetBytes(entry.Text));
                }
            }

            IsModified = false;
        }

        public void BeginChangeSet()
        {
            if (++changeSetDepth == 1)
            {
                undoStack.Push(new Stack<Change>());
            }
        }

        public void EndChangeSet()
        {
            if (changeSetDepth == 0)
            {
                throw new InvalidOperationException("EndChangeSet() called without matching BeginChangeSet()");
            }

            if (--changeSetDepth == 0)
            {
                if (undoStack.Peek().Count == 0)
                {
                    undoStack.Pop();
                }
                else
                {
                    redoStack.Clear();
                    OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
                }
            }
        }

        public void Undo()
        {
            if (!CanUndo)
            {
                throw new InvalidOperationException("Nothing to undo");
            }

            var changeSet = undoStack.Pop();

            var realUndoStack = undoStack;
            var realRedoStack = redoStack;

            undoStack = redoStack;
            redoStack = new Stack<Stack<Change>>();

            BeginChangeSet();
            while (changeSet.Count != 0)
            {
                var change = changeSet.Pop();
                change.Undo(this);
            }
            EndChangeSet();

            undoStack = realUndoStack;
            redoStack = realRedoStack;

            OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));
            OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
        }

        public void Redo()
        {
            if (!CanRedo)
            {
                throw new InvalidOperationException("Nothing to redo");
            }

            var changeSet = redoStack.Pop();

            var realRedoStack = redoStack;
            redoStack = new Stack<Stack<Change>>();

            BeginChangeSet();
            while (changeSet.Count != 0)
            {
                var change = changeSet.Pop();
                change.Undo(this);
            }
            EndChangeSet();

            redoStack = realRedoStack;

            OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private void RegisterChange(Change change)
        {
            Stack<Change> changeSet;
            if (changeSetDepth == 0)
            {
                changeSet = new Stack<Change>();
                undoStack.Push(changeSet);
                redoStack.Clear();

                OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
            }
            else
            {
                changeSet = undoStack.Peek();
            }

            changeSet.Push(change);
            OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));
        }

        private void Entries_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    if (e.NewIndex >= 0)
                    {
                        for (int i = e.NewIndex + 1; i < Entries.Count; ++i)
                        {
                            ++Entries[i].StrRef;
                        }
                    }
                    break;

                case ListChangedType.ItemDeleted:
                    if (e.OldIndex >= 0)
                    {
                        for (int i = e.OldIndex; i < Entries.Count; ++i)
                        {
                            --Entries[i].StrRef;
                        }
                    }
                    break;
            }
        }

        private void Entries_AddingNew(object sender, AddingNewEventArgs e)
        {
            e.NewObject = new TlkEntry { StrRef = Entries.Count };
        }
    }
}

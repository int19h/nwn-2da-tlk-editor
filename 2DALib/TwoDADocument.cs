/*
 * Neverwinter Nights 2DA Manipulation Library.
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
using System.IO;
using System.Globalization;
using System.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Int19h.NeverwinterNights.TwoDA
{
    /// <summary>
    /// An in-memory representation of a 2DA document.
    /// </summary>
    public class TwoDADocument : INotifyPropertyChanged
    {
        /// <summary>
        /// An abstract base class representing single undoable change in an undo stack. 
        /// </summary>
        private abstract class Change
        {
            /// <summary>
            /// Undoes the change.
            /// </summary>
            /// <param name="doc">The document on which to undo the change.</param>
            public abstract void Undo(TwoDADocument doc);
        }

        /// <summary>
        /// Represents a single change of value of a column in a specific row.
        /// </summary>
        private class FieldModifiedChange : Change
        {
            private int columnIndex, rowIndex;
            private object oldValue;

            /// <summary>
            /// Initializes a new instance of the <see cref="FieldModifiedChange"/> class.
            /// </summary>
            /// <param name="columnIndex">Index of the column that was modified.</param>
            /// <param name="rowIndex">Index of the row that was modified.</param>
            /// <param name="oldValue">The value at that column and row before modification.</param>
            public FieldModifiedChange(int columnIndex, int rowIndex, object oldValue)
            {
                this.columnIndex = columnIndex;
                this.rowIndex = rowIndex;
                this.oldValue = oldValue;
            }

            /// <summary>
            /// Undoes the change.
            /// </summary>
            /// <param name="doc">The document on which to undo the change.</param>
            public override void Undo(TwoDADocument doc)
            {
                doc.Data.Rows[rowIndex][columnIndex] = oldValue;
            }
        }

        /// <summary>
        /// Represents an addition of a single new row.
        /// </summary>
        private class RowAddedChange : Change
        {
            private int rowIndex;

            /// <summary>
            /// Initializes a new instance of the <see cref="RowAddedChange"/> class.
            /// </summary>
            /// <param name="rowIndex">Index of the row that was added.</param>
            public RowAddedChange(int rowIndex)
            {
                this.rowIndex = rowIndex;
            }

            /// <summary>
            /// Undoes the change.
            /// </summary>
            /// <param name="doc">The document on which to undo the change.</param>
            public override void Undo(TwoDADocument doc)
            {
                doc.Data.Rows[rowIndex].Delete();
            }
        }

        /// <summary>
        /// Represents a deletion of a single row.
        /// </summary>
        private class RowDeletedChange : Change
        {
            private int rowIndex;
            private object[] rowItemArray;

            /// <summary>
            /// Initializes a new instance of the <see cref="RowDeletedChange"/> class.
            /// </summary>
            /// <param name="rowIndex">Index of the row that was deleted.</param>
            /// <param name="rowItemArray">Values in the fields of the deleted row, in document order.</param>
            public RowDeletedChange(int rowIndex, object[] rowItemArray)
            {
                this.rowIndex = rowIndex;
                this.rowItemArray = rowItemArray;
            }

            /// <summary>
            /// Undoes the change.
            /// </summary>
            /// <param name="doc">The document on which to undo the change.</param>
            public override void Undo(TwoDADocument doc)
            {
                var row = doc.Data.NewRow();
                row.ItemArray = rowItemArray;
                doc.Data.Rows.InsertAt(row, rowIndex);
            }
        }

        /// <summary>
        /// Raw string representation of a "blank entry" in a 2DA file. When the 2DA is loaded
        /// into a <see cref="DataTable"/>, 2DA values matching this string will be represented
        /// as <see cref="DBNull.Value"/>.
        /// </summary>
        public const string BlankEntry = "****";

        private const string validSignature = "2DA V2.0";
        private const string defaultValueMarker = "DEFAULT:";
        private readonly char[] charsForbiddenInFieldValues = { '"', '\n', '\r' };

        private string defaultString = "";
        private TwoDASchema schema;
        private TwoDASchema.Column[] schemaColumns;
        private bool isLoading;
        private bool isModified;
        private Stack<Stack<Change>> undoStack = new Stack<Stack<Change>>();
        private Stack<Stack<Change>> redoStack = new Stack<Stack<Change>>();
        private int changeSetDepth;
        private DataRowState changedRowOldState;
        private int deletedRowIndex;
        private object[] deletedRowItemArray;
        private int changedColumnRowIndex;
        private object changedColumnOldValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoDADocument"/> class.
        /// </summary>
        public TwoDADocument()
        {
            Data = new DataTable();
            Data.ExtendedProperties[typeof(TwoDADocument)] = this;
            Data.TableNewRow += Data_TableNewRow;
            Data.RowChanging += Data_RowChanging;
            Data.RowChanged += Data_RowChanged;
            Data.RowDeleting += Data_RowDeleting;
            Data.RowDeleted += Data_RowDeleted;
            Data.ColumnChanging += Data_ColumnChanging;
            Data.ColumnChanged += Data_ColumnChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwoDADocument"/> class with
        /// a specified schema.
        /// </summary>
        /// <param name="schema">The schema to use.</param>
        public TwoDADocument(TwoDASchema schema)
            : this()
        {
            Schema = schema;
        }

        /// <summary>
        /// Gets or sets the default field value for this 2DA (specified by "DEFAULT:" declaration
        /// on the second line of the 2DA), as a raw string.
        /// </summary>
        /// <value>The default field value for this 2DA.</value>
        /// <remarks>
        /// Changes to this property are reported via the <see cref="PropertyChanged"/> event.
        /// </remarks>
        public string DefaultString
        {
            get
            {
                return defaultString;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (defaultString != value)
                {
                    defaultString = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("DefaultString"));
                    OnPropertyChanged(new PropertyChangedEventArgs("DefaultInt32"));
                    OnPropertyChanged(new PropertyChangedEventArgs("DefaultSingle"));
                    IsModified = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default field value for this 2DA (specified by "DEFAULT:" declaration
        /// on the second line of the 2DA), as a <see cref="System.Int32"/>.
        /// </summary>
        /// <value>The default integer field value for this 2DA.</value>
        /// <remarks>
        /// Changes to this property are reported via the <see cref="PropertyChanged"/> event.
        /// </remarks>
        public int DefaultInt32
        {
            get
            {
                int result;
                int.TryParse(DefaultString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
                return result;
            }
            set
            {
                DefaultString = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the default field value for this 2DA (specified by "DEFAULT:" declaration
        /// on the second line of the 2DA), as a <see cref="System.Single"/>.
        /// </summary>
        /// <value>The default floating-point field value for this 2DA.</value>
        /// <remarks>
        /// Changes to this property are reported via the <see cref="PropertyChanged"/> event.
        /// </remarks>
        public float DefaultSingle
        {
            get
            {
                float result;
                float.TryParse(DefaultString, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
                return result;
            }
            set
            {
                DefaultString = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the actual data contained in the 2DA, represented as a <see cref="DataTable"/>.
        /// </summary>
        /// <value>DataTable containing the actual data.</value>
        public DataTable Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the schema of the data in the 2DA.
        /// </summary>
        /// <value>The schema.</value>
        public TwoDASchema Schema
        {
            get
            {
                return schema;
            }
            private set
            {
                if (schema != value)
                {
                    schema = value;
                    FillSchemaColumns();
                }
            }
        }

        /// <summary>
        /// Gets the collection of schema columns, with indices corresponding to indices of columns
        /// in the <see cref="DataTable"/> returned from <see cref="Data"/> property. If some
        /// column from <see cref="Data"/> has no corresponding schema column, the returned
        /// collection will have <c>null</c> at that index.
        /// </summary>
        /// <value>The schema columns.</value>
        public ReadOnlyCollection<TwoDASchema.Column> SchemaColumns
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has been modified since it was
        /// loaded, or since the last call to <see cref="Save"/>.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is modified; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Changes to this property are reported via the <see cref="PropertyChanged"/> event.
        /// </remarks>
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

        /// <summary>
        /// Gets a value indicating whether this instance has any changes that can be undone
        /// via a call to <see cref="Undo"/>
        /// </summary>
        /// <value><c>true</c> if there are any undoable changes; otherwise, <c>false</c>.</value>
        public bool CanUndo
        {
            get { return undoStack.Count != 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has any undone changes that can be
        /// reapplied via a call to <see cref="Redo"/>
        /// </summary>
        /// <value><c>true</c> if there are any redoable changes; otherwise, <c>false</c>.</value>
        public bool CanRedo
        {
            get { return redoStack.Count != 0; }
        }

        /// <summary>
        /// Loads 2DA data from the specified reader.
        /// </summary>
        /// <param name="reader">The reader to read the data from.</param>
        /// <exception cref="InvalidDataException">
        /// 2DA data in the reader is not in a correct format, is corrupted, or does not match
        /// the schema.
        /// </exception>
        public void Load(TextReader reader)
        {
            Load(reader, null);
        }

        /// <summary>
        /// Loads 2DA data from the specified read.
        /// </summary>
        /// <param name="reader">The reader to read the data from.</param>
        /// <param name="lineNumberCallback">
        /// The line number callback. For each line read from the reader, the callback will
        /// be invoked once, with the number of the line read passed as an argument.
        /// </param>
        /// <exception cref="InvalidDataException">
        /// 2DA data in the reader is not in a correct format, is corrupted, or does not match
        /// the schema.
        /// </exception>
        public void Load(TextReader reader, Action<int> lineNumberCallback)
        {
            int lineNumber = 0;

            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            try
            {
                isLoading = true;

                Data.Clear();
                Data.Columns.Clear();

                string signatureLine = reader.ReadLine().TrimEnd();
                ++lineNumber;

                if (signatureLine != validSignature)
                {
                    throw new InvalidDataException(string.Format(
                        "Error at line {0}: '{1}' is not a valid 2DA signature",
                        lineNumber,
                        signatureLine));
                }

                if (lineNumberCallback != null)
                {
                    lineNumberCallback(lineNumber);
                }

                string defaultValueLine = reader.ReadLine().TrimEnd();
                ++lineNumber;

                if (defaultValueLine == null)
                {
                    throw new InvalidDataException(string.Format(
                        "Error at line {0}: default value line is missing",
                        lineNumber));
                }

                if (defaultValueLine.Length != 0)
                {
                    if (!defaultValueLine.StartsWith(defaultValueMarker))
                    {
                        throw new InvalidDataException(string.Format(
                            "Error at line {0}: default value line must either be blank, or begin with '{1}'",
                            lineNumber,
                            defaultValueMarker));
                    }
                    DefaultString = defaultValueLine.Remove(0, defaultValueMarker.Length).TrimStart();
                }

                if (lineNumberCallback != null)
                {
                    lineNumberCallback(lineNumber);
                }

                string columnNamesLine = null;
                do
                {
                    columnNamesLine = reader.ReadLine().TrimEnd();
                    ++lineNumber;
                } while (columnNamesLine == "");

                if (columnNamesLine == null)
                {
                    throw new InvalidDataException(string.Format(
                        "Error at line {0}: column names line is missing",
                        lineNumber));
                }

                var rowNumberColumn = new DataColumn("#", typeof(int));
                rowNumberColumn.AllowDBNull = true;
                Data.Columns.Add(rowNumberColumn);

                var columnNames = GetFieldValues(columnNamesLine);
                foreach (string columnName in columnNames)
                {
                    var column = new DataColumn(columnName, typeof(string));

                    TwoDASchema.Column schemaColumn = null;
                    if (Schema != null && Schema.Columns != null)
                    {
                        schemaColumn = Schema.Columns.FirstOrDefault(c => c.Name == columnName);
                    }

                    if (schemaColumn == null)
                    {
                        column.DataType = typeof(string);
                        column.MaxLength = 267;
                        column.AllowDBNull = true;
                        column.DefaultValue = DBNull.Value;
                    }
                    else
                    {
                        column.AllowDBNull = schemaColumn.AllowBlanks;
                        switch (schemaColumn.DataType)
                        {
                            case TwoDASchema.DataType.String:
                                column.DataType = typeof(string);
                                column.MaxLength = 267;
                                break;

                            case TwoDASchema.DataType.Float:
                                column.DataType = typeof(float);
                                break;

                            case TwoDASchema.DataType.Integer:
                            case TwoDASchema.DataType.HexInteger:
                            case TwoDASchema.DataType.StrRef:
                                column.DataType = typeof(int);
                                break;
                        }
                    }

                    Data.Columns.Add(column);
                }
                FillSchemaColumns();

                if (lineNumberCallback != null)
                {
                    lineNumberCallback(lineNumber);
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ++lineNumber;
                    line = line.TrimEnd();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    DataRow row = Data.NewRow();

                    IEnumerable<string> fieldValues = GetFieldValues(line);
                    int columnIndex = 0;
                    foreach (string fieldValue in fieldValues)
                    {
                        if (columnIndex >= Data.Columns.Count)
                        {
                            throw new InvalidDataException(string.Format(
                                "Error at line {0}: too many field values",
                                lineNumber));
                        }

                        object actualValue =
                            fieldValue == BlankEntry ?
                            (object)DBNull.Value :
                            fieldValue;

                        var schemaColumn = schemaColumns[columnIndex];
                        if (schemaColumn != null && actualValue != DBNull.Value)
                        {
                            switch (schemaColumn.DataType)
                            {
                                case TwoDASchema.DataType.Integer:
                                case TwoDASchema.DataType.StrRef:
                                    {
                                        actualValue = int.Parse(fieldValue, CultureInfo.InvariantCulture);
                                    } break;

                                case TwoDASchema.DataType.HexInteger:
                                    {
                                        if (fieldValue.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                                        {
                                            actualValue = int.Parse(fieldValue.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                        }
                                        else
                                        {
                                            actualValue = int.Parse(fieldValue, CultureInfo.InvariantCulture);
                                        }
                                    } break;

                                case TwoDASchema.DataType.Float:
                                    {
                                        actualValue = float.Parse(fieldValue, CultureInfo.InvariantCulture);
                                    } break;
                            }
                        }

                        try
                        {
                            row[columnIndex] = actualValue;
                        }
                        catch (ArgumentException ex)
                        {
                            throw new InvalidDataException(string.Format(
                                "Error at line {0}: {1}",
                                lineNumber,
                                ex.Message), ex);
                        }

                        ++columnIndex;
                    }

                    Data.Rows.Add(row);

                    if (lineNumberCallback != null)
                    {
                        lineNumberCallback(lineNumber);
                    }
                }

                Data.AcceptChanges();
                IsModified = false;
            }
            catch (DataException ex)
            {
                throw new InvalidDataException(string.Format(
                    "Error at line {0}: {1}",
                    lineNumber,
                    ex.Message), ex);
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

        /// <summary>
        /// Saves the 2DA data to the specified writer.
        /// </summary>
        /// <param name="writer">The writer to save the data to.</param>
        public void Save(TextWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var columnWidths = new int[Data.Columns.Count];
            for (int i = 1; i < Data.Columns.Count; ++i)
            {
                string s = FormatFieldValue(Data.Columns[i].ColumnName, null);
                columnWidths[i] = Math.Max(columnWidths[i], s.Length + 1);
            }
            foreach (var row in Data.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted))
            {
                for (int i = 0; i < Data.Columns.Count; ++i)
                {
                    string s = FormatFieldValue(row[i], schemaColumns[i]);
                    columnWidths[i] = Math.Max(columnWidths[i], s.Length + 1);
                }
            }

            writer.WriteLine(validSignature);

            if (!string.IsNullOrEmpty(defaultString))
            {
                writer.WriteLine(defaultValueMarker + defaultString);
            }
            else
            {
                writer.WriteLine();
            }

            if (Data.Columns.Count != 0)
            {
                writer.Write(new string(' ', columnWidths[0]));
            }
            for (int i = 1; i < Data.Columns.Count; ++i)
            {
                string s = FormatFieldValue(Data.Columns[i].ColumnName, null);
                writer.Write(s.PadRight(columnWidths[i]));
            }
            writer.WriteLine();

            foreach (var row in Data.AsEnumerable().Where(row => row.RowState != DataRowState.Deleted))
            {
                for (int i = 0; i < Data.Columns.Count; ++i)
                {
                    string s = FormatFieldValue(row[i], schemaColumns[i]);
                    writer.Write(s.PadRight(columnWidths[i]));
                }
                writer.WriteLine();
            }

            IsModified = false;
        }

        /// <summary>
        /// Begins an atomic set of changes, that should only be undoable as a single
        /// batch. Every call to this method should be paired with a call to <see cref="EndChangeSet"/>.
        /// </summary>
        /// <remarks>
        /// Nested calls are allowed, but they do not start new nested changesets. There
        /// can only be one changeset at a time. However, every nested call to this method
        /// still requires a paired call to <see cref="EndChangeSet"/>.
        /// </remarks>
        /// <seealso cref="Change"/>
        public void BeginChangeSet()
        {
            if (++changeSetDepth == 1)
            {
                undoStack.Push(new Stack<Change>());
            }
        }

        /// <summary>
        /// Ends an atomic set of changes started by <see cref="BeginChangeSet"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">No matching call to <see cref="BeginChangeSet"/>.</exception>
        /// <seealso cref="Change"/>
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

        /// <summary>
        /// Undoes the most recent change or a changeset made on this document.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="CanUndo"/> is false.</exception>
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

        /// <summary>
        /// Redoes the most recent undone change or a changeset made on this document.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="CanRedo"/> is false.</exception>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Updates the <see cref="SchemaColumns"/> property.
        /// </summary>
        private void FillSchemaColumns()
        {
            schemaColumns = new TwoDASchema.Column[Data.Columns.Count];
            SchemaColumns = new ReadOnlyCollection<TwoDASchema.Column>(schemaColumns);
            if (schema != null)
            {
                foreach (DataColumn column in Data.Columns)
                {
                    schemaColumns[column.Ordinal] = schema.Columns.FirstOrDefault(sc => sc.Name == column.ColumnName);
                }
            }

            IsModified = true;
        }

        /// <summary>
        /// Parses a single 2DA row and extracts raw string field values from it.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        private static IEnumerable<string> GetFieldValues(string line)
        {
            var value = new StringBuilder();
            bool quoted = false;

            foreach (char c in line)
            {
                switch (c)
                {
                    case '"':
                        quoted = !quoted;
                        break;

                    case ' ':
                    case '\t':
                        if (quoted)
                        {
                            goto default;
                        }
                        else
                        {
                            if (value.Length != 0)
                            {
                                yield return value.ToString();
                                value.Length = 0;
                            }
                            break;
                        }

                    default:
                        value.Append(c);
                        break;
                }
            }

            if (value.Length != 0)
            {
                yield return value.ToString();
            }
        }

        /// <summary>
        /// Formats a strongly-typed field value in a format suitable for writing to the 2DA file,
        /// according to the schema for the corresponding column.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="schemaColumn">The schema of the column from which the value was taken.</param>
        /// <returns></returns>
        private static string FormatFieldValue(object value, TwoDASchema.Column schemaColumn)
        {
            if (value == null || value is DBNull)
            {
                return BlankEntry;
            }

            if (value is int && schemaColumn != null && schemaColumn.DataType == TwoDASchema.DataType.HexInteger)
            {
                return "0x" + ((int)value).ToString("X" + schemaColumn.Digits);
            }

            string result = Convert.ToString(value, CultureInfo.InvariantCulture);
            result = result.Replace('"', '\'');
            if (result.Contains(' '))
            {
                result = '"' + result + '"';
            }

            return result;
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

        void Data_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            e.Row[0] = Data.Rows.Count;
        }

        private void Data_RowChanging(object sender, DataRowChangeEventArgs e)
        {
            changedRowOldState = e.Row.RowState;
        }

        private void Data_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            if (e.Action == DataRowAction.Add)
            {
                var rowIndex = Data.Rows.IndexOf(e.Row);

                Data.BeginLoadData();
                BeginChangeSet();
                try
                {
                    for (int i = rowIndex + 1; i < Data.Rows.Count; ++i)
                    {
                        var row = Data.Rows[i];

                        if (row.RowState == DataRowState.Deleted)
                        {
                            continue;
                        }

                        int? id = row.Field<int?>(0);
                        if (id != null)
                        {
                            row.SetField<int>(0, (int)id + 1);
                        }
                    }

                    var change = new RowAddedChange(rowIndex);
                    RegisterChange(change);
                }
                finally
                {
                    EndChangeSet();
                    Data.EndLoadData();
                }
            }

            IsModified = true;
        }

        private void Data_RowDeleting(object sender, DataRowChangeEventArgs e)
        {
            deletedRowIndex = Data.Rows.IndexOf(e.Row);
            deletedRowItemArray = e.Row.ItemArray;
        }

        private void Data_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            Data.BeginInit();
            BeginChangeSet();
            try
            {
                for (int i = deletedRowIndex + 1; i < Data.Rows.Count; ++i)
                {
                    var row = Data.Rows[i];

                    if (row.RowState == DataRowState.Deleted)
                    {
                        continue;
                    }

                    int? id = row.Field<int?>(0);
                    if (id != null)
                    {
                        row.SetField<int>(0, (int)id - 1);
                    }
                }

                if (e.Row.RowState != DataRowState.Detached)
                {
                    Data.Rows.RemoveAt(deletedRowIndex);
                }

                var change = new RowDeletedChange(deletedRowIndex, deletedRowItemArray);
                RegisterChange(change);
            }
            finally
            {
                EndChangeSet();
                Data.EndInit();
            }

            IsModified = true;
        }

        private void Data_ColumnChanging(object sender, DataColumnChangeEventArgs e)
        {
            var s = e.ProposedValue as string;
            if (s != null && s.IndexOfAny(charsForbiddenInFieldValues) >= 0)
            {
                throw new InvalidConstraintException("Field value cannot contain a quotation mark character or a newline character.");
            }

            changedColumnRowIndex = Data.Rows.IndexOf(e.Row);
            changedColumnOldValue = e.Row[e.Column];
        }

        private void Data_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            if (isLoading)
            {
                return;
            }

            BeginChangeSet();
            try
            {
                var change = new FieldModifiedChange(
                    e.Column.Ordinal,
                    changedColumnRowIndex,
                    changedColumnOldValue);
                RegisterChange(change);

                IsModified = true;
            }
            finally
            {
                EndChangeSet();
            }
        }
    }
}

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
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Collections;
using System.Drawing.Design;
using Int19h.NeverwinterNights.Tlk;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public class TwoDARowTypeDescriptor : ICustomTypeDescriptor
    {
        private class ColumnTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (TwoDADocument.BlankEntry.Equals(value))
                {
                    return DBNull.Value;
                }

                var column = TwoDARowTypeDescriptor.GetColumn(context);
                if (column == null)
                {
                    return value;
                }

                var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
                if (schemaColumn != null)
                {
                    if (schemaColumn.RowSource != null ||
                        schemaColumn.DataType == TwoDASchema.DataType.StrRef ||
                        schemaColumn.Enum != null)
                    {
                        var s = value as string;
                        if (s != null)
                        {
                            int sepPos = s.IndexOf('\u2013');
                            if (sepPos > 0)
                            {
                                value = s.Substring(0, sepPos).TrimEnd('\u2002');
                            }
                        }
                    }
                    else if (schemaColumn.DataType == TwoDASchema.DataType.HexInteger)
                    {
                        var s = value as string;
                        if (s != null && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            int sepPos = s.IndexOf('\u2013');
                            if (sepPos > 0)
                            {
                                s = s.Substring(0, sepPos);
                            }
                            value = int.Parse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                    }
                }

                return Convert.ChangeType(value, column.DataType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (context != null && destinationType == typeof(string))
                {
                    if (value is DBNull)
                    {
                        return TwoDADocument.BlankEntry;
                    }

                    var column = TwoDARowTypeDescriptor.GetColumn(context);
                    if (column == null)
                    {
                        return value;
                    }

                    var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
                    if (schemaColumn != null)
                    {
                        if (schemaColumn.Enum != null && value != null)
                        {
                            var msv = schemaColumn.Enum.FirstOrDefault(
                                sv =>
                                {
                                    try
                                    {
                                        var o = Convert.ChangeType(sv.Value, value.GetType());
                                        return object.Equals(value, o);
                                    }
                                    catch
                                    {
                                        return false;
                                    }
                                });
                            if (msv != null && msv.Name != null)
                            {
                                return string.Format("{0}\u2002\u2013\u2002{1}", value, msv.Name);
                            }

                        }
                        else if (schemaColumn.RowSource != null)
                        {
                            DataColumn keyColumn;
                            var referencedRows = GetReferencedRows(context, out keyColumn);
                            if (referencedRows != null)
                            {
                                var referencedRow = referencedRows.FirstOrDefault(row => object.Equals(row[keyColumn], value));
                                if (referencedRow != null)
                                {
                                    return RowReferenceToString(referencedRow, keyColumn);
                                }
                            }
                        }
                        else if (schemaColumn.DataType == TwoDASchema.DataType.StrRef)
                        {
                            if (value is int)
                            {
                                int strref = (int)value;

                                var twoDA = (TwoDADocument)column.Table.ExtendedProperties[typeof(TwoDADocument)];
                                var tlk = MainWindow.Instance.GetReferencedTlkDocument(twoDA, (strref & 0x01000000) != 0);
                                if (tlk != null)
                                {
                                    var referencedEntry = tlk.Entries[strref & 0x00FFFFFF];
                                    if (referencedEntry != null)
                                    {
                                        return StrReferenceToString(strref, referencedEntry);
                                    }
                                }
                            }
                        }
                        else if (schemaColumn.DataType == TwoDASchema.DataType.HexInteger)
                        {
                            if (value is int)
                            {
                                int i = (int)value;
                                string desc = "";
                                if (schemaColumn.Flags != null)
                                {
                                    foreach (var flag in schemaColumn.Flags)
                                    {
                                        if ((i & flag.Bits) != 0)
                                        {
                                            if (desc.Length == 0)
                                            {
                                                desc = " \u2013 ";
                                            }
                                            else
                                            {
                                                desc += ", ";
                                            }
                                            desc += flag.Name;
                                        }
                                    }
                                }

                                return "0x" + ((int)value).ToString("X" + schemaColumn.Digits, CultureInfo.InvariantCulture) + desc;
                            }
                        }
                    }
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
                if (schemaColumn == null)
                {
                    return null;
                }

                var standardValues = schemaColumn.Enum;
                if (standardValues != null && standardValues.Length != 0)
                {
                    var values = standardValues.Select(sv => Convert.ChangeType(sv.Value, context.PropertyDescriptor.PropertyType));
                    if (schemaColumn.AllowBlanks)
                    {
                        values = Enumerable.Concat(new object[] { TwoDADocument.BlankEntry }, values);
                    }
                    return new StandardValuesCollection(values.ToArray());
                }

                return null;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);
                if (schemaColumn == null)
                {
                    return false;
                }

                var standardValues = schemaColumn.Enum;
                if (standardValues != null && standardValues.Length != 0)
                {
                    return true;
                }

                return false;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            private static IEnumerable<DataRow> GetReferencedRows(ITypeDescriptorContext context, out DataColumn keyColumn)
            {
                var column = TwoDARowTypeDescriptor.GetColumn(context);
                var schemaColumn = TwoDARowTypeDescriptor.GetSchemaColumn(context);

                var rowSource = schemaColumn.RowSource;
                if (rowSource != null)
                {
                    var doc = (TwoDADocument)column.Table.ExtendedProperties[typeof(TwoDADocument)];
                    var rowSourceDoc = MainWindow.Instance.GetReferencedTwoDADocument(doc, rowSource);
                    if (rowSourceDoc != null && rowSourceDoc.Data.Columns.Contains(schemaColumn.RowSourceKeyField))
                    {
                        var keyColumn2 = keyColumn = rowSourceDoc.Data.Columns[schemaColumn.RowSourceKeyField];
                        return from row in rowSourceDoc.Data.AsEnumerable()
                               where row.RowState != DataRowState.Deleted && row[keyColumn2] != DBNull.Value
                               select row;
                    }
                }

                keyColumn = null;
                return null;
            }

            private static string RowReferenceToString(DataRow row, DataColumn keyColumn)
            {
                var rowKey = row[keyColumn];
                var rowLabel = row.Field<string>(1);
                return
                    rowLabel == null ?
                    rowKey.ToString() :
                    rowKey + "\u2002\u2013\u2002" + rowLabel;
            }

            private static string StrReferenceToString(int strref, TlkEntry entry)
            {
                return
                    entry.Text == null ?
                    strref.ToString() :
                    strref + "\u2002\u2013\u2002" + entry.Text;
            }
        }

        public class ColumnPropertyDescriptor : PropertyDescriptor
        {
            public ColumnPropertyDescriptor(DataColumn column, TwoDASchema.Column schemaColumn)
                : base(column.ColumnName, GetColumnAttributes(column, schemaColumn))
            {
                Column = column;
                SchemaColumn = schemaColumn;
            }

            private DataColumn Column { get; set; }

            private TwoDASchema.Column SchemaColumn { get; set; }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override Type ComponentType
            {
                get { return typeof(DataRow); }
            }

            public override object GetValue(object component)
            {
                var row = (DataRow)component;
                if (row.RowState == DataRowState.Deleted)
                {
                    return row[Column, DataRowVersion.Original];
                }
                else
                {
                    return row[Column];
                }
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(object); }
            }

            public override void ResetValue(object component)
            {
                var row = (DataRow)component;
                row[Column] = DBNull.Value;
            }

            public override void SetValue(object component, object value)
            {
                var row = (DataRow)component;
                row[Column] = value;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            protected override void FillAttributes(IList attributeList)
            {
                foreach (var attr in GetColumnAttributes(Column, SchemaColumn))
                {
                    attributeList.Add(attr);
                }
            }

            private static Attribute[] GetColumnAttributes(DataColumn column, TwoDASchema.Column schemaColumn)
            {
                var attributes =
                    new List<Attribute>
                    {
                        new TypeConverterAttribute(typeof(ColumnTypeConverter)),
                        new CategoryAttribute("Fields")
                    };

                if (column.Ordinal == 0)
                {
                    attributes.Add(new ParenthesizePropertyNameAttribute(true));
                    attributes.Add(new ReadOnlyAttribute(true));
                    attributes.Add(new DescriptionAttribute("Number of the row in the 2DA."));
                }

                if (schemaColumn != null)
                {
                    if (schemaColumn.Description != null)
                    {
                        attributes.Add(new DescriptionAttribute(schemaColumn.Description));
                    }

                    if (schemaColumn.DataType == TwoDASchema.DataType.HexInteger &&
                        schemaColumn.Flags.Length != 0)
                    {
                        attributes.Add(new EditorAttribute(typeof(HexNumberEditor), typeof(UITypeEditor)));
                    }

                    if (schemaColumn.RowSource != null)
                    {
                        attributes.Add(new EditorAttribute(typeof(RowRefEditor), typeof(UITypeEditor)));
                    }

                    if (schemaColumn.DataType == TwoDASchema.DataType.StrRef)
                    {
                        attributes.Add(new EditorAttribute(typeof(StrRefEditor), typeof(UITypeEditor)));
                    }
                }

                return attributes.ToArray();
            }
        }

        private static readonly AttributeCollection attributes = new AttributeCollection();
        private static readonly EventDescriptorCollection events = new EventDescriptorCollection(new EventDescriptor[0], true);

        private readonly DataRow row;
        private readonly TwoDADocument doc;
        private readonly PropertyDescriptorCollection properties;

        public TwoDARowTypeDescriptor(DataRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException("row");
            }

            this.row = row;

            doc = (TwoDADocument)row.Table.ExtendedProperties[typeof(TwoDADocument)];

            var properties = new List<PropertyDescriptor>();
            foreach (DataColumn column in row.Table.Columns)
            {
                TwoDASchema.Column schemaColumn = null;
                if (doc.Schema != null)
                {
                    schemaColumn = doc.Schema.Columns.FirstOrDefault(sc => sc.Name == column.ColumnName);
                }

                var property = new ColumnPropertyDescriptor(column, schemaColumn);
                properties.Add(property);
            }

            this.properties = new PropertyDescriptorCollection(properties.ToArray(), true);
        }

        public DataRow Row
        {
            get { return row; }
        }

        public TwoDADocument Document
        {
            get { return doc; }
        }

        public AttributeCollection GetAttributes()
        {
            return attributes;
        }

        public string GetClassName()
        {
            return null;
        }

        public string GetComponentName()
        {
            return null;
        }

        public TypeConverter GetConverter()
        {
            return null;
        }

        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        public object GetEditor(Type editorBaseType)
        {
            return null;
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return events;
        }

        public EventDescriptorCollection GetEvents()
        {
            return events;
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return properties;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return properties;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return row;
        }

        public static DataColumn GetColumn(ITypeDescriptorContext context)
        {
            var desc = GetTwoDARowTypeDescriptor(context);
            if (desc != null)
            {
                return desc.Document.Data.Columns[context.PropertyDescriptor.Name];
            }

            return null;
        }

        public static TwoDASchema.Column GetSchemaColumn(ITypeDescriptorContext context)
        {
            var desc = GetTwoDARowTypeDescriptor(context);
            if (desc != null)
            {
                var col = desc.Document.Data.Columns[context.PropertyDescriptor.Name];
                return desc.Document.SchemaColumns[col.Ordinal];
            }

            return null;
        }

        public static TwoDARowTypeDescriptor GetTwoDARowTypeDescriptor(ITypeDescriptorContext context)
        {
            var desc = context.Instance as TwoDARowTypeDescriptor;

            if (desc == null)
            {
                var descs = context.Instance as IEnumerable<TwoDARowTypeDescriptor>;
                if (descs != null)
                {
                    var docs = descs.Select(d => d.Document).Distinct();
                    if (docs.Count() == 1)
                    {
                        desc = descs.First();
                    }
                }
            }

            return desc;
        }
    }
}

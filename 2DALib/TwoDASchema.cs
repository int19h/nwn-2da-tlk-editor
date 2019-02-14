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
using System.Xml.Serialization;
using System.Xml.Schema;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Int19h.NeverwinterNights.TwoDA
{
    /// <summary>
    /// An in-memory representation of a .2daschema file, loaded via <see cref="XmlSerializer"/>.
    /// For detailed description of the schema, refer to the corresponding xs:documentation
    /// elements in 2daschema.xsd.
    /// </summary>
    [XmlRoot(Namespace = TwoDASchema.XmlNamespace)]
    public class TwoDASchema
    {
        public enum DataType
        {
            String,
            Integer,
            HexInteger,
            Float,
            StrRef
        }

        public class Column
        {
            public class Flag
            {
                private string value;

                [XmlText]
                public string Name { get; set; }

                [XmlAttribute]
                public string Value
                {
                    get
                    {
                        return value;
                    }
                    set
                    {
                        this.value = value;

                        // Even though the schema restricts it to hexadecimal only for now,
                        // handle decimal here as well (why not? we do it elsewhere...).
                        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            Bits = int.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        }
                        else
                        {
                            Bits = int.Parse(value);
                        }
                    }
                }

                /// <summary>
                /// Gets or sets the actual integer representation of the flag.
                /// </summary>
                /// <value>The integer representation of the flag.</value>
                [XmlIgnore]
                public int Bits { get; private set; }
            }

            public class Item
            {
                [XmlText]
                public string Name { get; set; }

                [XmlAttribute]
                public string Value { get; set; }
            }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            [DefaultValue(DataType.String)]
            public DataType DataType { get; set; }

            [XmlAttribute]
            [DefaultValue(0)]
            public int Digits { get; set; }

            [XmlAttribute]
            [DefaultValue(true)]
            public bool AllowBlanks { get; set; }

            [XmlAttribute]
            public string MinValue { get; set; }

            [XmlAttribute]
            public string MaxValue { get; set; }

            [XmlAttribute]
            public string RowSource { get; set; }

            [XmlAttribute]
            [DefaultValue("#")]
            public string RowSourceKeyField { get; set; }

            [XmlElement]
            public string Description { get; set; }

            [XmlArrayItem("Item", Namespace = TwoDASchema.XmlNamespace)]
            public Item[] Enum { get; set; }

            [XmlArrayItem("Flag", Namespace = TwoDASchema.XmlNamespace)]
            public Flag[] Flags { get; set; }

            public Column()
            {
                DataType = DataType.String;
                AllowBlanks = true;
                RowSourceKeyField = "#";
            }
        }

        public const string XmlNamespace = "http://int19h.org/xsd/2daschema.xsd";

        [XmlAttribute]
        public string DisplayName { get; set; }

        [XmlAttribute]
        public string AppliesTo
        {
            get { return AppliesToRegex.ToString(); }
            set { AppliesToRegex = new Regex(value, RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.IgnoreCase); }
        }

        [XmlElement("Column", Namespace = TwoDASchema.XmlNamespace)]
        public Column[] Columns { get; set; }

        /// <summary>
        /// Gets or sets the regex compiled from <see cref="AppliedTo"/>.
        /// </summary>
        /// <value>The regex compiled from <see cref="AppliedTo"/>.</value>
        [XmlIgnore]
        public Regex AppliesToRegex { get; set; }
    }
}

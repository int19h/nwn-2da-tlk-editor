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
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    static class TwoDASchemaRegistry
    {
        private const string schemasPath = "Schemas";

        public static ReadOnlyCollection<TwoDASchema> Schemas { get; private set; }

        static TwoDASchemaRegistry()
        {
            var schemas = new List<TwoDASchema>();
            var serializer = new XmlSerializer(typeof(TwoDASchema));

            string schemasFullPath =
                Path.Combine(
                    Path.GetDirectoryName(Application.ExecutablePath),
                    schemasPath);
            Trace.WriteLine("Loading 2DA schemas from " + schemasFullPath);
            if (Directory.Exists(schemasFullPath))
            {
                int schemaCount = 0;
                foreach (string fileName in Directory.GetFiles(schemasFullPath))
                {
                    Trace.WriteLine("Loading schema " + fileName);
                    using (var reader = XmlReader.Create(fileName))
                    {
                        try
                        {
                            var schema = (TwoDASchema)serializer.Deserialize(reader);
                            schemas.Add(schema);
                            ++schemaCount;
                        }
                        catch (InvalidOperationException ex)
                        {
                            Trace.WriteLine("Error loading schema: " + ex.Message);
                        }
                    }
                }
                Trace.WriteLine(schemaCount + " schemas loaded successfully");
            }
            else
            {
                Trace.WriteLine("Directory does not exist: " + schemasFullPath);
            }

            Schemas = new ReadOnlyCollection<TwoDASchema>(schemas);
        }

        public static TwoDASchema GetMatchingSchema(string fileName)
        {
            return Schemas.FirstOrDefault(schema => schema.AppliesToRegex.IsMatch(fileName));
        }
    }
}

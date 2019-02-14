/*
 * Neverwinter Nights 2DA/TLK Editor.
 * Copyright (C) 2008-2009 Pavel Minaev
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
using System.IO;
using System.Globalization;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public static class CSV
    {
        public static object[,] Parse(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            var values = new List<object>();
            int columnCount = 0, rowCount = 0;

            for (int lineColumnCount = 0; ; )
            {
                var value = new StringBuilder();

                int c = reader.Read();
                if (c == '\0')
                {
                    break;
                }

                if (c != '"')
                {
                    while (c >= 0 && c != ',' && c != '\r' && c != '\n')
                    {
                        value.Append((char)c);
                        c = reader.Read();
                    }
                }
                else
                {
                    for (; ; )
                    {
                        c = reader.Read();

                        if (c < 0)
                        {
                            break;
                        }
                        else if (c == '"')
                        {
                            c = reader.Read();
                            if (c != '"')
                            {
                                break;
                            }
                        }

                        value.Append((char)c);
                    }
                }

                values.Add(value.ToString());
                if (++lineColumnCount > columnCount)
                {
                    columnCount = lineColumnCount;
                }

                if (c == '\r' && reader.Peek() == '\n')
                {
                    c = reader.Read();
                }

                if (c == '\n')
                {
                    ++rowCount;
                    lineColumnCount = 0;
                    values.Add(null);
                }
                else if (c < 0)
                {
                    if (columnCount != 0)
                    {
                        ++rowCount;
                    }
                    break;
                }
            }

            var result = new object[rowCount, columnCount];
            int row = 0, col = 0;
            foreach (var value in values)
            {
                if (value == null)
                {
                    ++row;
                    col = 0;
                }
                else
                {
                    result[row, col++] = value;
                }
            }

            return result;
        }

        public static void Write(object[,] data, TextWriter writer)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            for (int row = 0; row < data.GetLength(0); ++row)
            {
                if (row > 0)
                {
                    writer.WriteLine();
                }

                for (int col = 0; col < data.GetLength(1); ++col)
                {
                    if (col > 0)
                    {
                        writer.Write(',');
                    }

                    writer.Write('"');
                    foreach (char c in Convert.ToString(data[row, col], CultureInfo.InvariantCulture))
                    {
                        writer.Write(c);
                        if (c == '"')
                        {
                            writer.Write('"');
                        }
                    }
                    writer.Write('"');
                }
            }
        }
    }
}

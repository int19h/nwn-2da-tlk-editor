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
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using Int19h.NeverwinterNights.TwoDATlkEditor.Properties;
using Microsoft.Win32;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    internal static class Program
    {
        [STAThread]
        internal static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Settings.Default.FirstRun)
            {
                string path = Convert.ToString(Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\BioWare\NWN\Neverwinter", "Path", null));
                if (path != null)
                {
                    Settings.Default.TwoDAPath = Path.Combine(path, "override");
                    Settings.Default.TlkPath = Path.Combine(path, "tlk");
                }

                Settings.Default.FirstRun = false;
                Settings.Default.Save();
            }

            var window = MainWindow.Instance;
            if (args != null)
            {
                window.OpenFiles(args);
            }

            Application.Run(window);

            Settings.Default.Save();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            for (int i = 1; i < int.MaxValue; ++i)
            {
                string errorInfoFileName = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    string.Format(Application.ProductName + " - error log #{0}.txt", i));
                if (!File.Exists(errorInfoFileName))
                {
                    File.WriteAllText(errorInfoFileName, e.ExceptionObject.ToString());

                    string text =
                        Application.ProductName + " has encountered a fatal error. " +
                        "Details of this error have been written to the following file: \r\n\r\n" +
                        errorInfoFileName + "\r\n\r\n" +
                        "When reporting this problem to the developer, please attach this " +
                        "error information file to your error report to minimize the time " +
                        "required to resolve the problem.";
                    MessageBox.Show(text, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }
        }
    }
}

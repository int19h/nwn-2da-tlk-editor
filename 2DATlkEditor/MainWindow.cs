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
using System.IO;
using Int19h.NeverwinterNights.TwoDATlkEditor.Properties;
using Int19h.NeverwinterNights.Tlk;
using Int19h.NeverwinterNights.TwoDA;

namespace Int19h.NeverwinterNights.TwoDATlkEditor
{
    public partial class MainWindow : Form
    {
        public static readonly MainWindow Instance = new MainWindow();

        private class OpenFileProgressState
        {
            public string FileName { get; set; }
            public object Document { get; set; }
        }

        static MainWindow()
        {
        }

        public MainWindow()
        {
            if (Instance != null)
            {
                throw new InvalidOperationException("There can be only one instance of MainWindow");
            }

            InitializeComponent();
        }

        private void RegisterDocumentWindow(DocumentWindow window)
        {
            mdiToolStrip.Visible = true;

            var mdiButton = new ToolStripButton(window.Text);
            var mdiSeparator = new ToolStripSeparator();
            mdiButton.Tag = window;
            mdiButton.ToolTipText = window.FileName;
            mdiButton.Image = new Icon(window.Icon, new Size(16, 16)).ToBitmap();
            mdiButton.Padding = new Padding(6, 2, 2, 6);
            mdiButton.Click +=
                delegate
                {
                    if (window.WindowState == FormWindowState.Minimized)
                    {
                        window.WindowState = FormWindowState.Normal;
                    }
                    window.Activate();
                };

            mdiToolStrip.Items.Add(mdiButton);
            mdiToolStrip.Items.Add(mdiSeparator);

            window.TextChanged += delegate { mdiButton.Text = window.Text; };
            window.Closed +=
                delegate
                {
                    mdiToolStrip.Items.Remove(mdiButton);
                    mdiToolStrip.Items.Remove(mdiSeparator);

                    if (mdiToolStrip.Items.Count == 0)
                    {
                        mdiToolStrip.Visible = false;
                    }
                };
            window.Activated +=
                delegate
                {
                    foreach (var button in mdiToolStrip.Items.OfType<ToolStripButton>())
                    {
                        button.Checked = (button == mdiButton);
                    }
                };
        }

        public TwoDADocument GetReferencedTwoDADocument(TwoDADocument referringDocument, string rowSource)
        {
            var twoDAWindows = MdiChildren.OfType<TwoDADocumentWindow>();

            string docFileName = (from dw in twoDAWindows
                                  where dw.Document == referringDocument
                                  select dw.FileName
                                 ).FirstOrDefault();
            if (docFileName == null)
            {
                return null;
            }

            var matchingWindows = (from dw in twoDAWindows
                                   where Path.GetFileName(dw.FileName) == rowSource
                                   select dw
                                  ).ToArray();
            if (matchingWindows.Length == 0)
            {
                return null;
            }

            if (matchingWindows.Length > 1)
            {
                var docDirName = Path.GetDirectoryName(docFileName);

                var sameDirWindow = matchingWindows.FirstOrDefault(dw => Path.GetDirectoryName(dw.FileName) == docDirName);
                if (sameDirWindow != null)
                {
                    return sameDirWindow.Document;
                }

                var stdDirName = Path.GetFullPath(Settings.Default.TwoDAPath);
                var stdDirWindow = matchingWindows.FirstOrDefault(dw => Path.GetDirectoryName(dw.FileName) == stdDirName);
                if (stdDirWindow != null)
                {
                    return stdDirWindow.Document;
                }

            }

            return matchingWindows.First().Document;
        }

        public TlkDocument GetReferencedTlkDocument(TwoDADocument referringDocument, bool alternate)
        {
            string docFileName = (from dw in MdiChildren.OfType<TwoDADocumentWindow>()
                                  where dw.Document == referringDocument
                                  select dw.FileName
                                 ).FirstOrDefault();
            if (docFileName == null)
            {
                return null;
            }

            var matchingWindows = (from dw in MdiChildren.OfType<TlkDocumentWindow>()
                                   let fileName = Path.GetFileName(dw.FileName)
                                   let std = "dialog.tlk".Equals(fileName, StringComparison.OrdinalIgnoreCase) ||
                                             "dialogf.tlk".Equals(fileName, StringComparison.OrdinalIgnoreCase)
                                   where std ^ alternate
                                   select dw
                                  ).ToArray();
            if (matchingWindows.Length == 0)
            {
                return null;
            }

            if (matchingWindows.Length > 1)
            {
                var docDirName = Path.GetDirectoryName(docFileName);

                var sameDirWindow = matchingWindows.FirstOrDefault(dw => Path.GetDirectoryName(dw.FileName) == docDirName);
                if (sameDirWindow != null)
                {
                    return sameDirWindow.Document;
                }

                var stdDirName = Path.GetFullPath(Settings.Default.TlkPath);
                var stdDirWindow = matchingWindows.FirstOrDefault(dw => Path.GetDirectoryName(dw.FileName) == stdDirName);
                if (stdDirWindow != null)
                {
                    return stdDirWindow.Document;
                }

            }

            return matchingWindows.First().Document;
        }

        public void OpenFiles(string[] fileNames)
        {
            var fileList = new List<string>(fileNames);

            var documentWindows = MdiChildren.OfType<DocumentWindow>();

            for (int i = 0; i < fileList.Count; ++i)
            {
                var fileName = fileList[i];
                var extension = Path.GetExtension(fileName).ToLowerInvariant();

                var existingWindow = documentWindows.FirstOrDefault(
                    dw => dw.FileName == fileName);
                if (existingWindow != null)
                {
                    existingWindow.Activate();
                    fileList.RemoveAt(i);
                    --i;
                }

                if (extension == ".2da" && Settings.Default.AutoLoad2DA)
                {
                    var schema = TwoDASchemaRegistry.GetMatchingSchema(Path.GetFileName(fileName));
                    if (schema != null && schema.Columns != null)
                    {
                        foreach (var schemaColumn in schema.Columns)
                        {
                            var rowSource = schemaColumn.RowSource;
                            if (rowSource != null)
                            {
                                if (!documentWindows.Any(dw => Path.GetFileName(dw.FileName) == rowSource) &&
                                    !fileList.Any(f => Path.GetFileName(f) == rowSource))
                                {
                                    string rowSourceFileName = Path.Combine(
                                        Path.GetDirectoryName(fileName),
                                        rowSource);
                                    if (!File.Exists(rowSourceFileName) && Directory.Exists(Settings.Default.TwoDAPath))
                                    {
                                        rowSourceFileName = Path.Combine(Settings.Default.TwoDAPath, rowSource);
                                    }
                                    if (File.Exists(rowSourceFileName))
                                    {
                                        fileList.Insert(0, rowSourceFileName);
                                        ++i;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            openFilesBackgroundWorker.RunWorkerAsync(fileList);
        }

        private TwoDADocument LoadTwoDADocument(string fileName)
        {
            var schema = TwoDASchemaRegistry.GetMatchingSchema(Path.GetFileName(fileName));

            int lineCount = 0;
            var buffer = new MemoryStream();
            using (var reader = new StreamReader(fileName))
            {
                var writer = new StreamWriter(buffer);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                    ++lineCount;
                }

                writer.Flush();
            }

            buffer.Position = 0;
            using (var reader = new StreamReader(buffer))
            {
                var doc = new TwoDADocument(schema);
                doc.Load(
                    reader,
                    lineNumber =>
                    {
                        if (lineNumber % 100 == 0)
                        {
                            openFilesBackgroundWorker.ReportProgress(
                                (int)Math.Round(lineNumber * 100m / lineCount),
                                new OpenFileProgressState
                                {
                                    FileName = fileName,
                                    Document = null
                                });
                        }
                    });
                return doc;
            }
        }

        private TlkDocument LoadTlkDocument(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            using (var reader = new BinaryReader(stream))
            {
                var doc = new TlkDocument();
                doc.Load(
                    reader,
                    (current, total) =>
                    {
                        if (current % 100 == 0)
                        {
                            openFilesBackgroundWorker.ReportProgress(
                                (int)Math.Round(current * 100m / total),
                                new OpenFileProgressState
                                {
                                    FileName = fileName,
                                    Document = null
                                });
                        }
                    });
                return doc;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenFiles(openFileDialog.FileNames);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        private void openFilesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var fileNames = (IEnumerable<string>)e.Argument;
            foreach (string fileName in fileNames)
            {
                if (openFilesBackgroundWorker.CancellationPending)
                {
                    return;
                }

                string extension = Path.GetExtension(fileName).ToLowerInvariant();

                do
                {
                    object document = null;
                    string errorMessage = null;

                    try
                    {
                        switch (extension)
                        {
                            case ".2da":
                                document = LoadTwoDADocument(fileName);
                                break;

                            case ".tlk":
                                document = LoadTlkDocument(fileName);
                                break;
                        }
                    }
                    catch (IOException ex)
                    {
                        errorMessage = string.Format(
                            "Error opening file '{0}'.\r\n\r\n{1}",
                            fileName,
                            ex.Message);
                    }
                    catch (InvalidDataException ex)
                    {
                        errorMessage = string.Format(
                            "File '{0}' is not a valid {1} file.\r\n\r\n{2}",
                            fileName,
                            extension,
                            ex.Message);
                    }

                    if (document != null)
                    {
                        openFilesBackgroundWorker.ReportProgress(
                            100,
                            new OpenFileProgressState
                            {
                                FileName = fileName,
                                Document = document
                            });
                    }

                    if (errorMessage != null)
                    {
                        var button = default(DialogResult);
                        Invoke(
                            (MethodInvoker)delegate
                            {
                                button = MessageBox.Show(
                                    this,
                                    errorMessage,
                                    "Error",
                                    MessageBoxButtons.AbortRetryIgnore,
                                    MessageBoxIcon.Error);
                            });
                        switch (button)
                        {
                            case DialogResult.Abort:
                                return;

                            case DialogResult.Retry:
                                continue;

                            case DialogResult.Ignore:
                                break;
                        }
                    }

                    break;
                } while (true);
            }
        }

        private void openFilesBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (OpenFileProgressState)e.UserState;

            var oldValue = toolStripProgressBar.Value;
            var oldText = toolStripStatusLabel.Text;

            var newValue = e.ProgressPercentage;
            var newText = "Loading file " + state.FileName;

            bool needRefresh = false;

            if (oldValue != newValue)
            {
                toolStripProgressBar.Visible = true;
                toolStripProgressBar.Value = e.ProgressPercentage;
                needRefresh = true;
            }

            if (oldText != newText)
            {
                toolStripStatusLabel.Text = newText;
                needRefresh = true;
            }

            if (needRefresh)
            {
                statusStrip.Refresh();
            }

            if (state.Document != null)
            {
                DocumentWindow window = null; ;
                if (state.Document is TwoDADocument)
                {
                    var twoDAWindow = new TwoDADocumentWindow();
                    twoDAWindow.Document = (TwoDADocument)state.Document;
                    window = twoDAWindow;
                }
                else
                {
                    var tlkWindow = new TlkDocumentWindow();
                    tlkWindow.Document = (TlkDocument)state.Document;
                    window = tlkWindow;
                }

                if (window != null)
                {
                    window.MdiParent = this;
                    window.FileName = state.FileName;
                    window.WindowState = FormWindowState.Maximized;
                    RegisterDocumentWindow(window);
                    window.Show();
                }
            }
        }

        private void openFilesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar.Visible = false;
            toolStripStatusLabel.Text = "Ready";
        }

        private void tileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void tileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OptionsDialog().ShowDialog(this);
        }
    }
}

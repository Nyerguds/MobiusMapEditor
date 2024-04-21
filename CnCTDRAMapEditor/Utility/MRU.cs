//
// Copyright 2020 Electronic Arts Inc.
//
// The Command & Conquer Map Editor and corresponding source code is free
// software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
//
// The Command & Conquer Map Editor and corresponding source code is distributed
// in the hope that it will be useful, but with permitted additional restrictions
// under Section 7 of the GPL. See the GNU General Public License in LICENSE.TXT
// distributed with this program. You should have received a copy of the
// GNU General Public License along with permitted additional restrictions
// with this program. If not, see https://github.com/electronicarts/CnC_Remastered_Collection
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MobiusEditor.Utility
{
    /// <summary>
    /// System to keep track of Most Recently Used files.
    /// </summary>
    public class MRU: IDisposable
    {
        private bool _disposed;
        private readonly RegistryKey registryKey;
        private readonly int maxFiles;
        private readonly List<string> files = new List<string>();
        private readonly ToolStripMenuItem menu;
        private readonly ToolStripMenuItem[] fileItems;

        public string[] Files => files.ToArray();
        public bool Disposed => _disposed;

        public event EventHandler<string> FileSelected;

        public MRU(string registryPath, int maxFiles, ToolStripMenuItem menu)
        {
            RegistryKey subKey = Registry.CurrentUser;
            foreach (var key in registryPath.Split('\\'))
            {
                RegistryKey oldKey = subKey;
                subKey = subKey.CreateSubKey(key, true);
                try { oldKey.Dispose(); }
                catch { /* ignore */ }
            }
            registryKey = subKey.CreateSubKey("MRU");
            this.maxFiles = maxFiles;
            this.menu = menu;
            this.menu.DropDownItems.Clear();
            fileItems = new ToolStripMenuItem[maxFiles];
            for (var i = 0; i < fileItems.Length; ++i)
            {
                var fileItem = fileItems[i] = new ToolStripMenuItem();
                fileItem.Visible = false;
                menu.DropDownItems.Add(fileItem);
            }
            LoadMRU();
            ShowMRU();
        }

        /// <summary>Adds a path to the MRU.</summary>
        /// <param name="file">Path to add.</param>
        /// <exception cref="ObjectDisposedException">The MRU is disposed.</exception>
        public void Add(string file)
        {
            if (_disposed)
                throw new ObjectDisposedException("MRU");
            files.RemoveAll(f => f == file);
            files.Insert(0, file);

            if (files.Count > maxFiles)
            {
                files.RemoveAt(files.Count - 1);
            }
            SaveMRU();
            ShowMRU();
        }

        /// <summary>Removes a path from the MRU.</summary>
        /// <param name="file">Path to add.</param>
        /// <exception cref="ObjectDisposedException">The MRU is disposed.</exception>
        public void Remove(string file)
        {
            if (_disposed)
                throw new ObjectDisposedException("MRU");
            files.RemoveAll(f => f == file);
            SaveMRU();
            ShowMRU();
        }

        /// <summary>Checks if an MRU path exists, either as mix path or as file path.</summary>
        /// <param name="path">A mix path or file path from the MRU to check.</param>
        /// <returns>True if it is a file path and the file exists, or it is a mix path and all components in the mix path exist.</returns>
        public static bool CheckIfExist(string path)
        {
            bool isMix = path.IndexOf('?') != -1;
            return (isMix && MixPath.PathIsValid(path)) || (!isMix && File.Exists(path));
        }

        /// <summary>Gets the FileInfo for this path. For mix paths, this uses the path of the mix archive.</summary>
        /// <param name="path">A mix path or file path from the MRU to get the base file info for.</param>
        /// <returns></returns>
        public static FileInfo GetBaseFileInfo(string path)
        {
            bool isMix = path.IndexOf('?') != -1;
            if (isMix)
            {
                MixPath.GetComponents(path, out string[] mixParts, out _);
                path = mixParts[0];
            }
            return new FileInfo(path);
        }

        private void LoadMRU()
        {
            files.Clear();
            for (var i = 0; i < maxFiles; ++i)
            {
                string fileName = registryKey.GetValue(i.ToString()) as string;
                if (!string.IsNullOrEmpty(fileName))
                {
                    files.Add(fileName);
                }
            }
        }

        private void SaveMRU()
        {
            for (var i = 0; i < files.Count; ++i)
            {
                registryKey.SetValue(i.ToString(), files[i]);
            }
            for (var i = files.Count; i < maxFiles; ++i)
            {
                registryKey.DeleteValue(i.ToString(), false);
            }
        }

        private void ShowMRU()
        {
            for (var i = 0; i < files.Count; ++i)
            {
                var file = files[i];
                var fileItem = fileItems[i];
                bool isMix = file.IndexOf('?') != -1;
                string fileText = isMix ? MixPath.GetFileNameReadable(file, false, out _) : file;
                fileItem.Text = string.Format("&{0} {1}", i + 1, fileText.Replace("&", "&&"));
                fileItem.Tag = file;
                fileItem.Click -= FileItem_Click;
                fileItem.Click += FileItem_Click;
                fileItem.Visible = true;
            }
            for (var i = files.Count; i < maxFiles; ++i)
            {
                var fileItem = fileItems[i];
                fileItem.Visible = false;
                fileItem.Click -= FileItem_Click;
            }
            menu.Enabled = files.Count > 0;
        }

        private void FileItem_Click(object sender, EventArgs e)
        {
            FileSelected?.Invoke(this, (sender as ToolStripMenuItem).Tag as string);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try { registryKey.Dispose(); }
                catch { /* ignore */ }
                _disposed = true;
            }
        }
    }
}

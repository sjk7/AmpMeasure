using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain
    {
        private void AddToMRU(string filePath)
        {
            try
            {
                var files = new List<string>();
                if (!string.IsNullOrEmpty(Properties.Settings.Default.MRUList))
                {
                    files.AddRange(Properties.Settings.Default.MRUList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                }

                files.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                files.Insert(0, filePath);
                
                if (files.Count > 10)
                {
                    files = files.Take(10).ToList();
                }

                Properties.Settings.Default.MRUList = string.Join("|", files);
                Properties.Settings.Default.Save();
                
                UpdateMRUMenu();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update MRU list: {ex.Message}");
            }
        }

        private void RemoveFromMRU(string filePath)
        {
            var files = new List<string>();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.MRUList))
            {
                files.AddRange(Properties.Settings.Default.MRUList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                if (files.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase)) > 0)
                {
                    Properties.Settings.Default.MRUList = string.Join("|", files);
                    Properties.Settings.Default.Save();
                    UpdateMRUMenu();
                }
            }
        }

        private void UpdateMRUMenu()
        {
            recentFilesToolStripMenuItem.DropDownItems.Clear();
            
            if (string.IsNullOrEmpty(Properties.Settings.Default.MRUList))
            {
                recentFilesToolStripMenuItem.Enabled = false;
                return;
            }

            var files = Properties.Settings.Default.MRUList.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (files.Length == 0)
            {
                recentFilesToolStripMenuItem.Enabled = false;
                return;
            }

            recentFilesToolStripMenuItem.Enabled = true;
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                var item = new ToolStripMenuItem($"{i + 1}. {Path.GetFileName(file)}");
                item.Tag = file;
                item.Click += async (s, e) => {
                    if (CheckUnsavedChanges()) await LoadWorksheetAsync((string)((ToolStripMenuItem)s).Tag);
                };
                recentFilesToolStripMenuItem.DropDownItems.Add(item);
            }
        }
    }
}

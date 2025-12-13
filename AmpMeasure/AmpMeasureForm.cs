// This file was previously named AmpMeasureForm.Main.cs.
// To ensure the WinForms designer works, rename this file to AmpMeasureForm.cs in Solution Explorer.
// After renaming, ensure all partial class files use the same class and namespace.
// No code changes are needed in this file.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace AmpMeasure
{
    public partial class AmpMeasureMain : Form
    {
        private bool _isLoadingColumns = false;
        private bool _isLoadingWindow = false;
        private bool _isCalculating = false;
        private bool _isDirty = false;
        private string _currentFilePath = null;
        private double _defaultSupplyVoltage = 50;
        private double _defaultLoadResistance = 50;
        private bool _isPropagatingValues = false;
        private PerformanceProfiler _profiler = new PerformanceProfiler(true);

        [DllImport("user32.dll", CharSet = CharSet.Auto)] 
        extern static bool DestroyIcon(IntPtr handle);

        public AmpMeasureMain()
        {
            InitializeComponent();
            this.SuspendLayout(); // Suspend layout as early as possible

            // Only run at runtime, not in the designer
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
                return;

            // Detect Windows dark mode on first run (if settings are default)
            if (!Properties.Settings.Default.Properties["DarkMode"].DefaultValue.Equals(Properties.Settings.Default.DarkMode))
            {
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize"))
                    {
                        if (key != null)
                        {
                            object appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                            if (appsUseLightTheme != null && Convert.ToInt32(appsUseLightTheme) == 0)
                            {
                                Properties.Settings.Default.DarkMode = true;
                                Properties.Settings.Default.Save();
                            }
                        }
                    }
                }
                catch { /* Ignore errors, fallback to default */ }
            }

            EnableColumnSorting();
            this.dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnHeaderMouseClick;

            // All runtime logic below this point
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);
            this.UpdateStyles();

            _defaultSupplyVoltage = Properties.Settings.Default.DefaultSupplyVoltage;
            _defaultLoadResistance = Properties.Settings.Default.DefaultLoadResistance;

            darkModeToolStripMenuItem.Checked = Properties.Settings.Default.DarkMode;
            if (Properties.Settings.Default.DarkMode)
            {
                ApplyTheme(true); // Apply dark theme as early as possible
            }

            SetAppIcon();
            SetDefaultFormSize();
            InitializeGrid();
            SetupColumnIcons();
            SetupMenuIcons();
            LoadColumnOrder();
            LoadColumnWidths();
            LoadWindowSettings();
            UpdateMRUMenu();
            UpdateTitle();

            // Hide debug menu items on production machines
            HideDebugMenuItems();

            // Add keyboard shortcut for new entry
            this.KeyPreview = true;
            this.KeyDown += AmpMeasureMain_KeyDown;

            _isDirty = false;

            string lastWorksheet = Properties.Settings.Default.LastWorksheetPath;
            if (!string.IsNullOrEmpty(lastWorksheet) && File.Exists(lastWorksheet))
            {
                this.Shown += async (s, e) => { await LoadWorksheetAsync(lastWorksheet); };
            }
            else
            {
                // Only show one initial row if no file is loaded
                AddInitialRows(1);
                // Set focus to the first editable cell (Peak Volts)
                this.Shown += (s, e) =>
                {
                    if (dataGridView1.Rows.Count > 0 && dataGridView1.Columns.Count > 0)
                    {
                        dataGridView1.CurrentCell = dataGridView1[0, 0];
                        dataGridView1.BeginEdit(true);
                    }
                };
            }

            this.ResumeLayout(true); // Resume layout after all theming and initialization

            dataGridView1.Refresh();

            // Log app startup only in debug mode
            if (IsDebugMode())
            {
                _profiler.StartTimer("App Startup");
                _profiler.StopTimer("App Startup");
                _profiler.LogToFile($"Application Started - {_virtualDataStore.Count} initial rows");
            }
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath);
            this.Text = $"Amp Measure - {fileName}" + (_isDirty ? "*" : "");
        }

        private void SetBusy(bool busy, string message = "")
        {
            if (busy)
            {
                Cursor.Current = Cursors.WaitCursor;
                menuStrip1.Enabled = false;
                dataGridView1.Enabled = false;
                toolStripStatusLabel1.Text = message;
            }
            else
            {
                Cursor.Current = Cursors.Default;
                menuStrip1.Enabled = true;
                dataGridView1.Enabled = true;
                toolStripStatusLabel1.Text = "Ready";
            }
        }

        private List<int> _rowOriginalOrder;

        private void EnableColumnSorting()
        {
            // Store the original order for stable sorting
            _rowOriginalOrder = Enumerable.Range(0, _virtualDataStore.Count).ToList();
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
        }

        private void UpdateSortGlyph()
        {
            // Remove glyphs from all columns
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            // Set glyph for sorted column
            if (_lastSortColumn >= 0 && _lastSortColumn < dataGridView1.Columns.Count)
            {
                dataGridView1.Columns[_lastSortColumn].HeaderCell.SortGlyphDirection =
                    _lastSortAscending ? SortOrder.Ascending : SortOrder.Descending;
            }
        }

        private int _lastSortColumn = -1;
        private bool _lastSortAscending = true;

        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= dataGridView1.Columns.Count)
                return;

            // Ensure _rowOriginalOrder is in sync with _virtualDataStore
            if (_rowOriginalOrder == null || _rowOriginalOrder.Count != _virtualDataStore.Count)
            {
                _rowOriginalOrder = Enumerable.Range(0, _virtualDataStore.Count).ToList();
            }

            // Toggle sort direction if same column, otherwise default to ascending
            if (_lastSortColumn == e.ColumnIndex)
                _lastSortAscending = !_lastSortAscending;
            else
                _lastSortAscending = true;
            _lastSortColumn = e.ColumnIndex;

            // Stable sort: pair each row with its original index
            var paired = _virtualDataStore
                .Select((row, idx) => new { Row = row, OrigIdx = _rowOriginalOrder[idx] })
                .ToList();

            paired.Sort((a, b) =>
            {
                string sa = a.Row[e.ColumnIndex] ?? string.Empty;
                string sb = b.Row[e.ColumnIndex] ?? string.Empty;
                double da, db;
                bool isNumA = double.TryParse(sa, out da);
                bool isNumB = double.TryParse(sb, out db);
                int cmp;
                if (isNumA && isNumB)
                    cmp = da.CompareTo(db);
                else
                    cmp = string.Compare(sa, sb, StringComparison.CurrentCultureIgnoreCase);
                if (cmp == 0)
                    cmp = a.OrigIdx.CompareTo(b.OrigIdx); // Stable: preserve previous order
                return _lastSortAscending ? cmp : -cmp;
            });

            // Update the data store and original order
            for (int i = 0; i < paired.Count; i++)
            {
                _virtualDataStore[i] = paired[i].Row;
                _rowOriginalOrder[i] = paired[i].OrigIdx;
            }

            UpdateSortGlyph();
            dataGridView1.Invalidate();
        }

        private void clearAllSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will reset all application settings to their defaults and restart the application. Continue?",
                "Reset All Settings",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Properties.Settings.Default.Reset();
                Properties.Settings.Default.Save();
                Application.Restart();
            }
        }

        private void simulateFirstRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will delete all application settings and MRU data. The next time the app starts, it will behave as if it was never run before. Continue?",
                "Simulate First Run",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                try
                {
                    // Remove all settings
                    Properties.Settings.Default.Reset();
                    Properties.Settings.Default.Save();
                    // Remove user config file manually
                    string company = Application.CompanyName;
                    string product = Application.ProductName;
                    string userConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string configDir = System.IO.Path.Combine(userConfigPath, company, product + ".exe_Url_");
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string roamingConfigDir = System.IO.Path.Combine(appData, company, product + ".exe_Url_");
                    // Delete all config files in both locations
                    if (System.IO.Directory.Exists(configDir))
                        System.IO.Directory.Delete(configDir, true);
                    if (System.IO.Directory.Exists(roamingConfigDir))
                        System.IO.Directory.Delete(roamingConfigDir, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Application.Restart();
            }
        }
    }
}

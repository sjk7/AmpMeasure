using System;
using System.Collections.Generic;
using System.Linq;

namespace AmpMeasure
{
    public partial class AmpMeasureMain
    {
        private void LoadColumnOrder()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ColumnOrder))
                return;

            try
            {
                _isLoadingColumns = true;

                var orderPairs = Properties.Settings.Default.ColumnOrder.Split(',');
                
                if (orderPairs.Length != dataGridView1.Columns.Count)
                {
                    ResetColumnOrderSettings();
                    return;
                }

                var orderMap = new Dictionary<string, int>();
                foreach (var pair in orderPairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int displayIndex))
                    {
                        orderMap[parts[0]] = displayIndex;
                    }
                    else
                    {
                        ResetColumnOrderSettings();
                        return;
                    }
                }

                foreach (System.Windows.Forms.DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (orderMap.ContainsKey(column.Name))
                    {
                        int newIndex = orderMap[column.Name];
                        if (newIndex >= 0 && newIndex < dataGridView1.Columns.Count)
                        {
                            column.DisplayIndex = newIndex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load column order: {ex.Message}");
                ResetColumnOrderSettings();
            }
            finally
            {
                _isLoadingColumns = false;
            }
        }

        private void LoadColumnWidths()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.ColumnWidths))
                return;

            try
            {
                _isLoadingColumns = true;
                
                var widths = Properties.Settings.Default.ColumnWidths.Split(',');
                
                if (widths.Length != dataGridView1.Columns.Count)
                {
                    ResetColumnWidthSettings();
                    return;
                }

                var parsedWidths = new List<int>();
                foreach (var widthStr in widths)
                {
                    if (!int.TryParse(widthStr, out int width) || width < 10 || width > 5000)
                    {
                        ResetColumnWidthSettings();
                        return;
                    }
                    parsedWidths.Add(width);
                }

                dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.None;
                for (int i = 0; i < parsedWidths.Count; i++)
                {
                    dataGridView1.Columns[i].Width = parsedWidths[i];
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load column widths: {ex.Message}");
                ResetColumnWidthSettings();
            }
            finally
            {
                _isLoadingColumns = false;
            }
        }

        private void SaveColumnWidths()
        {
            if (_isLoadingColumns)
                return;

            try
            {
                var widths = new List<string>();
                foreach (System.Windows.Forms.DataGridViewColumn column in dataGridView1.Columns)
                {
                    widths.Add(column.Width.ToString());
                }
                Properties.Settings.Default.ColumnWidths = string.Join(",", widths);
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save column widths: {ex.Message}");
            }
        }

        private void SaveColumnOrder()
        {
            if (_isLoadingColumns)
                return;

            try
            {
                var orderPairs = new List<string>();
                foreach (System.Windows.Forms.DataGridViewColumn column in dataGridView1.Columns)
                {
                    orderPairs.Add($"{column.Name}:{column.DisplayIndex}");
                }
                Properties.Settings.Default.ColumnOrder = string.Join(",", orderPairs);
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save column order: {ex.Message}");
            }
        }

        private void ResetColumnOrderSettings()
        {
            try
            {
                Properties.Settings.Default.ColumnOrder = string.Empty;
                Properties.Settings.Default.Save();
                
                colPeakVolts.DisplayIndex = 0;
                colSupplyCurrent.DisplayIndex = 1;
                colPowerOut.DisplayIndex = 2;
                colRMSVolts.DisplayIndex = 3;
                colSupplyVoltage.DisplayIndex = 4;
                colPowerIn.DisplayIndex = 5;
                colEfficiency.DisplayIndex = 6;
                colLoadResistance.DisplayIndex = 7;
                colDateTime.DisplayIndex = 8;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset column order: {ex.Message}");
            }
        }

        private void ResetColumnWidthSettings()
        {
            try
            {
                Properties.Settings.Default.ColumnWidths = string.Empty;
                Properties.Settings.Default.Save();
                dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset column widths: {ex.Message}");
            }
        }
    }
}

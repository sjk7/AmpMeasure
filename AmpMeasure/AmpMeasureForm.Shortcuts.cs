using System;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain : Form
    {
        private void AmpMeasureMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.N)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                JumpToNewEntry();
            }
        }

        private void JumpToNewEntry()
        {
            try
            {
                var newRow = new string[dataGridView1.Columns.Count];
                for (int i = 0; i < newRow.Length; i++)
                {
                    newRow[i] = "";
                }
                newRow[colSupplyVoltage.Index] = _defaultSupplyVoltage.ToString();
                newRow[colLoadResistance.Index] = _defaultLoadResistance.ToString();
                newRow[colDateTime.Index] = DateTime.Now.ToString("g");
                _virtualDataStore.Add(newRow);
                var tags = new System.Collections.Generic.Dictionary<string, string>();
                tags["colSupplyVoltage"] = "default";
                tags["colLoadResistance"] = "default";
                _virtualCellTags[_virtualDataStore.Count - 1] = tags;
                SetVirtualRowCount(_virtualDataStore.Count);
                int newRowIndex = _virtualDataStore.Count - 1;
                dataGridView1.CurrentCell = dataGridView1[colPeakVolts.Index, newRowIndex];
                dataGridView1.BeginEdit(true);
                _isDirty = true;
                UpdateTitle();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JumpToNewEntry failed: {ex.Message}");
            }
        }
    }
}

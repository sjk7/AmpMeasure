using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain
    {
        private void CalculateRow(int rowIndex)
        {
            _isCalculating = true;
            try
            {
                var row = dataGridView1.Rows[rowIndex];
                
                if (!double.TryParse(Convert.ToString(row.Cells["colPeakVolts"].Value), out double peakVolts)) peakVolts = 0;
                if (!double.TryParse(Convert.ToString(row.Cells["colSupplyCurrent"].Value), out double supplyCurrent)) supplyCurrent = 0;
                if (!double.TryParse(Convert.ToString(row.Cells["colSupplyVoltage"].Value), out double supplyVoltage)) supplyVoltage = 0;
                if (!double.TryParse(Convert.ToString(row.Cells["colLoadResistance"].Value), out double loadResistance)) loadResistance = 0;
                if (!double.TryParse(Convert.ToString(row.Cells["colPowerOut"].Value), out double powerOut)) powerOut = 0;

                double rmsVolts = peakVolts * 0.70710678;
                row.Cells["colRMSVolts"].Value = Math.Round(rmsVolts, 3);

                double powerIn = supplyVoltage * supplyCurrent;
                row.Cells["colPowerIn"].Value = Math.Round(powerIn, 3);

                double efficiency = 0;
                if (powerIn != 0)
                {
                    efficiency = (powerOut / powerIn) * 100;
                }
                row.Cells["colEfficiency"].Value = Math.Round(efficiency, 2);
            }
            finally
            {
                _isCalculating = false;
            }
        }

        private WorksheetData GetWorksheetData()
        {
            var data = new WorksheetData();
            
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                data.Columns.Add(new WorksheetColumn
                {
                    Name = col.Name,
                    HeaderText = col.HeaderText,
                    Width = col.Width,
                    DisplayIndex = col.DisplayIndex
                });
            }

            int totalRows = _virtualDataStore.Count;
            int columnCount = _virtualDataStore.Count > 0 ? _virtualDataStore[0].Length : 0;
            
            // Pre-allocate row list capacity
            data.Rows = new List<List<string>>(totalRows);
            
            for (int i = 0; i < totalRows; i++)
            {
                var virtualRow = _virtualDataStore[i];
                var rowData = new List<string>(columnCount); // Pre-allocate column capacity
                
                for (int j = 0; j < virtualRow.Length; j++)
                {
                    rowData.Add(virtualRow[j] ?? "");
                }
                
                data.Rows.Add(rowData);
                
                if (totalRows > 1000 && i % 2000 == 0 && i > 0)
                {
                    SetBusy(true, $"Preparing data: {i:N0} / {totalRows:N0}...");
                }
            }
            
            return data;
        }
    }

    [Serializable]
    public class WorksheetData
    {
        public List<WorksheetColumn> Columns { get; set; }
        public List<List<string>> Rows { get; set; }

        public WorksheetData()
        {
            Columns = new List<WorksheetColumn>();
            Rows = new List<List<string>>();
        }
    }

    [Serializable]
    public class WorksheetColumn
    {
        public string Name { get; set; }
        public string HeaderText { get; set; }
        public int Width { get; set; }
        public int DisplayIndex { get; set; }
    }
}

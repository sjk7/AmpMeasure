using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain
    {
        private void InitializeGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.RowTemplate.Height = 25;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            colRMSVolts.ReadOnly = true;
            colPowerIn.ReadOnly = true;
            colEfficiency.ReadOnly = true;

            EnableVirtualMode();

            dataGridView1.ColumnWidthChanged += DataGridView1_ColumnWidthChanged;
            dataGridView1.ColumnDisplayIndexChanged += DataGridView1_ColumnDisplayIndexChanged;
            dataGridView1.CellPainting += DataGridView1_CellPainting;
        }

        private void SetupColumnIcons()
        {
            SetColumnHeaderWithIcon(colPeakVolts, "Peak Volts", ColumnIconHelper.CreatePeakVoltsIcon());
            SetColumnHeaderWithIcon(colRMSVolts, "RMS Volts", ColumnIconHelper.CreateRMSVoltsIcon());
            SetColumnHeaderWithIcon(colSupplyCurrent, "Supply Current", ColumnIconHelper.CreateCurrentIcon());
            SetColumnHeaderWithIcon(colSupplyVoltage, "Supply Voltage", ColumnIconHelper.CreateVoltageIcon());
            SetColumnHeaderWithIcon(colPowerIn, "Power In", ColumnIconHelper.CreatePowerInIcon());
            SetColumnHeaderWithIcon(colPowerOut, "Power Out", ColumnIconHelper.CreatePowerOutIcon());
            SetColumnHeaderWithIcon(colEfficiency, "Efficiency", ColumnIconHelper.CreateEfficiencyIcon());
            SetColumnHeaderWithIcon(colLoadResistance, "Load Resistance", ColumnIconHelper.CreateResistanceIcon());
            SetColumnHeaderWithIcon(colDateTime, "Date/Time", ColumnIconHelper.CreateDateTimeIcon());
        }

        private void SetColumnHeaderWithIcon(DataGridViewColumn column, string text, Bitmap icon)
        {
            column.HeaderText = " " + text;
            
            if (dataGridView1.ColumnHeadersDefaultCellStyle.Padding == Padding.Empty)
            {
                dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(20, 0, 0, 0);
            }
            
            dataGridView1.Paint += (sender, e) =>
            {
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    Rectangle rect = dataGridView1.GetCellDisplayRectangle(col.Index, -1, true);
                    if (rect.Width > 0)
                    {
                        Bitmap colIcon = null;
                        if (col == colPeakVolts) colIcon = ColumnIconHelper.CreatePeakVoltsIcon();
                        else if (col == colRMSVolts) colIcon = ColumnIconHelper.CreateRMSVoltsIcon();
                        else if (col == colSupplyCurrent) colIcon = ColumnIconHelper.CreateCurrentIcon();
                        else if (col == colSupplyVoltage) colIcon = ColumnIconHelper.CreateVoltageIcon();
                        else if (col == colPowerIn) colIcon = ColumnIconHelper.CreatePowerInIcon();
                        else if (col == colPowerOut) colIcon = ColumnIconHelper.CreatePowerOutIcon();
                        else if (col == colEfficiency) colIcon = ColumnIconHelper.CreateEfficiencyIcon();
                        else if (col == colLoadResistance) colIcon = ColumnIconHelper.CreateResistanceIcon();
                        else if (col == colDateTime) colIcon = ColumnIconHelper.CreateDateTimeIcon();

                        if (colIcon != null)
                        {
                            e.Graphics.DrawImage(colIcon, rect.Left + 2, rect.Top + (rect.Height - 16) / 2, 16, 16);
                            colIcon.Dispose();
                        }
                    }
                }
            };
        }

        private void AddInitialRows(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var rowData = new string[dataGridView1.Columns.Count];
                
                for (int j = 0; j < rowData.Length; j++)
                {
                    rowData[j] = "";
                }
                
                rowData[colSupplyVoltage.Index] = _defaultSupplyVoltage.ToString();
                rowData[colLoadResistance.Index] = _defaultLoadResistance.ToString();
                rowData[colDateTime.Index] = DateTime.Now.ToString("g");
                
                _virtualDataStore.Add(rowData);
                
                var tags = new Dictionary<string, string>();
                tags["colSupplyVoltage"] = "default";
                tags["colLoadResistance"] = "default";
                _virtualCellTags[i] = tags;
            }
            
            SetVirtualRowCount(_virtualDataStore.Count);
        }

        private void DataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == colEfficiency.Index && e.RowIndex >= 0)
            {
                if (double.TryParse(Convert.ToString(e.Value), out double efficiency) && efficiency > 0)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);

                    int padding = 3;
                    int barHeight = 4;
                    int barWidth = e.CellBounds.Width - (padding * 2);
                    int barX = e.CellBounds.X + padding;
                    int barY = e.CellBounds.Y + e.CellBounds.Height - barHeight - padding;

                    Color barColor;
                    if (efficiency <= 20)
                        barColor = Color.FromArgb(220, 50, 50);
                    else if (efficiency <= 40)
                        barColor = Color.FromArgb(255, 140, 0);
                    else
                        barColor = Color.FromArgb(50, 180, 50);

                    using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                    {
                        e.Graphics.FillRectangle(bgBrush, barX, barY, barWidth, barHeight);
                    }

                    double cappedEfficiency = Math.Min(efficiency, 100);
                    int filledWidth = (int)(barWidth * (cappedEfficiency / 100.0));

                    using (SolidBrush fillBrush = new SolidBrush(barColor))
                    {
                        if (filledWidth > 0)
                        {
                            e.Graphics.FillRectangle(fillBrush, barX, barY, filledWidth, barHeight);
                        }
                    }

                    if (e.Value != null)
                    {
                        TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                        Rectangle textRect = new Rectangle(e.CellBounds.X, e.CellBounds.Y, 
                            e.CellBounds.Width, e.CellBounds.Height - barHeight - padding);
                        TextRenderer.DrawText(e.Graphics, e.Value.ToString(), e.CellStyle.Font, 
                            textRect, e.CellStyle.ForeColor, flags);
                    }

                    e.Handled = true;
                }
            }
        }

        private void DataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (dataGridView1.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.None && !_isLoadingColumns)
            {
                SaveColumnWidths();
            }
        }

        private void DataGridView1_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (!_isLoadingColumns)
            {
                SaveColumnOrder();
            }
        }
    }
}

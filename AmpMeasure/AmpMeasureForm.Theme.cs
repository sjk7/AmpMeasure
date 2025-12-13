using System;
using System.Drawing;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain
    {
        private void ApplyTheme(bool darkMode)
        {
            if (darkMode)
            {
                Color darkBackground = Color.FromArgb(45, 45, 48);
                Color darkForeground = Color.FromArgb(241, 241, 241);
                Color darkGridBackground = Color.FromArgb(37, 37, 38);
                Color darkGridLines = Color.FromArgb(62, 62, 66);
                Color darkHeaderBackground = Color.FromArgb(51, 51, 55);
                Color darkMenuBackground = Color.FromArgb(45, 45, 48);

                this.BackColor = darkBackground;
                this.ForeColor = darkForeground;

                dataGridView1.BackgroundColor = darkGridBackground;
                dataGridView1.GridColor = darkGridLines;
                dataGridView1.DefaultCellStyle.BackColor = darkGridBackground;
                dataGridView1.DefaultCellStyle.ForeColor = darkForeground;
                dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(51, 153, 255);
                dataGridView1.DefaultCellStyle.SelectionForeColor = darkForeground;
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(42, 42, 45);
                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = darkHeaderBackground;
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = darkForeground;
                dataGridView1.RowHeadersDefaultCellStyle.BackColor = darkHeaderBackground;
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor = darkForeground;
                dataGridView1.EnableHeadersVisualStyles = false;

                menuStrip1.BackColor = darkMenuBackground;
                menuStrip1.ForeColor = darkForeground;
                menuStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
                
                ApplyMenuItemColors(menuStrip1.Items, darkForeground, darkMenuBackground);

                statusStrip1.BackColor = darkMenuBackground;
                statusStrip1.ForeColor = darkForeground;
                statusStrip1.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            }
            else
            {
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;

                dataGridView1.BackgroundColor = SystemColors.AppWorkspace;
                dataGridView1.GridColor = SystemColors.ControlDark;
                dataGridView1.DefaultCellStyle.BackColor = SystemColors.Window;
                dataGridView1.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                dataGridView1.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                dataGridView1.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
                dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = SystemColors.Window;
                dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                dataGridView1.RowHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                dataGridView1.RowHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
                dataGridView1.EnableHeadersVisualStyles = true;

                menuStrip1.BackColor = SystemColors.MenuBar;
                menuStrip1.ForeColor = SystemColors.MenuText;
                menuStrip1.Renderer = new ToolStripProfessionalRenderer();
                
                ApplyMenuItemColors(menuStrip1.Items, SystemColors.MenuText, SystemColors.Menu);

                statusStrip1.BackColor = SystemColors.Control;
                statusStrip1.ForeColor = SystemColors.ControlText;
                statusStrip1.Renderer = new ToolStripProfessionalRenderer();
            }

            dataGridView1.Refresh();
        }

        private void ApplyMenuItemColors(ToolStripItemCollection items, Color foreColor, Color backColor)
        {
            foreach (ToolStripItem item in items)
            {
                item.ForeColor = foreColor;
                item.BackColor = backColor;
                
                if (item is ToolStripMenuItem menuItem && menuItem.HasDropDownItems)
                {
                    ApplyMenuItemColors(menuItem.DropDownItems, foreColor, backColor);
                }
            }
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(62, 62, 66);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(62, 62, 66);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(51, 51, 55);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(51, 51, 55);
        public override Color MenuItemBorder => Color.FromArgb(62, 62, 66);
        public override Color MenuBorder => Color.FromArgb(62, 62, 66);
        public override Color MenuItemPressedGradientMiddle => Color.FromArgb(51, 51, 55);
        public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientBegin => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientMiddle => Color.FromArgb(45, 45, 48);
        public override Color ToolStripGradientEnd => Color.FromArgb(45, 45, 48);
        public override Color SeparatorDark => Color.FromArgb(62, 62, 66);
        public override Color SeparatorLight => Color.FromArgb(62, 62, 66);
    }
}

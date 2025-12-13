using System;
using System.Windows.Forms;

namespace AmpMeasure
{
    public partial class AmpMeasureMain : Form
    {
        private bool IsDebugMode()
        {
            string machineName = Environment.MachineName.ToUpper();
            string userName = Environment.UserName.ToUpper();
            return machineName.Contains("DESKTOP") || machineName.Contains("DEV") || 
                   userName.Contains("ADMIN") || userName.Contains("DEVELOPER");
        }

        private void HideDebugMenuItems()
        {
            if (!IsDebugMode())
            {
                generateTestDataToolStripMenuItem.Visible = false;
                viewPerformanceLogToolStripMenuItem.Visible = false;
                toolStripSeparator5.Visible = false;
                toolStripSeparator6.Visible = false;
            }
        }
    }
}

using ReaLTaiizor.Controls;
using ReaLTaiizor.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MPOGPUFIX_Service_Configurator
{
    public static class WMIFix
    {
        public static bool Notice()
        {
            if (DialogResult.OK == MaterialMessageBox.Show("WMI Has caused an error! Would you like to attempt to fix it?", "Error Detected :(", false, MaterialFlexibleForm.ButtonsPosition.Right))
            {
                Process.Start("https://www.thewindowsclub.com/how-to-repair-or-rebuild-the-wmi-repository-on-windows-10");
                return true;
            }
            else
                return false;
        }
    }
}

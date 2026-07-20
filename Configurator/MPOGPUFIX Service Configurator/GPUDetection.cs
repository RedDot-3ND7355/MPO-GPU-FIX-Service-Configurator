using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Policy;
using System.Text;

namespace MPOGPUFIX_Service_Configurator
{
    public static class GPUDetection
    {

        private static ManagementObjectSearcher drvsearcher;

        public static bool IsAMDGPU()
        {
            drvsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            ManagementObjectCollection items = drvsearcher.Get();
            // GPU Name & Driver
            if (items != null)
            {
                string GPUName = "";
                string GPUVersion = "";
                foreach (ManagementObject mo in items)
                {
                    if (mo != null)
                    {
                        foreach (PropertyData property in mo.Properties)
                        {
                            if (property != null)
                            {
                                // Get GPUName 
                                if (property.Name == "Name")
                                    GPUName = property.Value.ToString();
                                // Get GPUVersion
                                if (property.Name == "DriverVersion")
                                    GPUVersion = property.Value.ToString();
                                // Validate APU, Status and GPU
                                if (VerifyifAPU(GPUName) || (property.Name == "ConfigManagerErrorCode" && property.Value.ToString() == "22") || (property.Name == "Availability" && property.Value.ToString() == "8") || (property.Name == "PNPDeviceID" && !property.Value.ToString().Contains("PCI")))
                                {
                                    GPUName = "";
                                    break;
                                }
                            }
                        }
                        if (GPUName.Length > 0)
                            break;
                    }
                }
                // Detect if AMD for additional settings
                if (GPUName.Contains("AMD") || GPUName.Contains("Radeon") || GPUName.Contains("Vega") || GPUName.Contains("Advanced Micro Devices"))
                    return true;
                else
                    return false;
            }
            else // RESTORE WMI
            {
                if (WMIFix.Notice())
                {
                    MaterialMessageBox.Show("Don't forget to reboot to apply changes after fixing your WMI Repository!");
                    Application.Exit();
                }
                else
                {
                    MaterialMessageBox.Show("This app requires WMI Repository to work. Cancelled, closing...");
                    Application.Exit();
                }
            }
            return false;
        }

        //
        // Begin Verification of APU
        //
        static string[] blacklistedgpunames = { "HD Graphics", "UHD Graphics", "RX Vega Graphics", "AMD Radeon(TM) Graphics", "Radeon Graphics", "Radeon(TM) Graphics" };
        static string[] whitelist = { "56", "64" };
        static private bool VerifyifAPU(string gpuname) // true IF apu
        {
            foreach (string name in blacklistedgpunames)
            {
                if (!CheckWhiteList(gpuname) && gpuname.Contains(name))
                    return true;
            }
            return false;
        }

        static private bool CheckWhiteList(string wlitem) // false IF not GPU whitelist
        {
            bool triggered = false;
            foreach (string name in whitelist)
                if (wlitem.Contains(name))
                    triggered = true;
            if (triggered)
                return true;
            return false;
        }
        //
        // End Verification of APU
        //
    }
}

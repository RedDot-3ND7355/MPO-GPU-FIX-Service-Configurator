using Microsoft.Win32;

namespace MPOGPUFIX_Service_Configurator
{
    public static class DWMHandler
    {
        public static void MPOGPUFix(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.overlayTestMode = enable ? "5" : "";
            SettingsHandler.WriteSettings();
        }

        public static void OverlayMinFPSFix(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.overlayMinFPS = enable ? "0" : "";
            SettingsHandler.WriteSettings();
        }

    }
}

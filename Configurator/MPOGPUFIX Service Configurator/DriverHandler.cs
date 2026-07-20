namespace MPOGPUFIX_Service_Configurator
{
    public static class DriverHandler
    {
        public static void TDRLevelDropDown(string level)
        {
            if (!Form1.AppStarted) return;
            string ConvertedValue = "";
            switch (level)
            {
                case "TDRLevel (None)":
                    ConvertedValue = "";
                    break;
                case "TdrLevelOff":
                    ConvertedValue = "0";
                    break;
                case "TdrLevelBugcheck":
                    ConvertedValue = "1";
                    break;
                case "TdrLevelRecoveryVGA":
                    ConvertedValue = "2";
                    break;
                case "TdrLevelRecover (Def)":
                    ConvertedValue = "3";
                    break;
            }
            SettingsHandler.tdrLevel = ConvertedValue;
            SettingsHandler.WriteSettings();
        }

        public static void TDRDelayFix(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.tdrDelay = enable ? "10" : "5";
            SettingsHandler.WriteSettings();
        }

        public static void HAGSFix(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.hags = enable ? "1" : "2";
            SettingsHandler.WriteSettings();
        }

        public static void DisableOverlays(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.disableOverlays = enable ? "1" : "";
            SettingsHandler.WriteSettings();
        }
    }
}

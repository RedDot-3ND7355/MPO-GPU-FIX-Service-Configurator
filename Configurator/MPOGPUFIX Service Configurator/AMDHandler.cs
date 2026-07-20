namespace MPOGPUFIX_Service_Configurator
{
    public static class AMDHandler
    {
        public static void AMDULPS(bool enable)
        {
            if (!Form1.AppStarted) return;
            SettingsHandler.enableUlps = enable ? "1" : "0";
            SettingsHandler.WriteSettings();
        }

        public static void ShaderCacheDropDown(string shaderCache)
        {
            if (!Form1.AppStarted) return;
            string ConvertedValue = "";
            switch (shaderCache)
            {
                case "ON":
                    ConvertedValue = "32 00";
                    break;
                case "AMD OPTIMIZED":
                    ConvertedValue = "31 00";
                    break;
                case "OFF":
                    ConvertedValue = "30 00";
                    break;
                default:
                    ConvertedValue = "31 00";
                    break;
            }
            SettingsHandler.shaderCache = ConvertedValue;
            SettingsHandler.WriteSettings();
        }
    }
}

using System.Xml;

namespace MPOGPUFIX_Service_Configurator
{
    public static class SettingsHandler
    {
        // Get the path to the App.config file via service name
        private static string configPath = "App.config";

        public static void WriteSettings()
        {
            string fullPath = Path.Combine(ServiceHandler.GetServiceExecutablePath("MPOGPUFIX Service"), configPath);
            if (!File.Exists(fullPath)) { return; }

            // Code to write settings to an XML file
            var doc = new XmlDocument();
            doc.Load(fullPath);
            var nodes = doc.SelectNodes("/configuration/appSettings/add");
            SetValue(nodes, "ShaderCache", shaderCache);
            SetValue(nodes, "EnableUlps", enableUlps);
            SetValue(nodes, "OverlayTestMode", overlayTestMode);
            SetValue(nodes, "TdrDelay", tdrDelay);
            SetValue(nodes, "TdrLevel", tdrLevel);
            SetValue(nodes, "HwSchMode", hags);
            SetValue(nodes, "OverlayMinFPS", overlayMinFPS);
            SetValue(nodes, "DisableOverlays", disableOverlays);
            SetValue(nodes, "TimerInterval", timerInt.ToString());
            doc.Save(fullPath);
        }

        private static void SetValue(XmlNodeList? nodes, string v, string shaderCache)
        {
            if (nodes == null) return;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes?["key"]?.Value == v)
                {
                    node.Attributes?["value"]?.Value = shaderCache;
                    break;
                }
            }
        }

        public static void ReadSettings()
        {
            string tmp = ServiceHandler.GetServiceExecutablePath("MPOGPUFIX Service");
            string fullPath = Path.Combine(ServiceHandler.GetServiceExecutablePath("MPOGPUFIX Service"), configPath);
            if (!File.Exists(fullPath)) { return; }

            // Code to read settings from an XML file
            var doc = new XmlDocument();
            doc.Load(fullPath);
            var nodes = doc.SelectNodes("/configuration/appSettings/add");
            shaderCache = GetValue(nodes, "ShaderCache");
            enableUlps = GetValue(nodes, "EnableUlps");
            overlayTestMode = GetValue(nodes, "OverlayTestMode");
            tdrDelay = GetValue(nodes, "TdrDelay");
            tdrLevel = GetValue(nodes, "TdrLevel");
            hags = GetValue(nodes, "HwSchMode");
            overlayMinFPS = GetValue(nodes, "OverlayMinFPS");
            disableOverlays = GetValue(nodes, "DisableOverlays");
            timerInt = int.Parse(GetValue(nodes, "TimerInterval"));
        }

        private static string GetValue(XmlNodeList? nodes, string v)
        {
            // Get the value of a specific key from the XML nodes
            if (nodes == null) return string.Empty;
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes?["key"]?.Value == v)
                {
                    return node.Attributes?["value"]?.Value ?? string.Empty;
                }
            }
            return string.Empty;
        }

        // Settings variables (default values)
        public static int timerInt = 3600000;
        public static string shaderCache = "31 00";
        public static string enableUlps = "1";
        public static string overlayTestMode = "";
        public static string tdrDelay = "5";
        public static string tdrLevel = "";
        public static string hags = "2";
        public static string overlayMinFPS = "";
        public static string disableOverlays = "";
    }
}

/* App.config file structure: 
 * 
 * 
 * <?xml version="1.0" encoding="utf-8"?>
<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7.2" />
  </startup>
  <appSettings>
    <add key="TimerInterval" value="3600000" />
    <!-- Interval for Service in MS -->
    <add key="ShaderCache" value="31 00" />
    <!-- 32 00 = ON | 31 00 = AMD Optimized | 30 00 == OFF | NO VALUE (default) -->
    <add key="EnableUlps" value="1" />
    <!-- 0 = disable ULPS | 1 = enable ULPS (default) -->
    <add key="OverlayTestMode" value="5" />
    <!-- 5 = MPO fix ON | NO VALUE (default) -->
    <add key="TdrDelay" value="10" />
    <!-- 10 = 10 seconds of TDR delay | 5 = 5 seconds of TDR delay (default) -->
    <add key="TdrLevel" value="0" />
    <!-- 0 = TdrLevelOff | 1 = TdrLevelBugcheck | 2 = TdrLevelRecoveryVGA | 3 = TdrLevelRecover | NO VALUE (default)-->
    <add key="HwSchMode" value="2" />
    <!-- 1 = HAGS Fix enabled | 2 = HAGS Fix disabled (default) -->
    <add key="OverlayMinFPS" value="0" />
    <!-- 0 = Unlimited FPS for Overlays | NO VALUE (default) -->
    <add key="DisableOverlays" value="0" />
    <!-- 1 = Disable Overlays | NO VALUE (default) -->
    <add key="ClientSettingsProvider.ServiceUri" value="" />
  </appSettings>
  <system.web>
    <membership defaultProvider="ClientAuthenticationMembershipProvider">
      <providers>
        <add name="ClientAuthenticationMembershipProvider" type="System.Web.ClientServices.Providers.ClientFormsAuthenticationMembershipProvider, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" serviceUri="" />
      </providers>
    </membership>
    <roleManager defaultProvider="ClientRoleProvider" enabled="true">
      <providers>
        <add name="ClientRoleProvider" type="System.Web.ClientServices.Providers.ClientRoleProvider, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" serviceUri="" cacheTimeout="86400" />
      </providers>
    </roleManager>
  </system.web>
</configuration>
 * 
 * 
 */
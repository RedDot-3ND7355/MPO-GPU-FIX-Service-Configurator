using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceProcess;
using System.Xml;

namespace MPOGPUFIX_Service
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer;
        private int timerInt = 86400000; // 1 Day
        private readonly string configPath;
        private readonly string logPath;
        private bool isAMD = false;

        public Service1()
        {
            InitializeComponent();
            configPath = AppDomain.CurrentDomain.BaseDirectory + "App.config";
            logPath = AppDomain.CurrentDomain.BaseDirectory + "service.log";
        }

        private void Log(string message, bool isError = false)
        {
            try
            {
                string prefix = isError ? "ERROR" : "INFO";
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now} | {prefix} | {message}\n");
            }
            catch { /* fail silently */ }
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Log("Service starting...");
                // Read App Config Variables
                ApplySettings();

                timer = new System.Timers.Timer(timerInt);
                timer.Elapsed += (s, e) => ApplySettings();
                timer.AutoReset = true;
                timer.Start();

                Log("Service started successfully.");
            } 
            catch (Exception ex) 
            {
                // Log error so we can see what failed
                Log($"CRITICAL ERROR on start: {ex}", true);
            } 
        }

        private string GetValue(XmlNodeList nodes, string key)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["key"]?.Value == key)
                    return node.Attributes["value"]?.Value;
            }
            return null;
        }

        private void ApplySettings()
        {
            try
            {
                if (!System.IO.File.Exists(configPath))
                {
                    Log("Config file not found.");
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(configPath);
                var nodes = doc.SelectNodes("/configuration/appSettings/add");
                // Read values from config
                string shaderCache = GetValue(nodes, "ShaderCache") ?? "";
                string enableUlps = GetValue(nodes, "EnableUlps") ?? "1";
                string overlayTestMode = GetValue(nodes, "OverlayTestMode") ?? "";
                string tdrDelay = GetValue(nodes, "TdrDelay") ?? "5";
                string tdrLevel = GetValue(nodes, "TdrLevel") ?? "";
                string hags = GetValue(nodes, "HwSchMode") ?? "2";
                string overlayMinFPS = GetValue(nodes, "OverlayMinFPS") ?? "";
                string disableOverlays = GetValue(nodes, "DisableOverlays") ?? "";

                int newInterval = int.TryParse(GetValue(nodes, "TimerInterval"), out int i) ? i : 86400000;

                // Update timer interval if it has changed
                if (newInterval != timerInt && timer != null)
                {
                    timerInt = newInterval;
                    timer?.Stop();
                    timer.Interval = newInterval;
                    timer.Start();
                    Log($"Timer interval updated to {timerInt}ms");
                }

                // AMD GPU profiles (ULPS + ShaderCache)
                ApplyAMDSettings(shaderCache, enableUlps);
                // GraphicsDrivers keys
                ApplyGraphicsDriversSettings(tdrDelay, tdrLevel, hags, disableOverlays);
                // DWM keys (OverlayTestMode + OverlayMinFPS)
                ApplyDWMSettings(overlayTestMode, overlayMinFPS);
                // Log of changes
                var appliedSettings = new List<string>();

                if (isAMD)
                {
                    appliedSettings.Add($"ShaderCache={shaderCache}");
                    appliedSettings.Add($"ULPS={enableUlps}");
                }

                if (!string.IsNullOrEmpty(overlayTestMode)) appliedSettings.Add($"OverlayTestMode={overlayTestMode}");
                if (!string.IsNullOrEmpty(tdrDelay)) appliedSettings.Add($"TdrDelay={tdrDelay}");
                if (!string.IsNullOrEmpty(tdrLevel)) appliedSettings.Add($"TdrLevel={tdrLevel}");
                if (!string.IsNullOrEmpty(hags)) appliedSettings.Add($"HAGS={hags}");
                if (!string.IsNullOrEmpty(overlayMinFPS)) appliedSettings.Add($"OverlayMinFPS={overlayMinFPS}");
                if (!string.IsNullOrEmpty(disableOverlays)) appliedSettings.Add($"DisableOverlays={disableOverlays}");

                string settingsLog = appliedSettings.Count > 0
                    ? string.Join(" | ", appliedSettings)
                    : "No active settings (defaults only)";

                string amdFlag = isAMD ? "[AMD Detected] " : "";
                Log($"Applied - {amdFlag}{settingsLog}");
            }
            catch (Exception ex)
            {
                Log($"ApplySettings error: {ex}", true);
            }
        }

        private void ApplyDWMSettings(string overlayTestMode, string overlayMinFPS)
        {
            using (var dwm = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\DWM", true))
            {
                if (dwm != null)
                {
                    if (!string.IsNullOrEmpty(overlayTestMode))
                        SetOrDelete(dwm, "OverlayTestMode", overlayTestMode);

                    if (!string.IsNullOrEmpty(overlayMinFPS))
                        SetOrDelete(dwm, "OverlayMinFPS", overlayMinFPS);
                }
            }
        }

        private void ApplyGraphicsDriversSettings(string tdrDelay, string tdrLevel, string hags, string disableOverlays)
        {
            using (var gfx = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", true))
            {
                if (gfx == null) return;
                gfx.SetValue("TdrDelay", int.Parse(tdrDelay ?? "5"), RegistryValueKind.DWord);
                SetOrDelete(gfx, "TdrLevel", tdrLevel);
                gfx.SetValue("HwSchMode", int.Parse(hags ?? "2"), RegistryValueKind.DWord);
                SetOrDelete(gfx, "DisableOverlays", disableOverlays);
            }
        }

        private void ApplyAMDSettings(string shaderCache, string enableUlps)
        {
            isAMD = false;
            string baseKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
            using (var root = Registry.LocalMachine.OpenSubKey(baseKey, true))
            {
                if (root != null)
                {
                    foreach (var sub in root.GetSubKeyNames().Where(s => s.Length == 4 && s.All(char.IsDigit)))
                    {
                        isAMD = true;
                        // ULPS
                        if (!string.IsNullOrEmpty(enableUlps))
                        {
                            using (var prof = root.OpenSubKey(sub, true))
                                prof?.SetValue("EnableUlps", int.Parse(enableUlps), RegistryValueKind.DWord);
                        }

                        // ShaderCache
                        if (!string.IsNullOrEmpty(shaderCache))
                        {
                            byte[] data = shaderCache.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
                            using (var umd = root.OpenSubKey(sub + @"\UMD", true))
                                umd?.SetValue("ShaderCache", data, RegistryValueKind.Binary);
                        }
                        else
                        {
                            using (var umd = root.OpenSubKey(sub + @"\UMD", true))
                                umd?.DeleteValue("ShaderCache", false);
                        }
                    }
                }
            }
        }

        private void SetOrDelete(RegistryKey key, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
                key.SetValue(name, int.Parse(value), RegistryValueKind.DWord);
            else
                key.DeleteValue(name, false);
        }

        protected override void OnStop()
        {
            timer?.Stop();
        }
    }
}

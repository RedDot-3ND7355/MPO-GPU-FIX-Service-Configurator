using ReaLTaiizor.Controls;
using System.ServiceProcess;

namespace MPOGPUFIX_Service_Configurator
{
    public partial class Form1 : Form
    {
        public static bool AppStarted = false;

        public Form1()
        {
            InitializeComponent();
            // Populate the Startup Type combobox
            LoadStartupTypes(materialComboBox4);
            // Load the configuration file and populate the UI with the current settings
            SettingsHandler.ReadSettings();
            UpdateControls();
            // Check if the service is installed/running/enabled and update the UI accordingly
            ServiceHandler.InitialDetection();
            // Apply to all buttons to ensure correct states on startup
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            // Check if the GPU is AMD and show/hide AMD-specific settings
            thunderGroupBox3.Enabled = GPUDetection.IsAMDGPU();
            // Ready State
            AppStarted = true;
        }

        // Update the UI controls based on the current settings
        private void UpdateControls()
        {
            // Graphics Driver Tweaks
            materialCheckBox5.Checked = SettingsHandler.disableOverlays == "1";
            materialCheckBox3.Checked = SettingsHandler.hags == "1";
            materialCheckBox2.Checked = SettingsHandler.tdrDelay == "10";
            SetComboIndex(materialComboBox3, MapTdrLevel(SettingsHandler.tdrLevel));
            // AMD Driver Tweaks
            materialCheckBox9.Checked = SettingsHandler.enableUlps == "1";
            SetComboIndex(materialComboBox2, MapShaderCache(SettingsHandler.shaderCache));
            // Windows DWM Tweaks
            materialCheckBox1.Checked = SettingsHandler.overlayTestMode == "5";
            materialCheckBox4.Checked = SettingsHandler.overlayMinFPS == "0";
            // Interval Timer
            SetComboIndex(materialComboBox1, MapTimerInterval(SettingsHandler.timerInt));
        }

        private string MapTdrLevel(string? val) => val switch
        {
            "0" => "TdrLevelRecover (Def)",
            "1" => "TdrLevelBugCheck",
            "2" => "TdrLevelRecoverNoUnreg",
            "3" => "TdrLevelRecoverNoReset",
            _ => "TdrLevelRecover (Def)"
        };

        private string MapShaderCache(string? val) => val switch
        {
            "31 00" => "AMD OPTIMIZED",
            "32 00" => "ON",
            "30 00" => "OFF",
            _ => "AMD OPTIMIZED"
        };

        private string MapTimerInterval(int ms) => ms switch
        {
            86400000 => "24 Hours",
            43200000 => "12 Hours",
            21600000 => "6 Hours",
            3600000 => "1 Hours",
            1800000 => "30 Minutes",
            900000 => "15 Minutes",
            600000 => "10 Minutes",
            300000 => "5 Minutes",
            0 => "Only On Boot",
            _ => "24 Hours"  // default
        };

        static void SetComboIndex(ComboBox cmb, string value)
        {
            cmb.SelectedIndex = cmb.Items.Cast<object>().ToList().FindIndex(item =>
                item?.ToString() == value || (item is ServiceStartupObject s && s.Text == value));
        }

        private void UpdateStatusBarText()
        {
            foreverStatusBar1.RectColor = ServiceHandler.Current?.ServiceController != null ? (ServiceHandler.Current?.ServiceStatus == ServiceControllerStatus.Running ? Color.Green : Color.Red) : Color.Gray;
            foreverStatusBar1.Text = "Service Status: " + (ServiceHandler.Current?.ServiceController != null ? ServiceHandler.Current?.ServiceStatus.ToString() : "Not Installed") + " | Startup Type: " + (ServiceHandler.Current != null ? ServiceHandler.Current.StartupType.ToString() : "N/A") + (ServiceHandler.Current?.isDelayed == true ? "(Delayed)" : "");
        }

        // Interval Timer DropDown
        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialComboBox1.SelectedItem == null || !AppStarted) return;
            SettingsHandler.timerInt = GetTimerMsFromText(materialComboBox1.SelectedItem?.ToString() ?? "86400000");
            SettingsHandler.WriteSettings();
        }

        private int GetTimerMsFromText(string text) => text switch
        {
            "24 Hours" => 86400000,
            "12 Hours" => 43200000,
            "6 Hours" => 21600000,
            "1 Hours" => 3600000,
            "30 Minutes" => 1800000,
            "15 Minutes" => 900000,
            "10 Minutes" => 600000,
            "5 Minutes" => 300000,
            "Only On Boot" => 0,
            _ => 86400000
        };

        // Uninstall Service Button
        private void nightButton1_Click(object sender, EventArgs e)
        {
            ServiceHandler.UninstallService();
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        /*
         * 0 = Uninstall Service
         * 1 = Install Service
         * 2 = Start Service
         * 3 = Stop Service
         * 4 = Enable Service
         * 5 = Disable Service
         */

        // Install Service Button
        private void nightButton2_Click(object sender, EventArgs e)
        {
            ServiceHandler.InstallService();
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        // Stop Service Button
        private void nightButton3_Click(object sender, EventArgs e)
        {
            ServiceHandler.StopService();
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        // Start Service Button
        private void nightButton4_Click(object sender, EventArgs e)
        {
            ServiceHandler.StartService();
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        // Disable Service Button
        private void nightButton5_Click(object sender, EventArgs e)
        {
            ServiceHandler.DisableService(ServiceHandler.Current?.ServiceController?.ServiceName ?? string.Empty);
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        // Enable Service Button
        private void nightButton6_Click(object sender, EventArgs e)
        {
            if (materialComboBox4.SelectedValue == null)
                materialComboBox4.SelectedIndex = 0; // Default to first item if somehow null

            // Safely extract the startup mode without unboxing a null.
            object? selectedValue = materialComboBox4.SelectedValue ?? materialComboBox4.Items[materialComboBox4.SelectedIndex];
            ServiceStartMode mode = ServiceStartMode.Automatic;

            if (selectedValue is ServiceStartMode ssm)
            {
                mode = ssm;
            }
            else if (selectedValue != null)
            {
                // When using anonymous objects for Items, try to read the "Value" property.
                var prop = selectedValue.GetType().GetProperty("Value");
                var propVal = prop?.GetValue(selectedValue);
                if (propVal is ServiceStartMode pssm)
                    mode = pssm;
            }

            // Determine delayed if user selected the "Automatic (Delayed)" item (index 1).
            bool delayed = materialComboBox4.SelectedIndex == 1;

            ServiceHandler.EnableService(ServiceHandler.Current?.ServiceController?.ServiceName ?? string.Empty, mode, delayed);
            UpdateButtonStates(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
            UpdateButtonColorAccordingToEnabledState(nightButton1, nightButton2, nightButton3, nightButton4, nightButton5, nightButton6);
        }

        private void LoadStartupTypes(MaterialComboBox cmbStartup)
        {
            cmbStartup.Items.Clear();
            cmbStartup.Items.Add(new ServiceStartupObject { Text = "Automatic", Value = ServiceStartMode.Automatic });
            cmbStartup.Items.Add(new ServiceStartupObject { Text = "Automatic (Delayed)", Value = ServiceStartMode.Automatic });
            cmbStartup.Items.Add(new ServiceStartupObject { Text = "Manual", Value = ServiceStartMode.Manual });
            cmbStartup.Items.Add(new ServiceStartupObject { Text = "Disabled", Value = ServiceStartMode.Disabled });
            cmbStartup.DisplayMember = "Text";
            cmbStartup.ValueMember = "Value";
            // Reset index
            cmbStartup.SelectedIndex = 0;
        }

        public void UpdateButtonStates(ReaLTaiizor.Controls.Button btnUninstall, ReaLTaiizor.Controls.Button btnInstall, ReaLTaiizor.Controls.Button btnStop, ReaLTaiizor.Controls.Button btnStart, ReaLTaiizor.Controls.Button btnDisable, ReaLTaiizor.Controls.Button btnEnable)
        {
            // Update the status bar text with the current service status and startup type
            UpdateStatusBarText();
            if (ServiceHandler.Current?.ServiceController == null)
            {
                // Service not installed
                btnInstall.Enabled = true;
                btnUninstall.Enabled = false;
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnEnable.Enabled = false;
                btnDisable.Enabled = false;
                materialComboBox1.Enabled = false;
                materialComboBox4.Enabled = false;
                return;
            }

            //bool isInstalled = true;
            bool isRunning = ServiceHandler.Current.ServiceStatus == ServiceControllerStatus.Running;
            bool isDisabled = ServiceHandler.Current.StartupType == ServiceStartMode.Disabled;
            materialComboBox1.Enabled = true;
            materialComboBox4.Enabled = true;
            btnInstall.Enabled = false;
            btnUninstall.Enabled = true;
            btnStart.Enabled = !isRunning && !isDisabled;
            btnStop.Enabled = isRunning;
            btnEnable.Enabled = isDisabled;
            btnDisable.Enabled = !isDisabled;
        }

        private void UpdateButtonColorAccordingToEnabledState(ReaLTaiizor.Controls.Button btnUninstall, ReaLTaiizor.Controls.Button btnInstall, ReaLTaiizor.Controls.Button btnStop, ReaLTaiizor.Controls.Button btnStart, ReaLTaiizor.Controls.Button btnDisable, ReaLTaiizor.Controls.Button btnEnable)
        {
            btnUninstall.InactiveColor = btnUninstall.Enabled ? Color.FromArgb(46, 46, 46) : Color.Brown;
            btnInstall.InactiveColor = btnInstall.Enabled ? Color.FromArgb(46, 46, 46) : Color.DarkGreen;
            btnStop.InactiveColor = btnStop.Enabled ? Color.FromArgb(46, 46, 46) : Color.Brown;
            btnStart.InactiveColor = btnStart.Enabled ? Color.FromArgb(46, 46, 46) : Color.DarkGreen;
            btnDisable.InactiveColor = btnDisable.Enabled ? Color.FromArgb(46, 46, 46) : Color.Brown;
            btnEnable.InactiveColor = btnEnable.Enabled ? Color.FromArgb(46, 46, 46) : Color.DarkGreen;
        }

        // Switch the Startup type to the one selected in the combobox
        private void materialComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialComboBox4.SelectedItem != null && AppStarted)
            {
                string selectedText = ((ServiceStartupObject)materialComboBox4.SelectedItem).Text;
                ServiceStartMode selectedValue = ((ServiceStartupObject)materialComboBox4.SelectedItem).Value;
                ServiceHandler.ChangeStartupType(selectedValue, selectedText.Contains("Delayed"));
                UpdateStatusBarText();
            }
        }

        //
        // AMD Driver Tweaks
        //

        // ShaderCache DropDown
        private void materialComboBox2_SelectedIndexChanged(object sender, EventArgs e) =>
            AMDHandler.ShaderCacheDropDown(materialComboBox2.SelectedItem?.ToString() ?? "AMD OPTIMIZED");

        // AMD ULPS Checkbox
        private void materialCheckBox9_CheckedChanged(object sender, EventArgs e) =>
            AMDHandler.AMDULPS(materialCheckBox9.Checked);

        //
        // Windows DWM Tweaks
        //

        // MPO Fix Checkbox
        private void materialCheckBox1_CheckedChanged(object sender, EventArgs e) =>
            DWMHandler.MPOGPUFix(materialCheckBox1.Checked);

        // Overlay Min FPS Checkbox
        private void materialCheckBox4_CheckedChanged_1(object sender, EventArgs e) =>
            DWMHandler.OverlayMinFPSFix(materialCheckBox4.Checked);

        //
        // Graphics Driver Tweaks
        //

        // TDR Level DropDown
        private void materialComboBox3_SelectedIndexChanged(object sender, EventArgs e) =>
            DriverHandler.TDRLevelDropDown(materialComboBox3.SelectedItem?.ToString() ?? "TdrLevelRecover (Def)");

        // TDR Fix Checkbox
        private void materialCheckBox2_CheckedChanged(object sender, EventArgs e) =>
            DriverHandler.TDRDelayFix(materialCheckBox2.Checked);

        // HAGS Fix Checkbox
        private void materialCheckBox3_CheckedChanged(object sender, EventArgs e) =>
            DriverHandler.HAGSFix(materialCheckBox3.Checked);

        // Disable Overlays Checkbox
        private void materialCheckBox5_CheckedChanged(object sender, EventArgs e) =>
            DriverHandler.DisableOverlays(materialCheckBox5.Checked);

    }

}

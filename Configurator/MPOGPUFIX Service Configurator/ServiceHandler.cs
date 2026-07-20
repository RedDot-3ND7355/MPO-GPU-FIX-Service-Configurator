using Microsoft.Win32;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Reflection;
using System.ServiceProcess;

namespace MPOGPUFIX_Service_Configurator
{
    public static class ServiceHandler
    {
        public static ServicecObject? Current { get; private set; }

        public static string GetServiceExecutablePath(string serviceName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
            {
                if (key != null)
                {
                    var imagePath = key.GetValue("ImagePath") as string;
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        // Remove quotes and get the directory
                        imagePath = imagePath.Trim('"');
                        return Path.GetDirectoryName(imagePath) ?? string.Empty;
                    }
                }
            }
            return string.Empty;
        }

        public static void InstallService()
        {
            ServiceLowKeyHandler(true);
            RefreshService("MPOGPUFIX Service");
        }

        public static void UninstallService()
        {
            if (Current?.ServiceController?.Status == ServiceControllerStatus.Running)
            {
                Current.ServiceController.Stop();
                Current.ServiceController.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            RefreshService("MPOGPUFIX Service");
            ServiceLowKeyHandler(false);
            Current = null;
        }

        public static void InitialDetection()
        {
            Current = GetServiceStatus("MPOGPUFIX Service");
        }

        public static void ChangeStartupType(ServiceStartMode mode, bool delayed)
        {
            EnableService("MPOGPUFIX Service", mode, delayed);
        }

        private static void ServiceLowKeyHandler(bool install)
        {
            string serviceName = "MPOGPUFIX Service";
            string exePath = Path.GetFullPath(".\\MPOGPUFIX Service.exe");
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MPOGPUFIX_Service_Configurator.MPOGPUFIX Service.exe";
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var fs = new FileStream(".\\MPOGPUFIX Service.exe", FileMode.Create))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Resource '{resourceName}' not found.");
                stream.CopyTo(fs);
                fs.Close();

                string args = install
                    ? $"create \"{serviceName}\" binPath= \"{exePath}\" start= auto"
                    : $"delete \"{serviceName}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "sc.exe",
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas",                    // Run as Administrator
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using (var process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
        }

        public static void StartService() 
        { 
            Current?.ServiceController?.Start(); 
            RefreshService("MPOGPUFIX Service");
            Current?.ServiceController?.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
            RefreshService("MPOGPUFIX Service");
        }

        public static void StopService() 
        { 
            Current?.ServiceController?.Stop();
            RefreshService("MPOGPUFIX Service");
            Current?.ServiceController?.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
            RefreshService("MPOGPUFIX Service");
        }

        public static void EnableService(string serviceName, ServiceStartMode mode, bool delayed)
        {
            string modeStr = mode switch
            {
                ServiceStartMode.Automatic => "Automatic",
                ServiceStartMode.Manual => "Manual",
                ServiceStartMode.Disabled => "Disabled",
                _ => throw new ArgumentException("Invalid mode")
            };

            using (var mo = new ManagementObject($"Win32_Service.Name='{serviceName}'"))
                mo.InvokeMethod("ChangeStartMode", new object[] { modeStr });

            if (mode == ServiceStartMode.Automatic && delayed)
                SetDelayedAuto(serviceName);
            else if (mode == ServiceStartMode.Automatic)
                UnsetDelayedAuto(serviceName);

            RefreshService(serviceName);
        }

        private static void UnsetDelayedAuto(string serviceName)
        {
            using (var key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
                key.SetValue("DelayedAutostart", 0, RegistryValueKind.DWord);
        }

        public static void DisableService(string serviceName)
        {
            using (var sc = new ServiceController(serviceName))
            {
                if (sc.Status == ServiceControllerStatus.Running)
                    sc.Stop();
            }

            using (var mo = new ManagementObject($"Win32_Service.Name='{serviceName}'"))
                mo.InvokeMethod("ChangeStartMode", new object[] { "Disabled" });

            using (var key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
                key.SetValue("DelayedAutostart", 0, RegistryValueKind.DWord);

            RefreshService(serviceName);
        }

        private static void SetDelayedAuto(string serviceName)
        {
            using (var key = Registry.LocalMachine.CreateSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
            {
                key.SetValue("Start", 2, RegistryValueKind.DWord);
                key.SetValue("DelayedAutostart", 1, RegistryValueKind.DWord);
            }
        }

        private static void RefreshService(string serviceName)
        {
            Current = GetServiceStatus(serviceName);
        }

        private static ServicecObject GetServiceStatus(string serviceName)
        {
            var allServices = ServiceController.GetServices(); // or GetDevices() if needed
            var sc = allServices.FirstOrDefault(s => s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
            if (sc == null)
            {
                // Service does not exist
                return new ServicecObject
                {
                    ServiceController = null,
                    ServiceStatus = ServiceControllerStatus.Stopped,
                    StartupType = ServiceStartMode.Disabled,
                    isDelayed = false
                };
            }

            return new ServicecObject
            {
                ServiceController = sc,
                ServiceStatus = sc.Status,
                StartupType = sc.StartType,
                isDelayed = IsDelayedAuto(serviceName)
            };
        }

        private static bool IsDelayedAuto(string serviceName)
        {
            using (var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}"))
            {
                if (key != null)
                {
                    int start = (int)(key.GetValue("Start", 0) ?? 0);
                    int delayed = (int)(key.GetValue("DelayedAutostart", 0) ?? 0);
                    return start == 2 && delayed == 1;
                }
            }
            return false;
        }
    }

    public class ServicecObject
    {
        public ServiceControllerStatus ServiceStatus { get; set; }
        public ServiceStartMode StartupType { get; set; }
        public ServiceController? ServiceController { get; set; }
        public bool isDelayed { get; set; }
    }

    public class ServiceStartupObject
    {
        public required string Text { get; set; }
        public ServiceStartMode Value { get; set; }
    }
}
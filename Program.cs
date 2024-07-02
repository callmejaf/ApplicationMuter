using System;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Management;

namespace MuteDiscordApp
{
    class Program
    {
        private static List<string> applications;
        private static List<string> devices;
        private static MMDeviceEnumerator deviceEnumerator = new MMDeviceEnumerator();
        private static Dictionary<string, MMDevice> deviceCache = new Dictionary<string, MMDevice>();

        static void Main(string[] args)
        {
            string configFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            string settingsPath = Path.Combine(configFolderPath, "settings.json");

            Console.WriteLine($"Looking for settings file at: {settingsPath}");

            if (!File.Exists(settingsPath))
            {
                Console.WriteLine($"Settings file '{settingsPath}' not found.");
                return;
            }

            string json = File.ReadAllText(settingsPath);
            Console.WriteLine($"Settings file content: {json}");

            var settings = JsonSerializer.Deserialize<Settings>(json);
            Console.WriteLine($"Deserialized settings: Devices='{string.Join(", ", settings?.Devices)}', Applications='{string.Join(", ", settings?.Applications)}'");

            if (settings == null || settings.Devices == null || settings.Applications == null)
            {
                Console.WriteLine("Settings 'devices' and 'applications' must be set in the settings.json file.");
                return;
            }

            devices = settings.Devices;
            applications = settings.Applications;

            // Create a background thread for the muting logic
            Thread muteThread = new Thread(() => MonitorAndMuteDevices())
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };

            muteThread.Start();

            // Monitor for new process creation
            MonitorNewProcesses(applications);

            // Keep the main thread alive
            Console.WriteLine("Monitoring for new audio sessions. Press Ctrl+C to exit.");
            Thread.Sleep(Timeout.Infinite);
        }

        static void MonitorAndMuteDevices()
        {
            // Infinite loop to continuously mute specified applications on specified devices
            while (true)
            {
                foreach (string deviceName in devices)
                {
                    if (!deviceCache.ContainsKey(deviceName))
                    {
                        MMDevice currentDevice = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)
                            .FirstOrDefault(d => d.FriendlyName.Equals(deviceName, StringComparison.OrdinalIgnoreCase));

                        if (currentDevice != null)
                        {
                            deviceCache[deviceName] = currentDevice;
                        }
                        else
                        {
                            Console.WriteLine($"Device '{deviceName}' not found.");
                            deviceCache.Remove(deviceName);
                        }
                    }

                    if (deviceCache.TryGetValue(deviceName, out MMDevice targetDevice))
                    {
                        MuteApplicationsOnDevice(targetDevice, applications);
                    }
                }

                // Wait for 5 seconds before the next iteration to reduce CPU usage
                Thread.Sleep(1000);
            }
        }

        static void MuteApplicationsOnDevice(MMDevice device, List<string> applications)
        {
            var sessionManager = device.AudioSessionManager;
            var sessions = sessionManager.Sessions;

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                Process process = GetProcessBySession(session);

                if (process != null && applications.Any(app => process.ProcessName.Contains(app, StringComparison.OrdinalIgnoreCase) || session.DisplayName.Contains(app, StringComparison.OrdinalIgnoreCase)))
                {
                    if (!session.SimpleAudioVolume.Mute)
                    {
                        session.SimpleAudioVolume.Mute = true;
                        Console.WriteLine($"Muted {process.ProcessName} ({session.DisplayName}) on {device.FriendlyName}");
                    }
                }
            }
        }

        static Process GetProcessBySession(AudioSessionControl session)
        {
            try
            {
                int processId = (int)session.GetProcessID;
                return Process.GetProcessById(processId);
            }
            catch
            {
                return null;
            }
        }

        static void MonitorNewProcesses(List<string> applications)
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(
                new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));

            startWatch.EventArrived += (sender, e) =>
            {
                string processName = (string)e.NewEvent.Properties["ProcessName"].Value;
                if (applications.Any(app => processName.Contains(app, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"New process detected: {processName}");
                    // Trigger immediate re-enumeration
                    foreach (var deviceName in devices)
                    {
                        deviceCache.Remove(deviceName);
                    }
                }
            };

            startWatch.Start();
        }
    }

    class Settings
    {
        public List<string> Devices { get; set; }
        public List<string> Applications { get; set; }
    }
}

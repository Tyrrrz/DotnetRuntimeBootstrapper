using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace DotnetRuntimeBootstrapper.Executable.Env
{
    internal static class InstallationHistory
    {
        private static string? _historyFilePath;

        private static string GetHistoryFilePath()
        {
            if (!string.IsNullOrEmpty(_historyFilePath))
                return _historyFilePath;

            var parentDirectoryPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Tyrrrz/DotnetRuntimeBootstrapper/"
            );

            // Try to include machine ID in the file name so that the file becomes
            // invalid if the operating system is reset or reinstalled.
            try
            {
                var machineId = (string) Registry.LocalMachine
                    .OpenSubKey("SOFTWARE\\Microsoft\\SQMClient")?
                    .GetValue("MachineId");

                return _historyFilePath = Path.Combine(parentDirectoryPath, $"InstallationHistory_{machineId}.txt");
            }
            catch
            {
                return _historyFilePath = Path.Combine(parentDirectoryPath, "InstallationHistory.txt");
            }
        }

        private static HashSet<string>? _installedItems;

        private static HashSet<string> GetInstalledItems()
        {
            if (_installedItems is not null)
                return _installedItems;

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var line in File.ReadAllLines(GetHistoryFilePath()))
                {
                    result.Add(line.Trim());
                }
            }
            catch (IOException)
            {
                // File might not have been created yet, which is fine
            }

            return _installedItems = result;
        }

        public static bool Contains(string itemId) => GetInstalledItems().Contains(itemId);

        public static void Record(string itemId)
        {
            if (Contains(itemId))
                return;

            // Update in-memory representation for consistency
            _installedItems?.Add(itemId);

            try
            {
                var directoryPath = Path.GetDirectoryName(GetHistoryFilePath());
                if (!string.IsNullOrEmpty(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                // We may end up writing duplicate entries in case the file gets updated
                // by another process after our initial read.
                // This should be fine as we're only going to be writing to it once per
                // installation, which in itself should happen once per operating
                // system lifetime, so the file shouldn't grow indefinitely.
                File.AppendAllText(GetHistoryFilePath(), itemId + Environment.NewLine);
            }
            catch (IOException)
            {
                // There might be contention around this file in case there are multiple
                // instances of the bootstrapper running at the same time.
                // This should be fine because they're all probably going to be writing
                // the same thing anyway, so just ignore exceptions in this case.
            }
        }
    }
}
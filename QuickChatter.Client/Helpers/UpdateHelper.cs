using QuickChatter.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace QuickChatter.Client.Helpers
{
    public static class UpdateService
    {
        private const string VersionUrl = "https://raw.githubusercontent.com/TheNevix/QuickChatter/master/QuickChatter.Client/version.json";

        public static async Task<UpdateInfo?> CheckForUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                string json = await client.GetStringAsync(VersionUrl);

                var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(json);
                if (updateInfo == null) return null;

                Version current = Assembly.GetExecutingAssembly().GetName().Version;
                Version remote = new Version(updateInfo.version);

                return remote > current ? updateInfo : null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task DownloadAndUpdate(UpdateInfo info)
        {
            // 1. Download the installer to a temp path
            string tempInstallerPath = Path.Combine(Path.GetTempPath(), "QuickChatterInstaller.exe");

            using var client = new WebClient();
            await client.DownloadFileTaskAsync(info.url, tempInstallerPath);

            // 2. Copy the entire Updater output folder to a new temp folder
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string sourceUpdaterDir = Path.Combine(baseDir, "Updater"); // or wherever the Updater is built
            string tempUpdaterDir = Path.Combine(Path.GetTempPath(), "QuickChatterUpdater_" + Guid.NewGuid());

            Directory.CreateDirectory(tempUpdaterDir);
            foreach (string file in Directory.GetFiles(sourceUpdaterDir))
            {
                string fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(tempUpdaterDir, fileName), true);
            }

            // 3. Start Updater.exe from the temp folder
            string updaterExePath = Path.Combine(tempUpdaterDir, "Updater.exe");
            string appPath = Path.Combine(baseDir, "QuickChatter.Client.exe");

            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExePath,
                Arguments = $"\"{tempInstallerPath}\" \"{appPath}\"",
                UseShellExecute = true
            });

            Application.Current.Shutdown();
        }

    }
}

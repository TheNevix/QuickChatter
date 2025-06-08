using QuickChatter.Client.Helpers;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuickChatter.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var updateInfo = await UpdateService.CheckForUpdateAsync();
            if (updateInfo != null)
            {
                string changesText = string.Join("\n- ", updateInfo.changes);
                string message = $"A new version ({updateInfo.version}) is available.\n\n" +
                                 $"Changes:\n- {changesText}\n\nClick OK to update. The application will restart automatically when the update has finished.";

                var result = MessageBox.Show(
                    message,
                    "Update Available",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.OK)
                {
                    await UpdateService.DownloadAndUpdate(updateInfo);
                }
            }
        }
    }

}

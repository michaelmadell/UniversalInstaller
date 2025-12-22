using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UniversalInstaller.Core.Installation;
using UniversalInstaller.Core.Models;
using UniversalInstaller.Core.Utilities;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class InstallationPage : UserControl
    {
        private readonly InstallerConfig _config;
        private InstallationEngine _engine;
        private bool _installationComplete = false;

        public InstallationPage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
        }

        public async void StartInstallation()
        {
            try
            {
                PathResolver.AppName = _config.Setup.AppName;
                PathResolver.GroupName = _config.Setup.DefaultGroupName;

                var sourceBasePath = AppDomain.CurrentDomain.BaseDirectory;
                _engine = new InstallationEngine(_config, sourceBasePath);

                _engine.ProgressChanged += Engine_ProgressChanged;
                _engine.LogMessage += Engine_LogMessage;

                LogMessage("Starting installation...");

                var success = await _engine.InstallAsync();

                if (success)
                {
                    StatusText.Text = "Installation completed successfully!";
                    LogMessage("Installation completed successfully!");
                    _installationComplete = true;

                    // Enable Next button
                    if (Window.GetWindow(this) is MainWindow mainWindow)
                    {
                        mainWindow.EnableNextButton();
                    }
                }
                else
                {
                    StatusText.Text = "Installation was cancelled.";
                    LogMessage("Installation was cancelled.");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Installation failed!";
                LogMessage($"Installation failed: {ex.Message}");
                MessageBox.Show(
                    $"Installation failed: {ex.Message}",
                    "Installation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Engine_ProgressChanged(object sender, InstallProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = e.PercentComplete;
                PercentText.Text = $"{e.PercentComplete}%";
                CurrentActionText.Text = e.Message;
            });
        }

        private void Engine_LogMessage(object sender, string message)
        {
            LogMessage(message);
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(message + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }
    }
}

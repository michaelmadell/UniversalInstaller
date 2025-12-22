using System.Windows.Controls;
using UniversalInstaller.Core.Models;
using UniversalInstaller.Core.Utilities;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class ReadyPage : UserControl
    {
        private readonly InstallerConfig _config;

        public ReadyPage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadSummary();
        }

        private void LoadSummary()
        {
            AppNameText.Text = _config.Setup.AppName;
            VersionText.Text = _config.Setup.AppVersion;
            LocationText.Text = PathResolver.InstallationPath ?? "Not set";
            PublisherText.Text = _config.Setup.AppPublisher ?? "Unknown";
        }
    }
}

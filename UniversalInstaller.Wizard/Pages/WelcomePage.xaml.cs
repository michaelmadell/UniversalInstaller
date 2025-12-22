using System.Windows.Controls;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class WelcomePage : UserControl
    {
        private readonly InstallerConfig _config;

        public WelcomePage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadContent();
        }

        private void LoadContent()
        {
            WelcomeTitle.Text = $"Welcome to the {_config.Setup.AppName} Setup Wizard";
            WelcomeMessage.Text = $"This will install {_config.Setup.AppName} version {_config.Setup.AppVersion} on your computer.\n\n" +
                                 $"It is recommended that you close all other applications before continuing.\n\n" +
                                 (!string.IsNullOrEmpty(_config.Setup.AppPublisher) ? $"Publisher: {_config.Setup.AppPublisher}" : "");
        }
    }
}

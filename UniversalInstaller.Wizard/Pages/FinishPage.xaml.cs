using System.Windows.Controls;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class FinishPage : UserControl
    {
        private readonly InstallerConfig _config;

        public FinishPage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadContent();
        }

        private void LoadContent()
        {
            FinishTitle.Text = $"Completing the {_config.Setup.AppName} Setup Wizard";
            FinishMessage.Text = $"{_config.Setup.AppName} has been successfully installed on your computer.\n\n" +
                                "Click Finish to close Setup.";
        }
    }
}

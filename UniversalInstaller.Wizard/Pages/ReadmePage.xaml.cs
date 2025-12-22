using System.Linq;
using System.Windows.Controls;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class ReadmePage : UserControl
    {
        private readonly InstallerConfig _config;

        public ReadmePage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadReadme();
        }

        private void LoadReadme()
        {
            if (_config.ReadmeText.Any())
            {
                ReadmeTextBox.Text = string.Join("\r\n", _config.ReadmeText);
            }
            else
            {
                ReadmeTextBox.Text = "No readme information available.";
            }
        }
    }
}

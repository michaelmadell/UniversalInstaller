using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class LicensePage : UserControl, IWizardPage
    {
        private readonly InstallerConfig _config;

        public LicensePage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadLicense();
        }

        private void LoadLicense()
        {
            if (_config.LicenseText.Any())
            {
                LicenseTextBox.Text = string.Join("\r\n", _config.LicenseText);
            }
            else
            {
                LicenseTextBox.Text = "No license text available.";
            }
        }

        public bool ValidatePage()
        {
            if (!AcceptCheckBox.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show(
                    "You must accept the license agreement to continue.",
                    "License Agreement",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void AcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Enable Next button
        }

        private void AcceptCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Disable Next button
        }
    }
}

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;
using UniversalInstaller.Core.Models;
using UniversalInstaller.Core.Utilities;

namespace UniversalInstaller.Wizard.Pages
{
    public partial class DirectoryPage : UserControl, IWizardPage
    {
        private readonly InstallerConfig _config;

        public DirectoryPage(InstallerConfig config)
        {
            InitializeComponent();
            _config = config;
            LoadContent();
        }

        private void LoadContent()
        {
            DirectoryMessage.Text = $"Setup will install {_config.Setup.AppName} in the following folder.\n\n" +
                                   "To install in a different folder, click Browse and select another folder.";

            var defaultPath = PathResolver.Resolve(_config.Setup.DefaultDirName);
            DirectoryTextBox.Text = defaultPath;
            PathResolver.InstallationPath = defaultPath;

            UpdateSpaceInfo();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new VistaFolderBrowserDialog
            {
                Description = "Select Installation Folder",
                SelectedPath = DirectoryTextBox.Text,
                UseDescriptionForTitle = true
            };

            if (folderDialog.ShowDialog() == true)
            {
                DirectoryTextBox.Text = folderDialog.SelectedPath;
                PathResolver.InstallationPath = folderDialog.SelectedPath;
                UpdateSpaceInfo();
            }
        }

        private void UpdateSpaceInfo()
        {
            try
            {
                var path = DirectoryTextBox.Text;
                var drive = Path.GetPathRoot(path);

                if (!string.IsNullOrEmpty(drive))
                {
                    var driveInfo = new DriveInfo(drive);
                    var availableSpace = driveInfo.AvailableFreeSpace / (1024 * 1024); // Convert to MB

                    SpaceRequiredText.Text = $"Space required: 100 MB (estimated)";
                    SpaceAvailableText.Text = $"Space available: {availableSpace:N0} MB";
                }
            }
            catch
            {
                SpaceRequiredText.Text = "";
                SpaceAvailableText.Text = "";
            }
        }

        public bool ValidatePage()
        {
            var path = DirectoryTextBox.Text;

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Please select an installation directory.", "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            try
            {
                var drive = Path.GetPathRoot(path);
                if (!Directory.Exists(drive))
                {
                    MessageBox.Show("The selected drive does not exist.", "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                PathResolver.InstallationPath = path;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid directory path: {ex.Message}", "Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }
    }
}

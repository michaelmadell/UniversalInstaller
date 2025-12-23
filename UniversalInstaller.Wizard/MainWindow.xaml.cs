using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UniversalInstaller.Core.Configuration;
using UniversalInstaller.Core.Models;
using UniversalInstaller.Wizard.Pages;

namespace UniversalInstaller.Wizard
{
    public partial class MainWindow : Window
    {
        private List<UserControl> _pages = new List<UserControl>();
        private int _currentPageIndex = 0;
        private InstallerConfig _config;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfiguration();
            InitializePages();
            ShowCurrentPage();
        }

        private void LoadConfiguration()
        {
            try
            {
                // Extract installer package if needed
                ExtractInstallerPackage();

                // Try to load config.ini from the same directory as the executable
                var configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
                if (System.IO.File.Exists(configPath))
                {
                    _config = IniParser.ParseFile(configPath);
                    Title = $"{_config.Setup.AppName} Setup";
                }
                else
                {
                    // Use default config for testing
                    _config = new InstallerConfig();
                    _config.Setup.AppName = "Sample Application";
                    _config.Setup.AppVersion = "1.0.0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _config = new InstallerConfig();
            }
        }

        private void ExtractInstallerPackage()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var installerDatPath = System.IO.Path.Combine(baseDir, "installer.dat");

            // For single-file apps, use Environment.ProcessPath which works correctly
            var exePath = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(exePath))
                exePath = System.IO.Path.Combine(baseDir, System.AppDomain.CurrentDomain.FriendlyName);

            // Try to extract from embedded package in the executable first
            if (TryExtractEmbeddedPackage(exePath, baseDir))
            {
                return;
            }

            // Fallback: Check if installer.dat exists alongside the executable
            if (System.IO.File.Exists(installerDatPath))
            {
                try
                {
                    ExtractZipArchive(installerDatPath, baseDir);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error extracting installer package: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool TryExtractEmbeddedPackage(string exePath, string destinationDir)
        {
            try
            {
                if (!System.IO.File.Exists(exePath))
                    return false;

                using (var exeStream = System.IO.File.OpenRead(exePath))
                {
                    // Read the last 16 bytes to check for our marker
                    if (exeStream.Length < 16)
                        return false;

                    exeStream.Seek(-16, System.IO.SeekOrigin.End);
                    using (var reader = new System.IO.BinaryReader(exeStream))
                    {
                        var zipOffset = reader.ReadInt64();
                        var marker = reader.ReadString();

                        if (marker != "UNIINST")
                            return false;

                        // Found embedded package - extract it to a temp file
                        exeStream.Seek(zipOffset, System.IO.SeekOrigin.Begin);
                        var zipLength = exeStream.Length - zipOffset - 16;

                        var tempZipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "installer_temp.dat");
                        using (var tempStream = System.IO.File.Create(tempZipPath))
                        {
                            var buffer = new byte[81920]; // 80KB buffer
                            long remaining = zipLength;
                            while (remaining > 0)
                            {
                                int toRead = (int)Math.Min(buffer.Length, remaining);
                                int bytesRead = exeStream.Read(buffer, 0, toRead);
                                if (bytesRead == 0) break;
                                tempStream.Write(buffer, 0, bytesRead);
                                remaining -= bytesRead;
                            }
                        }

                        // Extract the ZIP archive
                        ExtractZipArchive(tempZipPath, destinationDir);

                        // Clean up temp file
                        try { System.IO.File.Delete(tempZipPath); } catch { }

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        private void ExtractZipArchive(string zipPath, string destinationDir)
        {
            using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var destinationPath = System.IO.Path.Combine(destinationDir, entry.FullName);

                    // Create directory if needed
                    var destDir = System.IO.Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(destDir) && !System.IO.Directory.Exists(destDir))
                    {
                        System.IO.Directory.CreateDirectory(destDir);
                    }

                    // Extract file (skip directories)
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        entry.ExtractToFile(destinationPath, overwrite: true);
                    }
                }
            }
        }

        private void InitializePages()
        {
            _pages.Clear();

            // Add pages based on configuration
            if (!_config.Setup.DisableWelcomePage)
                _pages.Add(new WelcomePage(_config));

            if (_config.Setup.ShowLicense && (_config.LicenseText.Any() || !string.IsNullOrEmpty(_config.Setup.LicenseFile)))
                _pages.Add(new LicensePage(_config));

            if (_config.Setup.ShowReadme && (_config.ReadmeText.Any() || !string.IsNullOrEmpty(_config.Setup.ReadmeFile)))
                _pages.Add(new ReadmePage(_config));

            if (_config.Components.Any())
                _pages.Add(new ComponentsPage(_config));

            if (!_config.Setup.DisableDirPage)
                _pages.Add(new DirectoryPage(_config));

            if (!_config.Setup.DisableReadyPage)
                _pages.Add(new ReadyPage(_config));

            _pages.Add(new InstallationPage(_config));

            if (!_config.Setup.DisableFinishedPage)
                _pages.Add(new FinishPage(_config));
        }

        private void ShowCurrentPage()
        {
            if (_currentPageIndex >= 0 && _currentPageIndex < _pages.Count)
            {
                PageContent.Content = _pages[_currentPageIndex];
                UpdateNavigationButtons();
            }
        }

        private void UpdateNavigationButtons()
        {
            BackButton.IsEnabled = _currentPageIndex > 0;

            // Check if current page is installation page
            var isInstallPage = _pages[_currentPageIndex] is InstallationPage;
            var isFinishPage = _pages[_currentPageIndex] is FinishPage;

            if (isInstallPage)
            {
                NextButton.IsEnabled = false;
                BackButton.IsEnabled = false;
            }
            else if (isFinishPage)
            {
                NextButton.Content = "Finish";
                NextButton.IsEnabled = true;
            }
            else if (_currentPageIndex == _pages.Count - 1)
            {
                NextButton.Content = "Finish";
            }
            else
            {
                NextButton.Content = "Next >";
                NextButton.IsEnabled = true;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                ShowCurrentPage();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate current page
            if (_pages[_currentPageIndex] is IWizardPage wizardPage)
            {
                if (!wizardPage.ValidatePage())
                {
                    return;
                }
            }

            if (_currentPageIndex < _pages.Count - 1)
            {
                _currentPageIndex++;
                ShowCurrentPage();

                // Auto-start installation if we just moved to installation page
                if (_pages[_currentPageIndex] is InstallationPage installPage)
                {
                    installPage.StartInstallation();
                }
            }
            else
            {
                // Finish button clicked
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to cancel the installation?",
                "Cancel Installation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        public void EnableNextButton()
        {
            NextButton.IsEnabled = true;
        }
    }

    public interface IWizardPage
    {
        bool ValidatePage();
    }
}

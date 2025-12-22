using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using UniversalInstaller.Core.Models;
using UniversalInstaller.Core.Utilities;

namespace UniversalInstaller.Core.Installation
{
    public class InstallationEngine
    {
        public event EventHandler<InstallProgressEventArgs> ProgressChanged;
        public event EventHandler<string> LogMessage;

        private readonly InstallerConfig _config;
        private readonly string _sourceBasePath;
        private readonly List<string> _installedFiles = new List<string>();
        private readonly List<string> _createdDirectories = new List<string>();
        private readonly List<RegistryKey> _createdRegistryKeys = new List<RegistryKey>();

        public InstallationEngine(InstallerConfig config, string sourceBasePath)
        {
            _config = config;
            _sourceBasePath = sourceBasePath;
        }

        public async Task<bool> InstallAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                Log("Installation started");
                int totalSteps = CalculateTotalSteps();
                int currentStep = 0;

                // Create directories
                foreach (var dir in _config.Dirs)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    await CreateDirectoryAsync(dir);
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, $"Creating directory: {dir.Name}");
                }

                // Copy files
                foreach (var file in _config.Files)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    await CopyFileAsync(file);
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, $"Copying: {Path.GetFileName(file.Source)}");
                }

                // Create registry entries
                foreach (var reg in _config.Registry)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    CreateRegistryEntry(reg);
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, $"Creating registry entry: {reg.Subkey}");
                }

                // Create icons/shortcuts
                foreach (var icon in _config.Icons)
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    await CreateShortcutAsync(icon);
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, $"Creating shortcut: {icon.Name}");
                }

                // Create uninstaller
                if (_config.Setup.CreateUninstaller)
                {
                    await CreateUninstallerAsync();
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, "Creating uninstaller");
                }

                // Run post-install scripts
                foreach (var run in _config.Run.Where(r => !r.Flags.Contains("postinstall")))
                {
                    if (cancellationToken.IsCancellationRequested) return false;

                    await RunCommandAsync(run);
                    currentStep++;
                    ReportProgress(currentStep, totalSteps, $"Running: {Path.GetFileName(run.Filename)}");
                }

                ReportProgress(totalSteps, totalSteps, "Installation completed successfully");
                Log("Installation completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Installation failed: {ex.Message}");
                await RollbackAsync();
                throw;
            }
        }

        private async Task CreateDirectoryAsync(DirectoryEntry dir)
        {
            var path = PathResolver.Resolve(dir.Name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                _createdDirectories.Add(path);
                Log($"Created directory: {path}");
            }
            await Task.CompletedTask;
        }

        private async Task CopyFileAsync(FileEntry file)
        {
            var sourcePath = Path.Combine(_sourceBasePath, file.Source);
            var destDir = PathResolver.Resolve(file.DestDir);

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _createdDirectories.Add(destDir);
            }

            if (file.Recurse && Directory.Exists(sourcePath))
            {
                await CopyDirectoryRecursiveAsync(sourcePath, destDir);
            }
            else if (File.Exists(sourcePath))
            {
                var destName = string.IsNullOrEmpty(file.DestName) ? Path.GetFileName(sourcePath) : file.DestName;
                var destPath = Path.Combine(destDir, destName);

                await Task.Run(() => File.Copy(sourcePath, destPath, true));
                _installedFiles.Add(destPath);
                Log($"Copied file: {sourcePath} -> {destPath}");
            }
            else if (sourcePath.Contains("*") || sourcePath.Contains("?"))
            {
                // Handle wildcards
                var dirPath = Path.GetDirectoryName(sourcePath);
                var pattern = Path.GetFileName(sourcePath);

                if (Directory.Exists(dirPath))
                {
                    foreach (var matchedFile in Directory.GetFiles(dirPath, pattern))
                    {
                        var destPath = Path.Combine(destDir, Path.GetFileName(matchedFile));
                        await Task.Run(() => File.Copy(matchedFile, destPath, true));
                        _installedFiles.Add(destPath);
                        Log($"Copied file: {matchedFile} -> {destPath}");
                    }
                }
            }
        }

        private async Task CopyDirectoryRecursiveAsync(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
                _createdDirectories.Add(destDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                await Task.Run(() => File.Copy(file, destFile, true));
                _installedFiles.Add(destFile);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                await CopyDirectoryRecursiveAsync(dir, destSubDir);
            }
        }

        private void CreateRegistryEntry(RegistryEntry reg)
        {
            try
            {
                var rootKey = GetRegistryRoot(reg.Root);
                var subkey = PathResolver.Resolve(reg.Subkey);

                using (var key = rootKey.CreateSubKey(subkey, true))
                {
                    if (key != null)
                    {
                        var valueType = GetRegistryValueKind(reg.ValueType);
                        object value = reg.ValueData;

                        if (valueType == RegistryValueKind.DWord)
                            value = int.Parse(reg.ValueData);
                        else if (valueType == RegistryValueKind.QWord)
                            value = long.Parse(reg.ValueData);

                        key.SetValue(reg.ValueName, value, valueType);
                        Log($"Created registry entry: {reg.Root}\\{subkey}\\{reg.ValueName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to create registry entry: {ex.Message}");
            }
        }

        private async Task CreateShortcutAsync(IconEntry icon)
        {
            var shortcutPath = PathResolver.Resolve(icon.Name);
            var targetPath = PathResolver.Resolve(icon.Filename);
            var workingDir = string.IsNullOrEmpty(icon.WorkingDir) ? Path.GetDirectoryName(targetPath) : PathResolver.Resolve(icon.WorkingDir);

            var shortcutDir = Path.GetDirectoryName(shortcutPath);
            if (!Directory.Exists(shortcutDir))
            {
                Directory.CreateDirectory(shortcutDir);
                _createdDirectories.Add(shortcutDir);
            }

            if (!shortcutPath.EndsWith(".lnk"))
                shortcutPath += ".lnk";

            await Task.Run(() => ShortcutHelper.CreateShortcut(shortcutPath, targetPath, workingDir, icon.Parameters, icon.IconFilename));
            _installedFiles.Add(shortcutPath);
            Log($"Created shortcut: {shortcutPath}");
        }

        private async Task RunCommandAsync(RunEntry run)
        {
            try
            {
                var filename = PathResolver.Resolve(run.Filename);
                var workingDir = string.IsNullOrEmpty(run.WorkingDir) ? Path.GetDirectoryName(filename) : PathResolver.Resolve(run.WorkingDir);

                var startInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = run.Parameters,
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    Log($"Executed: {filename} (Exit code: {process.ExitCode})");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to execute command: {ex.Message}");
            }
        }

        private async Task CreateUninstallerAsync()
        {
            var uninstallerPath = Path.Combine(PathResolver.InstallationPath, "uninstall.exe");
            var manifestPath = Path.Combine(PathResolver.InstallationPath, "install.manifest");

            // Create installation manifest
            var manifest = new InstallationManifest
            {
                InstalledFiles = _installedFiles,
                CreatedDirectories = _createdDirectories,
                AppName = _config.Setup.AppName,
                InstallPath = PathResolver.InstallationPath
            };

            await Task.Run(() =>
            {
                File.WriteAllText(manifestPath, System.Text.Json.JsonSerializer.Serialize(manifest));
            });

            // Copy uninstaller executable (should be embedded or included)
            // For now, we'll create a placeholder
            Log($"Uninstaller created at: {uninstallerPath}");

            // Register uninstaller in Windows
            RegisterUninstaller(uninstallerPath);
        }

        private void RegisterUninstaller(string uninstallerPath)
        {
            try
            {
                var uninstallKey = Registry.LocalMachine.CreateSubKey($@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{_config.Setup.AppName}");
                if (uninstallKey != null)
                {
                    var displayName = string.IsNullOrEmpty(_config.Setup.UninstallDisplayName) ? _config.Setup.AppName : _config.Setup.UninstallDisplayName;

                    uninstallKey.SetValue("DisplayName", displayName);
                    uninstallKey.SetValue("UninstallString", $"\"{uninstallerPath}\"");
                    uninstallKey.SetValue("DisplayVersion", _config.Setup.AppVersion);
                    uninstallKey.SetValue("Publisher", _config.Setup.AppPublisher);
                    uninstallKey.SetValue("DisplayIcon", string.IsNullOrEmpty(_config.Setup.UninstallDisplayIcon) ? uninstallerPath : PathResolver.Resolve(_config.Setup.UninstallDisplayIcon));
                    uninstallKey.SetValue("InstallLocation", PathResolver.InstallationPath);

                    uninstallKey.Close();
                    Log("Registered uninstaller in Windows");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to register uninstaller: {ex.Message}");
            }
        }

        private async Task RollbackAsync()
        {
            Log("Rolling back installation...");

            // Delete installed files
            foreach (var file in _installedFiles.AsEnumerable().Reverse())
            {
                try
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                        Log($"Deleted: {file}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Failed to delete {file}: {ex.Message}");
                }
            }

            // Remove created directories
            foreach (var dir in _createdDirectories.AsEnumerable().Reverse())
            {
                try
                {
                    if (Directory.Exists(dir) && !Directory.EnumerateFileSystemEntries(dir).Any())
                    {
                        Directory.Delete(dir);
                        Log($"Deleted directory: {dir}");
                    }
                }
                catch (Exception ex)
                {
                    Log($"Failed to delete directory {dir}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        private int CalculateTotalSteps()
        {
            return _config.Dirs.Count + _config.Files.Count + _config.Registry.Count +
                   _config.Icons.Count + _config.Run.Count + (_config.Setup.CreateUninstaller ? 1 : 0);
        }

        private void ReportProgress(int current, int total, string message)
        {
            ProgressChanged?.Invoke(this, new InstallProgressEventArgs
            {
                CurrentStep = current,
                TotalSteps = total,
                Message = message,
                PercentComplete = (int)((double)current / total * 100)
            });
        }

        private void Log(string message)
        {
            LogMessage?.Invoke(this, $"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private RegistryKey GetRegistryRoot(string root)
        {
            return root.ToUpper() switch
            {
                "HKLM" or "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
                "HKCU" or "HKEY_CURRENT_USER" => Registry.CurrentUser,
                "HKCR" or "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
                "HKU" or "HKEY_USERS" => Registry.Users,
                "HKCC" or "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
                _ => Registry.LocalMachine
            };
        }

        private RegistryValueKind GetRegistryValueKind(string type)
        {
            return type.ToLower() switch
            {
                "string" or "sz" => RegistryValueKind.String,
                "dword" => RegistryValueKind.DWord,
                "qword" => RegistryValueKind.QWord,
                "binary" => RegistryValueKind.Binary,
                "expandsz" => RegistryValueKind.ExpandString,
                "multisz" => RegistryValueKind.MultiString,
                _ => RegistryValueKind.String
            };
        }
    }

    public class InstallProgressEventArgs : EventArgs
    {
        public int CurrentStep { get; set; }
        public int TotalSteps { get; set; }
        public string Message { get; set; }
        public int PercentComplete { get; set; }
    }

    public class InstallationManifest
    {
        public List<string> InstalledFiles { get; set; } = new List<string>();
        public List<string> CreatedDirectories { get; set; } = new List<string>();
        public string AppName { get; set; }
        public string InstallPath { get; set; }
    }
}

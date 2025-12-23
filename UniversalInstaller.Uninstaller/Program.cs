using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;
using UniversalInstaller.Core.Installation;

namespace UniversalInstaller.Uninstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            // Check if we're running from temp (cleanup mode)
            var isCleanupMode = args.Length > 0 && args[0] == "--cleanup";

            if (!isCleanupMode)
            {
                // First run: Copy to temp and restart from there
                var currentExe = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
                var tempExe = Path.Combine(Path.GetTempPath(), "UniversalInstaller_Cleanup_" + Guid.NewGuid().ToString("N") + ".exe");
                var installDir = AppDomain.CurrentDomain.BaseDirectory;

                try
                {
                    File.Copy(currentExe, tempExe, true);

                    // Start the cleanup process from temp
                    var startInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = tempExe,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    // Add arguments properly without extra quotes
                    startInfo.ArgumentList.Add("--cleanup");
                    startInfo.ArgumentList.Add(installDir);

                    System.Diagnostics.Process.Start(startInfo);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: Could not start cleanup process: {ex.Message}");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }
            }

            // Cleanup mode: We're running from temp, can safely delete installation directory
            var targetDir = args.Length > 1 ? args[1] : AppDomain.CurrentDomain.BaseDirectory;

            Console.WriteLine("Universal Installer - Uninstaller");
            Console.WriteLine("=================================\n");

            try
            {
                // Load installation manifest from the installation directory
                var manifestPath = Path.Combine(targetDir, "install.manifest");

                if (!File.Exists(manifestPath))
                {
                    Console.WriteLine("Error: Installation manifest not found.");
                    Console.WriteLine($"Expected location: {manifestPath}");
                    Console.WriteLine($"Target directory: {targetDir}");
                    Console.WriteLine($"Args count: {args.Length}");
                    if (args.Length > 0)
                    {
                        for (int i = 0; i < args.Length; i++)
                        {
                            Console.WriteLine($"  Arg[{i}]: '{args[i]}'");
                        }
                    }
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                var manifestJson = File.ReadAllText(manifestPath);
                var manifest = JsonSerializer.Deserialize<InstallationManifest>(manifestJson);

                if (manifest == null)
                {
                    Console.WriteLine("Error: Could not load installation manifest.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine($"Application: {manifest.AppName}");
                Console.WriteLine($"Installation Path: {manifest.InstallPath}");
                Console.WriteLine($"Files to remove: {manifest.InstalledFiles.Count}");
                Console.WriteLine($"Directories to remove: {manifest.CreatedDirectories.Count}");
                Console.WriteLine();

                // Confirm uninstallation
                Console.Write("Are you sure you want to uninstall this application? (y/n): ");
                var response = Console.ReadLine();

                if (!response.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Uninstallation cancelled.");
                    return;
                }

                Console.WriteLine("\nUninstalling...\n");

                // Check for and run removal script from registry
                // Try multiple possible registry paths
                string[] possiblePaths = new[]
                {
                    $@"Software\CoreStation\HXAgent", // Specific to CoreStation HX Agent
                    $@"Software\{manifest.AppName.Replace(" ", "")}\{manifest.AppName.Replace(" ", "")}", // Generic pattern
                };

                string removeScriptPath = null;
                foreach (var regPath in possiblePaths)
                {
                    try
                    {
                        using (var key = Registry.LocalMachine.OpenSubKey(regPath, false))
                        {
                            if (key != null)
                            {
                                removeScriptPath = key.GetValue("RemoveScript") as string;
                                if (!string.IsNullOrEmpty(removeScriptPath))
                                {
                                    Console.WriteLine($"Found removal script in registry: {regPath}");
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next path
                    }
                }

                if (!string.IsNullOrEmpty(removeScriptPath) && File.Exists(removeScriptPath))
                {
                    Console.WriteLine($"Running removal script: {removeScriptPath}");
                    try
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            UseShellExecute = false,
                            CreateNoWindow = false,
                            WorkingDirectory = Path.GetDirectoryName(removeScriptPath)
                        };
                        startInfo.ArgumentList.Add("-ExecutionPolicy");
                        startInfo.ArgumentList.Add("Bypass");
                        startInfo.ArgumentList.Add("-NoProfile");
                        startInfo.ArgumentList.Add("-File");
                        startInfo.ArgumentList.Add(removeScriptPath);

                        var process = System.Diagnostics.Process.Start(startInfo);
                        if (process != null)
                        {
                            process.WaitForExit();
                            Console.WriteLine($"Removal script completed with exit code: {process.ExitCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to run removal script: {ex.Message}");
                    }
                }

                Console.WriteLine();

                int filesRemoved = 0;
                int dirsRemoved = 0;

                // Remove installed files
                Console.WriteLine("Removing files...");
                foreach (var file in manifest.InstalledFiles.AsEnumerable().Reverse())
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Console.WriteLine($"  Deleted: {Path.GetFileName(file)}");
                            filesRemoved++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Failed to delete {file}: {ex.Message}");
                    }
                }

                // Remove created directories
                Console.WriteLine("\nRemoving directories...");
                foreach (var dir in manifest.CreatedDirectories.AsEnumerable().Reverse())
                {
                    try
                    {
                        if (Directory.Exists(dir))
                        {
                            // Only remove if empty
                            if (!Directory.EnumerateFileSystemEntries(dir).Any())
                            {
                                Directory.Delete(dir);
                                Console.WriteLine($"  Deleted: {dir}");
                                dirsRemoved++;
                            }
                            else
                            {
                                Console.WriteLine($"  Skipped (not empty): {dir}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Failed to delete {dir}: {ex.Message}");
                    }
                }

                // Remove registry entry for uninstaller
                Console.WriteLine("\nRemoving registry entries...");
                try
                {
                    using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true))
                    {
                        if (key != null)
                        {
                            key.DeleteSubKey(manifest.AppName, false);
                            Console.WriteLine($"  Removed uninstaller registry entry");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Failed to remove registry entry: {ex.Message}");
                }

                // Remove installation directory (now safe since we're running from temp)
                if (Directory.Exists(manifest.InstallPath))
                {
                    try
                    {
                        // Give the original uninstaller process time to exit
                        System.Threading.Thread.Sleep(500);

                        Directory.Delete(manifest.InstallPath, true);
                        dirsRemoved++;
                        Console.WriteLine($"\nRemoved installation directory: {manifest.InstallPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nFailed to remove installation directory: {ex.Message}");
                        Console.WriteLine("Some files may still be in use. Please remove manually if needed.");
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nUninstallation completed!");
                Console.WriteLine($"Files removed: {filesRemoved}");
                Console.WriteLine($"Directories removed: {dirsRemoved}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError during uninstallation: {ex.Message}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();

            // Cleanup temp exe if we're in cleanup mode
            if (isCleanupMode)
            {
                try
                {
                    var currentExe = Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(currentExe) && currentExe.Contains("UniversalInstaller_Cleanup_"))
                    {
                        // Schedule deletion of temp exe using cmd
                        var batchFile = Path.Combine(Path.GetTempPath(), "cleanup_" + Guid.NewGuid().ToString("N") + ".bat");
                        File.WriteAllText(batchFile, $"@echo off\ntimeout /t 2 /nobreak >nul\ndel /f /q \"{currentExe}\"\ndel /f /q \"%~f0\"");

                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = batchFile,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                        };

                        System.Diagnostics.Process.Start(startInfo);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}

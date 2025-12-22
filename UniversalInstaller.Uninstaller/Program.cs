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
            Console.WriteLine("Universal Installer - Uninstaller");
            Console.WriteLine("=================================\n");

            try
            {
                // Load installation manifest
                var manifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "install.manifest");

                if (!File.Exists(manifestPath))
                {
                    Console.WriteLine("Error: Installation manifest not found.");
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

                // Remove installation directory if empty
                if (Directory.Exists(manifest.InstallPath))
                {
                    try
                    {
                        if (!Directory.EnumerateFileSystemEntries(manifest.InstallPath).Any())
                        {
                            Directory.Delete(manifest.InstallPath);
                            Console.WriteLine($"\nRemoved installation directory: {manifest.InstallPath}");
                        }
                        else
                        {
                            Console.WriteLine($"\nInstallation directory not removed (contains user files): {manifest.InstallPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\nFailed to remove installation directory: {ex.Message}");
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
        }
    }
}

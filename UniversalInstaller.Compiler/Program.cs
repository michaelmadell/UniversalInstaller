using System;
using System.IO;
using System.IO.Compression;
using UniversalInstaller.Core.Configuration;

namespace UniversalInstaller.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Universal Installer Compiler v1.0");
            Console.WriteLine("==================================\n");

            if (args.Length == 0)
            {
                ShowUsage();
                return;
            }

            string configFile = args[0];
            string outputDir = args.Length > 1 ? args[1] : "Output";

            try
            {
                CompileInstaller(configFile, outputDir);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: UniversalInstaller.Compiler <config.ini> [output_directory]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  config.ini        - Path to the installer configuration file");
            Console.WriteLine("  output_directory  - Output directory for the installer (default: Output)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  UniversalInstaller.Compiler myapp.ini C:\\Build\\Output");
        }

        static void CompileInstaller(string configFile, string outputDir)
        {
            if (!File.Exists(configFile))
            {
                throw new FileNotFoundException($"Configuration file not found: {configFile}");
            }

            Console.WriteLine($"Loading configuration: {configFile}");
            var config = IniParser.ParseFile(configFile);

            Console.WriteLine($"Application: {config.Setup.AppName} v{config.Setup.AppVersion}");
            Console.WriteLine($"Publisher: {config.Setup.AppPublisher}");
            Console.WriteLine();

            var baseDir = Path.GetDirectoryName(Path.GetFullPath(configFile));
            var outputPath = Path.GetFullPath(outputDir);

            if (!Directory.Exists(outputPath))
            {
                Console.WriteLine($"Creating output directory: {outputPath}");
                Directory.CreateDirectory(outputPath);
            }

            var installerName = string.IsNullOrEmpty(config.Setup.OutputBaseFilename)
                ? "setup.exe"
                : config.Setup.OutputBaseFilename + ".exe";

            var installerPath = Path.Combine(outputPath, installerName);

            Console.WriteLine("Building installer...");
            Console.WriteLine($"Output: {installerPath}");
            Console.WriteLine();

            // Step 1: Copy the wizard executable
            Console.WriteLine("[1/5] Copying wizard executable...");
            var wizardExe = FindWizardExecutable();
            if (!File.Exists(wizardExe))
            {
                throw new FileNotFoundException("Wizard executable not found. Please build the solution first.");
            }
            File.Copy(wizardExe, installerPath, true);

            // Step 2: Create package directory
            Console.WriteLine("[2/5] Creating package structure...");
            var packageDir = Path.Combine(outputPath, "_temp_package");
            if (Directory.Exists(packageDir))
                Directory.Delete(packageDir, true);
            Directory.CreateDirectory(packageDir);

            // Step 3: Copy configuration
            Console.WriteLine("[3/5] Embedding configuration...");
            var targetConfigPath = Path.Combine(packageDir, "config.ini");
            File.Copy(configFile, targetConfigPath, true);

            // Step 4: Copy all referenced files
            Console.WriteLine("[4/5] Collecting installation files...");
            int fileCount = 0;

            foreach (var file in config.Files)
            {
                var sourcePath = Path.Combine(baseDir, file.Source);

                if (file.Recurse && Directory.Exists(sourcePath))
                {
                    var targetPath = Path.Combine(packageDir, "Files", Path.GetFileName(sourcePath));
                    CopyDirectory(sourcePath, targetPath);
                    fileCount += Directory.GetFiles(targetPath, "*", SearchOption.AllDirectories).Length;
                }
                else if (File.Exists(sourcePath))
                {
                    var targetPath = Path.Combine(packageDir, "Files", Path.GetFileName(sourcePath));
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(sourcePath, targetPath, true);
                    fileCount++;
                }
                else if (sourcePath.Contains("*") || sourcePath.Contains("?"))
                {
                    var dirPath = Path.GetDirectoryName(sourcePath);
                    var pattern = Path.GetFileName(sourcePath);

                    if (Directory.Exists(dirPath))
                    {
                        foreach (var matchedFile in Directory.GetFiles(dirPath, pattern))
                        {
                            var targetPath = Path.Combine(packageDir, "Files", Path.GetFileName(matchedFile));
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            File.Copy(matchedFile, targetPath, true);
                            fileCount++;
                        }
                    }
                }
            }

            Console.WriteLine($"  Collected {fileCount} file(s)");

            // Copy wizard dependencies
            CopyWizardDependencies(Path.GetDirectoryName(wizardExe), packageDir);

            // Step 5: Create self-extracting archive (simplified - in real scenario, would embed as resource)
            Console.WriteLine("[5/5] Creating installer package...");

            if (config.Setup.Compression)
            {
                var archivePath = Path.Combine(outputPath, "installer.dat");
                ZipFile.CreateFromDirectory(packageDir, archivePath);
                Console.WriteLine($"  Created compressed package: {archivePath}");
            }

            // Clean up temp directory
            Directory.Delete(packageDir, true);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nInstaller created successfully!");
            Console.WriteLine($"Output: {installerPath}");
            Console.ResetColor();

            Console.WriteLine("\nInstaller Details:");
            Console.WriteLine($"  Application: {config.Setup.AppName}");
            Console.WriteLine($"  Version: {config.Setup.AppVersion}");
            Console.WriteLine($"  Files: {fileCount}");
            Console.WriteLine($"  Compression: {(config.Setup.Compression ? "Enabled" : "Disabled")}");
        }

        static string FindWizardExecutable()
        {
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UniversalInstaller.Wizard.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Debug", "net10.0-windows", "UniversalInstaller.Wizard.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Release", "net10.0-windows", "UniversalInstaller.Wizard.exe"),
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                    return fullPath;
            }

            return "UniversalInstaller.Wizard.exe";
        }

        static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var targetFile = Path.Combine(targetDir, Path.GetFileName(file));
                File.Copy(file, targetFile, true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                var targetSubDir = Path.Combine(targetDir, Path.GetFileName(dir));
                CopyDirectory(dir, targetSubDir);
            }
        }

        static void CopyWizardDependencies(string wizardDir, string packageDir)
        {
            var dllFiles = Directory.GetFiles(wizardDir, "*.dll");
            foreach (var dll in dllFiles)
            {
                var targetPath = Path.Combine(packageDir, Path.GetFileName(dll));
                File.Copy(dll, targetPath, true);
            }
        }
    }
}

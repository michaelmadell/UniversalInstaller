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

            // Step 1: Copy the wizard executable (contains embedded uninstaller and Core.dll)
            Console.WriteLine("[1/5] Copying wizard executable...");
            Console.WriteLine("  Note: Looking for published single-file wizard executable");
            var wizardExe = FindWizardExecutable();
            if (!File.Exists(wizardExe))
            {
                throw new FileNotFoundException("Wizard executable not found. Please publish the Wizard project first using:\n  dotnet publish UniversalInstaller.Wizard -c Debug");
            }

            var wizardDir = Path.GetDirectoryName(wizardExe);

            // Check if this is a single-file published exe (no .dll alongside it)
            var wizardBaseName = Path.GetFileNameWithoutExtension(wizardExe);
            var wizardDll = Path.Combine(wizardDir, wizardBaseName + ".dll");
            var isSingleFile = !File.Exists(wizardDll);

            // Copy the .exe file
            File.Copy(wizardExe, installerPath, true);
            Console.WriteLine($"  Copied: {Path.GetFileName(installerPath)} ({new FileInfo(wizardExe).Length / 1024} KB)");

            if (!isSingleFile)
            {
                // Not published as single-file, copy runtime files
                Console.WriteLine("  Warning: Wizard not published as single-file. Copying runtime files...");
                var installerBaseName = Path.GetFileNameWithoutExtension(installerPath);

                // Copy .dll
                var installerDll = Path.Combine(outputPath, installerBaseName + ".dll");
                if (File.Exists(wizardDll))
                {
                    File.Copy(wizardDll, installerDll, true);
                    Console.WriteLine($"  Copied: {Path.GetFileName(installerDll)}");
                }

                // Copy .runtimeconfig.json
                var wizardRuntimeConfig = Path.Combine(wizardDir, wizardBaseName + ".runtimeconfig.json");
                var installerRuntimeConfig = Path.Combine(outputPath, installerBaseName + ".runtimeconfig.json");
                if (File.Exists(wizardRuntimeConfig))
                {
                    File.Copy(wizardRuntimeConfig, installerRuntimeConfig, true);
                    Console.WriteLine($"  Copied: {Path.GetFileName(installerRuntimeConfig)}");
                }

                // Copy and fix .deps.json
                var wizardDeps = Path.Combine(wizardDir, wizardBaseName + ".deps.json");
                var installerDeps = Path.Combine(outputPath, installerBaseName + ".deps.json");
                if (File.Exists(wizardDeps))
                {
                    var depsContent = File.ReadAllText(wizardDeps);
                    depsContent = depsContent.Replace(wizardBaseName + ".dll", installerBaseName + ".dll");
                    depsContent = depsContent.Replace("\"" + wizardBaseName + "/", "\"" + installerBaseName + "/");
                    File.WriteAllText(installerDeps, depsContent);
                    Console.WriteLine($"  Copied: {Path.GetFileName(installerDeps)}");
                }
            }
            else
            {
                Console.WriteLine("  Using single-file published wizard (no additional runtime files needed)");
            }

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

            // Step 5: Create self-extracting archive and append to executable
            Console.WriteLine("[5/5] Creating self-contained installer package...");

            if (config.Setup.Compression)
            {
                var archivePath = Path.Combine(outputPath, "installer.dat");
                ZipFile.CreateFromDirectory(packageDir, archivePath);
                Console.WriteLine($"  Created compressed package: {archivePath}");
                Console.WriteLine($"  Package size: {new FileInfo(archivePath).Length / 1024} KB");

                // Append installer.dat to the executable to make it truly self-contained
                Console.WriteLine($"  Appending package to executable...");
                using (var exeStream = File.Open(installerPath, FileMode.Append))
                using (var datStream = File.OpenRead(archivePath))
                {
                    datStream.CopyTo(exeStream);

                    // Write a marker and the offset of where the ZIP starts
                    using (var writer = new BinaryWriter(exeStream))
                    {
                        var zipOffset = exeStream.Length - datStream.Length;
                        writer.Write(zipOffset); // Write offset as Int64
                        writer.Write("UNIINST"); // Write magic marker
                    }
                }

                // Keep installer.dat for users who want to distribute both files
                Console.WriteLine($"  Embedded package into executable");
                Console.WriteLine($"  Note: You can distribute just the .exe file, or both .exe and .dat");
            }
            else
            {
                // Without compression, copy files directly next to installer
                Console.WriteLine($"  Copying package files to output directory (no compression)");
                CopyDirectory(packageDir, outputPath);
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
            Console.WriteLine($"  Self-contained: Yes (includes embedded uninstaller)");
        }

        static string FindWizardExecutable()
        {
            var possiblePaths = new[]
            {
                // Try published single-file first (with runtime identifier)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Debug", "net10.0-windows", "win-x64", "publish", "UniversalInstaller.Wizard.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Release", "net10.0-windows", "win-x64", "publish", "UniversalInstaller.Wizard.exe"),
                // Try published single-file without runtime identifier
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Debug", "net10.0-windows", "publish", "UniversalInstaller.Wizard.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "UniversalInstaller.Wizard", "bin", "Release", "net10.0-windows", "publish", "UniversalInstaller.Wizard.exe"),
                // Fall back to regular build output
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

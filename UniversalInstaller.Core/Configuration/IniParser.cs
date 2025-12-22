using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniversalInstaller.Core.Models;

namespace UniversalInstaller.Core.Configuration
{
    public class IniParser
    {
        public static InstallerConfig ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");

            var lines = File.ReadAllLines(filePath);
            return Parse(lines, Path.GetDirectoryName(filePath));
        }

        public static InstallerConfig Parse(string[] lines, string basePath = "")
        {
            var config = new InstallerConfig();
            string currentSection = "";
            var currentEntry = new Dictionary<string, string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                // Handle section headers
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // Save previous entry if exists
                    if (currentEntry.Count > 0 && !string.IsNullOrEmpty(currentSection))
                    {
                        AddEntryToConfig(config, currentSection, currentEntry);
                        currentEntry.Clear();
                    }

                    currentSection = line.Substring(1, line.Length - 2).Trim();
                    continue;
                }

                // Handle key-value pairs
                var delimiterIndex = line.IndexOf('=');
                if (delimiterIndex > 0)
                {
                    var key = line.Substring(0, delimiterIndex).Trim();
                    var value = line.Substring(delimiterIndex + 1).Trim();

                    // Handle multi-line values
                    if (value.EndsWith("\\"))
                    {
                        var sb = new StringBuilder(value.TrimEnd('\\'));
                        while (i + 1 < lines.Length)
                        {
                            i++;
                            var nextLine = lines[i].TrimStart();
                            if (nextLine.EndsWith("\\"))
                            {
                                sb.Append(nextLine.TrimEnd('\\'));
                            }
                            else
                            {
                                sb.Append(nextLine);
                                break;
                            }
                        }
                        value = sb.ToString();
                    }

                    // Remove quotes if present
                    if (value.StartsWith("\"") && value.EndsWith("\""))
                        value = value.Substring(1, value.Length - 2);

                    // For Setup section, apply immediately
                    if (currentSection.Equals("Setup", StringComparison.OrdinalIgnoreCase))
                    {
                        SetPropertyValue(config.Setup, key, value, basePath);
                    }
                    else
                    {
                        // For other sections, accumulate entries
                        if (key.Equals("Source", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("Filename", StringComparison.OrdinalIgnoreCase))
                        {
                            // Start of new entry
                            if (currentEntry.Count > 0)
                            {
                                AddEntryToConfig(config, currentSection, currentEntry);
                                currentEntry.Clear();
                            }
                        }
                        currentEntry[key] = value;
                    }
                }
            }

            // Add last entry
            if (currentEntry.Count > 0 && !string.IsNullOrEmpty(currentSection))
            {
                AddEntryToConfig(config, currentSection, currentEntry);
            }

            // Load external files
            LoadExternalFiles(config, basePath);

            return config;
        }

        private static void AddEntryToConfig(InstallerConfig config, string section, Dictionary<string, string> entry)
        {
            switch (section.ToLower())
            {
                case "files":
                    config.Files.Add(CreateObject<FileEntry>(entry));
                    break;
                case "dirs":
                    config.Dirs.Add(CreateObject<DirectoryEntry>(entry));
                    break;
                case "icons":
                    config.Icons.Add(CreateObject<IconEntry>(entry));
                    break;
                case "registry":
                    config.Registry.Add(CreateObject<RegistryEntry>(entry));
                    break;
                case "run":
                    config.Run.Add(CreateObject<RunEntry>(entry));
                    break;
                case "components":
                    config.Components.Add(CreateObject<ComponentEntry>(entry));
                    break;
                case "tasks":
                    config.Tasks.Add(CreateObject<TaskEntry>(entry));
                    break;
            }
        }

        private static T CreateObject<T>(Dictionary<string, string> properties) where T : new()
        {
            var obj = new T();
            var type = typeof(T);

            foreach (var kvp in properties)
            {
                var prop = type.GetProperty(kvp.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        object value = kvp.Value;
                        if (prop.PropertyType == typeof(bool))
                        {
                            value = kvp.Value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                                    kvp.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                    kvp.Value == "1";
                        }
                        else if (prop.PropertyType == typeof(int))
                        {
                            value = int.Parse(kvp.Value);
                        }

                        prop.SetValue(obj, Convert.ChangeType(value, prop.PropertyType));
                    }
                    catch { }
                }
            }

            return obj;
        }

        private static void SetPropertyValue(object obj, string propertyName, string value, string basePath)
        {
            var type = obj.GetType();
            var prop = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (prop != null && prop.CanWrite)
            {
                try
                {
                    object convertedValue = value;
                    if (prop.PropertyType == typeof(bool))
                    {
                        convertedValue = value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                                        value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                        value == "1";
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        convertedValue = int.Parse(value);
                    }

                    prop.SetValue(obj, Convert.ChangeType(convertedValue, prop.PropertyType));
                }
                catch { }
            }
        }

        private static void LoadExternalFiles(InstallerConfig config, string basePath)
        {
            // Load license file
            if (!string.IsNullOrEmpty(config.Setup.LicenseFile))
            {
                var licensePath = Path.Combine(basePath, config.Setup.LicenseFile);
                if (File.Exists(licensePath))
                {
                    config.LicenseText.AddRange(File.ReadAllLines(licensePath));
                }
            }

            // Load readme file
            if (!string.IsNullOrEmpty(config.Setup.ReadmeFile))
            {
                var readmePath = Path.Combine(basePath, config.Setup.ReadmeFile);
                if (File.Exists(readmePath))
                {
                    config.ReadmeText.AddRange(File.ReadAllLines(readmePath));
                }
            }
        }
    }
}

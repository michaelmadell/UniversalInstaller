using System;
using System.Collections.Generic;
using System.IO;

namespace UniversalInstaller.Core.Utilities
{
    public static class PathResolver
    {
        private static readonly Dictionary<string, Func<string>> SpecialFolders = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "{app}", () => InstallationPath ?? "" },
            { "{win}", () => Environment.GetFolderPath(Environment.SpecialFolder.Windows) },
            { "{sys}", () => Environment.SystemDirectory },
            { "{pf}", () => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) },
            { "{pf32}", () => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) },
            { "{pf64}", () => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) },
            { "{cf}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) },
            { "{cf32}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86) },
            { "{cf64}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles) },
            { "{tmp}", () => Path.GetTempPath() },
            { "{sd}", () => Environment.GetFolderPath(Environment.SpecialFolder.System) },
            { "{userappdata}", () => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) },
            { "{localappdata}", () => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) },
            { "{userdocs}", () => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) },
            { "{userdesktop}", () => Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
            { "{userstartmenu}", () => Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) },
            { "{userprograms}", () => Environment.GetFolderPath(Environment.SpecialFolder.Programs) },
            { "{userstartup}", () => Environment.GetFolderPath(Environment.SpecialFolder.Startup) },
            { "{commonappdata}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) },
            { "{commondocs}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) },
            { "{commondesktop}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory) },
            { "{commonstartmenu}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu) },
            { "{commonprograms}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms) },
            { "{commonstartup}", () => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup) },
        };

        public static string InstallationPath { get; set; }
        public static string GroupName { get; set; }
        public static string AppName { get; set; }

        public static string Resolve(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var resolved = path;

            // Replace all special folders
            foreach (var folder in SpecialFolders)
            {
                if (resolved.Contains(folder.Key))
                {
                    resolved = resolved.Replace(folder.Key, folder.Value());
                }
            }

            // Handle {group}
            if (resolved.Contains("{group}"))
            {
                var programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                var groupPath = string.IsNullOrEmpty(GroupName) ? AppName : GroupName;
                resolved = resolved.Replace("{group}", Path.Combine(programsPath, groupPath ?? ""));
            }

            // Handle environment variables
            resolved = Environment.ExpandEnvironmentVariables(resolved);

            return resolved;
        }

        public static string GetDefaultInstallPath(string appName)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), appName);
        }
    }
}

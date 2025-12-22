using System;
using System.Collections.Generic;

namespace UniversalInstaller.Core.Models
{
    public class InstallerConfig
    {
        public SetupSection Setup { get; set; } = new SetupSection();
        public List<FileEntry> Files { get; set; } = new List<FileEntry>();
        public List<DirectoryEntry> Dirs { get; set; } = new List<DirectoryEntry>();
        public List<IconEntry> Icons { get; set; } = new List<IconEntry>();
        public List<RegistryEntry> Registry { get; set; } = new List<RegistryEntry>();
        public List<RunEntry> Run { get; set; } = new List<RunEntry>();
        public List<string> LicenseText { get; set; } = new List<string>();
        public List<string> ReadmeText { get; set; } = new List<string>();
        public List<ComponentEntry> Components { get; set; } = new List<ComponentEntry>();
        public List<TaskEntry> Tasks { get; set; } = new List<TaskEntry>();
    }

    public class SetupSection
    {
        public string AppName { get; set; } = "Application";
        public string AppVersion { get; set; } = "1.0.0";
        public string AppPublisher { get; set; } = "";
        public string AppPublisherURL { get; set; } = "";
        public string AppSupportURL { get; set; } = "";
        public string AppUpdatesURL { get; set; } = "";
        public string DefaultDirName { get; set; } = "{pf}\\MyApp";
        public string DefaultGroupName { get; set; } = "";
        public string OutputDir { get; set; } = "Output";
        public string OutputBaseFilename { get; set; } = "setup";
        public string SetupIconFile { get; set; } = "";
        public string WizardImageFile { get; set; } = "";
        public string WizardSmallImageFile { get; set; } = "";
        public bool Compression { get; set; } = true;
        public bool SolidCompression { get; set; } = true;
        public bool CreateUninstaller { get; set; } = true;
        public bool AllowNoIcons { get; set; } = false;
        public string LicenseFile { get; set; } = "";
        public string ReadmeFile { get; set; } = "";
        public bool ShowLicense { get; set; } = true;
        public bool ShowReadme { get; set; } = false;
        public string UninstallDisplayName { get; set; } = "";
        public string UninstallDisplayIcon { get; set; } = "";
        public bool PrivilegesRequired { get; set; } = true;
        public bool DisableWelcomePage { get; set; } = false;
        public bool DisableDirPage { get; set; } = false;
        public bool DisableProgramGroupPage { get; set; } = false;
        public bool DisableReadyPage { get; set; } = false;
        public bool DisableFinishedPage { get; set; } = false;
        public string WizardStyle { get; set; } = "modern";
    }

    public class FileEntry
    {
        public string Source { get; set; } = "";
        public string DestDir { get; set; } = "{app}";
        public string DestName { get; set; } = "";
        public string Components { get; set; } = "";
        public string Tasks { get; set; } = "";
        public bool Recurse { get; set; } = false;
        public string Flags { get; set; } = "";
    }

    public class DirectoryEntry
    {
        public string Name { get; set; } = "";
        public string Components { get; set; } = "";
        public string Tasks { get; set; } = "";
    }

    public class IconEntry
    {
        public string Name { get; set; } = "";
        public string Filename { get; set; } = "";
        public string WorkingDir { get; set; } = "";
        public string Parameters { get; set; } = "";
        public string IconFilename { get; set; } = "";
        public string Components { get; set; } = "";
        public string Tasks { get; set; } = "";
    }

    public class RegistryEntry
    {
        public string Root { get; set; } = "HKLM";
        public string Subkey { get; set; } = "";
        public string ValueName { get; set; } = "";
        public string ValueType { get; set; } = "string";
        public string ValueData { get; set; } = "";
        public string Components { get; set; } = "";
        public string Tasks { get; set; } = "";
        public string Flags { get; set; } = "";
    }

    public class RunEntry
    {
        public string Filename { get; set; } = "";
        public string Parameters { get; set; } = "";
        public string WorkingDir { get; set; } = "";
        public string Description { get; set; } = "";
        public string Flags { get; set; } = "";
        public string Components { get; set; } = "";
        public string Tasks { get; set; } = "";
    }

    public class ComponentEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Types { get; set; } = "";
        public bool Fixed { get; set; } = false;
    }

    public class TaskEntry
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Components { get; set; } = "";
        public bool Checked { get; set; } = true;
    }
}

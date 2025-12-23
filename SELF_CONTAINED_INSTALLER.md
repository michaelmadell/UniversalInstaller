# Self-Contained Installer Implementation

## Overview

The Universal Installer now produces **truly self-contained executables** that include all necessary components within a single installer file. This means you only need to distribute one `.exe` file to end users.

## What's Included

Each compiled installer now embeds:

1. **Wizard UI** - The installation interface
2. **Configuration** - Your INI settings
3. **Installation Files** - All application files specified in the config
4. **Uninstaller** - A fully functional uninstaller executable
5. **Core Library** - Required runtime dependencies

## How It Works

### Build Process

When you build the solution, the following happens automatically:

1. **UniversalInstaller.Core** is built first (shared library)
2. **UniversalInstaller.Uninstaller** is built next
3. **UniversalInstaller.Wizard** embeds the uninstaller and Core.dll as resources
4. **UniversalInstaller.Compiler** packages everything and appends it to the wizard executable

### Compiler Process (Creating a Self-Contained Installer)

When you compile an installer, the compiler:

1. Copies the Wizard.exe as the base installer
2. Creates a package directory with:
   - `config.ini` - Your INI configuration
   - `Files/` - All application files from the config
   - Supporting DLLs (Ookii.Dialogs.Wpf.dll, etc.)
3. Creates a ZIP archive (`installer.dat`) from the package
4. **Appends the ZIP to the end of the .exe file**
5. Writes a footer with the ZIP offset and a magic marker ("UNIINST")

The result is a **single .exe file** that contains everything!

### Resource Embedding

The Wizard project includes these embedded resources:
- `UniversalInstaller.Uninstaller.exe` - Extracted during installation
- `UniversalInstaller.Core.dll` - Required by the uninstaller

### First-Run Extraction

When the installer first runs, it:

1. Checks if it has an embedded package (reads the last 16 bytes for the magic marker)
2. If found, extracts the ZIP from within the .exe to a temp file
3. Unpacks the ZIP to extract config.ini and application files to the current directory
4. Falls back to checking for an adjacent `installer.dat` file if no embedded package

### Installation Process

When a user runs your installer:

1. The wizard extracts the configuration
2. Files are copied according to the INI settings
3. Registry entries and shortcuts are created
4. **The uninstaller is extracted** to the installation directory
5. **Core.dll is extracted** for the uninstaller to use
6. An installation manifest is created tracking all installed files
7. The uninstaller is registered with Windows

### Uninstallation Process

When the user uninstalls via Add/Remove Programs or runs `uninstall.exe`:

1. The manifest is read to identify all installed files
2. Files are removed in reverse installation order
3. Directories are cleaned up
4. Registry entries are removed
5. The uninstaller cleans up after itself

## Building Installers

### Prerequisites

Build the entire solution first:

```bash
dotnet build UniversalInstaller.sln --configuration Release
```

### Compiling an Installer

```bash
UniversalInstaller.Compiler.exe myapp.ini Output
```

This produces:
- `Output/setup.exe` - A single self-contained installer

### Distribution

Simply distribute the single `setup.exe` file. No additional files needed!

## Technical Details

### Embedded Resources Configuration

In `UniversalInstaller.Wizard.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="..\UniversalInstaller.Uninstaller\bin\$(Configuration)\net10.0\UniversalInstaller.Uninstaller.exe">
    <Link>Resources\UniversalInstaller.Uninstaller.exe</Link>
    <LogicalName>UniversalInstaller.Uninstaller.exe</LogicalName>
  </EmbeddedResource>
  <EmbeddedResource Include="..\UniversalInstaller.Core\bin\$(Configuration)\net10.0\UniversalInstaller.Core.dll">
    <Link>Resources\UniversalInstaller.Core.dll</Link>
    <LogicalName>UniversalInstaller.Core.dll</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

### Extraction During Installation

In `InstallationEngine.cs`, the `CreateUninstallerAsync()` method:

1. Creates the installation manifest (JSON)
2. Extracts `UniversalInstaller.Uninstaller.exe` from resources
3. Extracts `UniversalInstaller.Core.dll` from resources
4. Registers the uninstaller in Windows registry

### Files Created During Installation

In the installation directory:
- `uninstall.exe` - Extracted uninstaller
- `install.manifest` - JSON file tracking installed components
- `UniversalInstaller.Core.dll` - Required by uninstaller
- Your application files

## Benefits

1. **Single File Distribution** - No external dependencies
2. **No Missing Files** - Everything needed is embedded
3. **Works Everywhere** - No installation required to run the installer
4. **Professional** - Users see a complete, polished experience
5. **Clean Uninstall** - Fully functional uninstaller included

## Manifest File Format

The `install.manifest` file is JSON and contains:

```json
{
  "InstalledFiles": ["C:\\Program Files\\MyApp\\app.exe", ...],
  "CreatedDirectories": ["C:\\Program Files\\MyApp", ...],
  "AppName": "MyApplication",
  "InstallPath": "C:\\Program Files\\MyApp"
}
```

This allows the uninstaller to remove exactly what was installed.

## Registry Integration

The installer registers itself under:
```
HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppName}
```

With these values:
- `DisplayName` - Application name
- `UninstallString` - Path to uninstall.exe
- `DisplayVersion` - Version number
- `Publisher` - Publisher name
- `DisplayIcon` - Icon to show
- `InstallLocation` - Installation directory

## Example Workflow

1. **Developer**: Writes `myapp.ini` configuration
2. **Developer**: Runs compiler: `UniversalInstaller.Compiler myapp.ini`
3. **Developer**: Distributes single `setup.exe` file
4. **User**: Downloads and runs `setup.exe`
5. **User**: Follows wizard to install application
6. **User**: Can uninstall via Windows Settings or `uninstall.exe`

## Migration Notes

If you have existing installers from before this update:

- Rebuild your installers using the new compiler
- The new installers will automatically include the uninstaller
- Old installations won't have the uninstaller embedded (expected)
- New installations will be fully self-contained

## Troubleshooting

**Build Error: "Resource not found"**
- Ensure you build the full solution, not just individual projects
- The Wizard depends on Uninstaller and Core being built first

**Uninstaller not working**
- Check that `install.manifest` exists in the installation directory
- Verify `UniversalInstaller.Core.dll` was extracted properly
- The manifest file must be valid JSON

**Files not embedding**
- Clean and rebuild the solution
- Check that the paths in the .csproj are correct
- Verify the build configuration (Debug/Release) matches

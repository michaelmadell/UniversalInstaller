# Universal Installer - Complete Documentation

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [INI Configuration Reference](#ini-configuration-reference)
4. [Building Installers](#building-installers)
5. [Advanced Features](#advanced-features)
6. [Examples](#examples)
7. [Troubleshooting](#troubleshooting)

---

## Introduction

Universal Installer is a powerful, flexible Windows installer creation system similar to Inno Setup. It allows you to create professional installation packages using simple INI configuration files.

### Key Features

- **INI-based Configuration**: Easy-to-understand configuration format
- **Modern Wizard Interface**: Professional WPF-based installer wizard
- **Flexible Installation**: Support for files, directories, registry entries, and shortcuts
- **Script Execution**: Run PowerShell, CMD, or executable scripts during installation
- **Component Selection**: Allow users to choose which components to install
- **License & Readme Support**: Built-in pages for EULA and readme files
- **Uninstaller Generation**: Automatically creates Windows-compliant uninstaller
- **Path Variables**: Support for Windows special folders and custom variables
- **Compression**: Optional file compression for smaller installers

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Windows 10/11
- Visual Studio 2022 or later (for development)

### Building the Project

1. Clone or download the Universal Installer source code
2. Open `UniversalInstaller.sln` in Visual Studio
3. Build the solution (Ctrl+Shift+B)

Alternatively, use the command line:

```bash
dotnet build UniversalInstaller.sln
```

### Creating Your First Installer

1. Create a configuration file (e.g., `myapp.ini`)
2. Define your application settings
3. List files to install
4. Build the installer using the compiler

Example:

```bash
UniversalInstaller.Compiler.exe myapp.ini
```

This will create a `setup.exe` in the Output directory.

---

## INI Configuration Reference

### [Setup] Section

The `[Setup]` section contains basic application and installer settings.

#### Application Information

| Property | Description | Example |
|----------|-------------|---------|
| `AppName` | Name of your application | `AppName=My Application` |
| `AppVersion` | Version number | `AppVersion=1.0.0` |
| `AppPublisher` | Company or publisher name | `AppPublisher=ACME Corp` |
| `AppPublisherURL` | Publisher's website | `AppPublisherURL=https://example.com` |
| `AppSupportURL` | Support website URL | `AppSupportURL=https://example.com/support` |
| `AppUpdatesURL` | Updates/download URL | `AppUpdatesURL=https://example.com/updates` |

#### Installation Directories

| Property | Description | Example |
|----------|-------------|---------|
| `DefaultDirName` | Default installation directory | `DefaultDirName={pf}\MyApp` |
| `DefaultGroupName` | Start Menu folder name | `DefaultGroupName=My Application` |

#### Output Settings

| Property | Description | Example |
|----------|-------------|---------|
| `OutputDir` | Output directory for installer | `OutputDir=Output` |
| `OutputBaseFilename` | Installer filename (without .exe) | `OutputBaseFilename=MyAppSetup` |

#### Installer Appearance

| Property | Description | Example |
|----------|-------------|---------|
| `SetupIconFile` | Icon for installer executable | `SetupIconFile=app.ico` |
| `WizardImageFile` | Large wizard image (164x314) | `WizardImageFile=wizard.bmp` |
| `WizardSmallImageFile` | Small wizard image (55x58) | `WizardSmallImageFile=wizardsmall.bmp` |
| `WizardStyle` | Wizard visual style | `WizardStyle=modern` |

#### Compression

| Property | Description | Example |
|----------|-------------|---------|
| `Compression` | Enable compression (yes/no) | `Compression=yes` |
| `SolidCompression` | Use solid compression (yes/no) | `SolidCompression=yes` |

#### Installer Behavior

| Property | Description | Example |
|----------|-------------|---------|
| `PrivilegesRequired` | Require admin privileges (yes/no) | `PrivilegesRequired=yes` |
| `CreateUninstaller` | Generate uninstaller (yes/no) | `CreateUninstaller=yes` |
| `AllowNoIcons` | Allow installation without icons (yes/no) | `AllowNoIcons=no` |

#### License and Documentation

| Property | Description | Example |
|----------|-------------|---------|
| `LicenseFile` | Path to license/EULA file | `LicenseFile=license.txt` |
| `ShowLicense` | Show license page (yes/no) | `ShowLicense=yes` |
| `ReadmeFile` | Path to readme file | `ReadmeFile=readme.txt` |
| `ShowReadme` | Show readme page (yes/no) | `ShowReadme=yes` |

#### Wizard Pages

| Property | Description | Example |
|----------|-------------|---------|
| `DisableWelcomePage` | Hide welcome page (yes/no) | `DisableWelcomePage=no` |
| `DisableDirPage` | Hide directory selection (yes/no) | `DisableDirPage=no` |
| `DisableProgramGroupPage` | Hide program group page (yes/no) | `DisableProgramGroupPage=no` |
| `DisableReadyPage` | Hide ready to install page (yes/no) | `DisableReadyPage=no` |
| `DisableFinishedPage` | Hide finish page (yes/no) | `DisableFinishedPage=no` |

#### Uninstaller Settings

| Property | Description | Example |
|----------|-------------|---------|
| `UninstallDisplayName` | Name in Add/Remove Programs | `UninstallDisplayName=My App` |
| `UninstallDisplayIcon` | Icon in Add/Remove Programs | `UninstallDisplayIcon={app}\app.exe` |

---

### [Files] Section

The `[Files]` section defines which files to install.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Source` | Source file path (supports wildcards) | Yes |
| `DestDir` | Destination directory | Yes |
| `DestName` | Destination filename (optional) | No |
| `Components` | Associated components (comma-separated) | No |
| `Tasks` | Associated tasks (comma-separated) | No |
| `Recurse` | Copy subdirectories (yes/no) | No |
| `Flags` | Additional flags | No |

#### Examples

```ini
[Files]
; Single file
Source=MyApp.exe
DestDir={app}

; Multiple files with wildcards
Source=*.dll
DestDir={app}

; Recursive directory copy
Source=Resources
DestDir={app}\Resources
Recurse=yes

; Component-specific file
Source=AdvancedTools.exe
DestDir={app}
Components=advanced
```

---

### [Dirs] Section

Create additional directories during installation.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Name` | Directory path to create | Yes |
| `Components` | Associated components | No |
| `Tasks` | Associated tasks | No |

#### Examples

```ini
[Dirs]
Name={app}\data
Name={app}\logs
Name={userappdata}\MyApp
Name={app}\plugins
Components=core
```

---

### [Icons] Section

Create shortcuts in Start Menu, Desktop, or other locations.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Name` | Shortcut path and name | Yes |
| `Filename` | Target executable/file | Yes |
| `WorkingDir` | Working directory | No |
| `Parameters` | Command-line parameters | No |
| `IconFilename` | Icon file path | No |
| `Components` | Associated components | No |
| `Tasks` | Associated tasks | No |

#### Examples

```ini
[Icons]
; Start Menu shortcut
Name={group}\My Application
Filename={app}\MyApp.exe
WorkingDir={app}
IconFilename={app}\MyApp.exe

; Desktop shortcut (task-based)
Name={userdesktop}\My Application
Filename={app}\MyApp.exe
Tasks=desktopicon

; Uninstaller shortcut
Name={group}\Uninstall My Application
Filename={app}\uninstall.exe
```

---

### [Registry] Section

Create or modify Windows registry entries.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Root` | Registry root key | Yes |
| `Subkey` | Registry subkey path | Yes |
| `ValueName` | Value name | No |
| `ValueType` | Data type (string, dword, qword, binary, expandsz, multisz) | Yes |
| `ValueData` | Value data | Yes |
| `Components` | Associated components | No |
| `Tasks` | Associated tasks | No |
| `Flags` | Additional flags | No |

#### Registry Root Keys

- `HKLM` or `HKEY_LOCAL_MACHINE`: Local machine settings
- `HKCU` or `HKEY_CURRENT_USER`: Current user settings
- `HKCR` or `HKEY_CLASSES_ROOT`: File associations and COM
- `HKU` or `HKEY_USERS`: All users
- `HKCC` or `HKEY_CURRENT_CONFIG`: Current configuration

#### Examples

```ini
[Registry]
; Application registration
Root=HKLM
Subkey=Software\MyCompany\MyApp
ValueName=InstallPath
ValueType=string
ValueData={app}

; Version number
Root=HKLM
Subkey=Software\MyCompany\MyApp
ValueName=Version
ValueType=string
ValueData=1.0.0

; DWORD value
Root=HKCU
Subkey=Software\MyCompany\MyApp
ValueName=FirstRun
ValueType=dword
ValueData=1

; File association
Root=HKCR
Subkey=.myext
ValueType=string
ValueData=MyApp.Document
```

---

### [Run] Section

Execute programs, scripts, or commands during/after installation.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Filename` | Program/script to execute | Yes |
| `Parameters` | Command-line parameters | No |
| `WorkingDir` | Working directory | No |
| `Description` | Description shown to user | No |
| `Flags` | Execution flags | No |
| `Components` | Associated components | No |
| `Tasks` | Associated tasks | No |

#### Flags

- `postinstall`: Run after installation completes (user can skip)
- `nowait`: Don't wait for program to finish
- `runhidden`: Hide program window
- `shellexec`: Use ShellExecute instead of CreateProcess

#### Examples

```ini
[Run]
; Run setup script
Filename={app}\setup.bat
WorkingDir={app}
Description=Running setup script

; Execute PowerShell script
Filename=powershell.exe
Parameters=-ExecutionPolicy Bypass -File "{app}\setup.ps1"
WorkingDir={app}
Description=Configuring application

; Optional post-install launch
Filename={app}\MyApp.exe
Description=Launch My Application
Flags=postinstall

; Database initialization
Filename={app}\dbsetup.exe
Parameters=/install /silent
Description=Initializing database
```

---

### [Components] Section

Define optional components for custom installation.

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Name` | Component identifier | Yes |
| `Description` | User-visible description | Yes |
| `Types` | Installation types including this component | No |
| `Fixed` | Component cannot be unchecked (yes/no) | No |

#### Examples

```ini
[Components]
Name=core
Description=Core application files (required)
Types=full,compact,custom
Fixed=yes

Name=docs
Description=Documentation and help files
Types=full,custom
Fixed=no

Name=samples
Description=Sample files and templates
Types=full,custom

Name=advanced
Description=Advanced tools for power users
Types=full,custom
```

---

### [Tasks] Section

Define optional tasks (like creating desktop shortcuts).

#### Properties

| Property | Description | Required |
|----------|-------------|----------|
| `Name` | Task identifier | Yes |
| `Description` | User-visible description | Yes |
| `Components` | Required components | No |
| `Checked` | Default state (yes/no) | No |

#### Examples

```ini
[Tasks]
Name=desktopicon
Description=Create a desktop shortcut
Components=core
Checked=yes

Name=quicklaunch
Description=Create a Quick Launch shortcut
Checked=no

Name=associate
Description=Associate .myext files with this application
Components=core
Checked=yes

Name=autostart
Description=Start application automatically with Windows
Checked=no
```

---

## Path Variables

Universal Installer supports the following path variables:

| Variable | Description | Example |
|----------|-------------|---------|
| `{app}` | Application installation directory | `C:\Program Files\MyApp` |
| `{win}` | Windows directory | `C:\Windows` |
| `{sys}` | Windows System directory | `C:\Windows\System32` |
| `{pf}` | Program Files | `C:\Program Files` |
| `{pf32}` | Program Files (x86) | `C:\Program Files (x86)` |
| `{pf64}` | Program Files (64-bit) | `C:\Program Files` |
| `{cf}` | Common Files | `C:\Program Files\Common Files` |
| `{tmp}` | Temporary directory | `C:\Users\User\AppData\Local\Temp` |
| `{userappdata}` | User Application Data | `C:\Users\User\AppData\Roaming` |
| `{localappdata}` | User Local AppData | `C:\Users\User\AppData\Local` |
| `{userdocs}` | User Documents | `C:\Users\User\Documents` |
| `{userdesktop}` | User Desktop | `C:\Users\User\Desktop` |
| `{userstartmenu}` | User Start Menu | `C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu` |
| `{userprograms}` | User Programs folder | `C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu\Programs` |
| `{userstartup}` | User Startup folder | `C:\Users\User\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup` |
| `{commonappdata}` | Common Application Data | `C:\ProgramData` |
| `{commondocs}` | Common Documents | `C:\Users\Public\Documents` |
| `{commondesktop}` | Common Desktop | `C:\Users\Public\Desktop` |
| `{commonstartmenu}` | Common Start Menu | `C:\ProgramData\Microsoft\Windows\Start Menu` |
| `{commonprograms}` | Common Programs | `C:\ProgramData\Microsoft\Windows\Start Menu\Programs` |
| `{commonstartup}` | Common Startup | `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup` |
| `{group}` | Start Menu program group | `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\MyApp` |

---

## Building Installers

### Using the Compiler

```bash
UniversalInstaller.Compiler.exe <config.ini> [output_directory]
```

#### Parameters

- `config.ini`: Path to your INI configuration file
- `output_directory`: (Optional) Output directory for the installer. Default: `Output`

#### Examples

```bash
# Basic usage
UniversalInstaller.Compiler.exe myapp.ini

# Specify output directory
UniversalInstaller.Compiler.exe myapp.ini C:\Build\Installers

# Using relative paths
UniversalInstaller.Compiler.exe configs\myapp.ini ..\output
```

### Build Output

The compiler creates:
1. **setup.exe** (or your specified name): The installer executable
2. **installer.dat**: Compressed package containing all files
3. Build log showing what was included

---

## Advanced Features

### Custom Installation Scripts

You can run custom scripts during installation:

**PowerShell Example:**

```ini
[Run]
Filename=powershell.exe
Parameters=-ExecutionPolicy Bypass -File "{app}\scripts\configure.ps1"
Description=Configuring application
```

**Batch File Example:**

```ini
[Run]
Filename={app}\scripts\setup.bat
Parameters=/quiet
WorkingDir={app}
Description=Running setup
```

### File Associations

Associate file types with your application:

```ini
[Registry]
; Define file extension
Root=HKCR
Subkey=.myext
ValueType=string
ValueData=MyApp.Document

; Define document type
Root=HKCR
Subkey=MyApp.Document
ValueType=string
ValueData=My Application Document

; Set icon
Root=HKCR
Subkey=MyApp.Document\DefaultIcon
ValueType=string
ValueData={app}\MyApp.exe,0

; Set open command
Root=HKCR
Subkey=MyApp.Document\shell\open\command
ValueType=string
ValueData="{app}\MyApp.exe" "%1"
```

### Conditional Installation

Use components and tasks to create conditional installations:

```ini
[Components]
Name=core
Description=Core files
Fixed=yes

Name=plugins
Description=Optional plugins

[Files]
Source=core\*.exe
DestDir={app}
Components=core

Source=plugins\*.dll
DestDir={app}\plugins
Components=plugins
```

### Multi-Language Support

Create language-specific sections (future feature):

```ini
[Setup]
AppName=My Application

[Languages]
Name=english
Name=spanish

[Messages.english]
WelcomeLabel=Welcome to Setup

[Messages.spanish]
WelcomeLabel=Bienvenido a la instalación
```

---

## Examples

### Example 1: Simple Application

```ini
[Setup]
AppName=Simple App
AppVersion=1.0
DefaultDirName={pf}\SimpleApp
OutputBaseFilename=SimpleAppSetup

[Files]
Source=SimpleApp.exe
DestDir={app}

[Icons]
Name={group}\Simple App
Filename={app}\SimpleApp.exe
```

### Example 2: Application with Database

```ini
[Setup]
AppName=Database Application
AppVersion=2.0
DefaultDirName={pf}\DbApp
LicenseFile=license.txt
ShowLicense=yes

[Files]
Source=DbApp.exe
DestDir={app}

Source=database\*.sql
DestDir={app}\database

[Dirs]
Name={app}\data

[Run]
Filename={app}\DbSetup.exe
Parameters=/install "{app}\database\schema.sql"
Description=Setting up database
```

### Example 3: Plugin-Based Application

```ini
[Setup]
AppName=Plugin App
AppVersion=3.0
DefaultDirName={pf}\PluginApp

[Components]
Name=core
Description=Core application
Fixed=yes

Name=plugins
Description=Standard plugins

Name=advanced
Description=Advanced plugins

[Files]
Source=App.exe
DestDir={app}
Components=core

Source=plugins\standard\*.dll
DestDir={app}\plugins
Components=plugins

Source=plugins\advanced\*.dll
DestDir={app}\plugins
Components=advanced

[Icons]
Name={group}\Plugin App
Filename={app}\App.exe

[Tasks]
Name=desktopicon
Description=Create desktop shortcut
```

---

## Troubleshooting

### Common Issues

#### Issue: "Configuration file not found"
**Solution**: Ensure the INI file path is correct and the file exists.

#### Issue: "Source file not found"
**Solution**: Check that all `Source` paths in `[Files]` section are relative to the INI file location.

#### Issue: "Installer won't run on other computers"
**Solution**: Ensure the target computer has .NET 10.0 or later installed.

#### Issue: "Registry entries not created"
**Solution**: Run the installer with administrator privileges if using HKLM registry keys.

#### Issue: "Files not copying"
**Solution**: Check file paths and ensure `DestDir` uses valid path variables.

### Debug Mode

Enable detailed logging by checking the installation log in the wizard.

### Getting Help

- Check the examples in the `Examples` folder
- Review sample configurations
- Report issues on the project repository

---

## License

Universal Installer is open-source software. See LICENSE file for details.

---

## Version History

- **1.0.0**: Initial release
  - INI-based configuration
  - File installation
  - Registry support
  - Shortcut creation
  - Wizard interface
  - Uninstaller generation

---

**Universal Installer** - Create professional Windows installers with ease!

# Getting Started with Universal Installer

Welcome to Universal Installer! This guide will help you get started creating Windows installers.

## What You Just Built

Universal Installer is a complete Windows installer creation system with:

✅ **INI-based configuration** - Easy to write and maintain
✅ **Professional WPF wizard** - Modern installation interface
✅ **Full feature set** - Files, registry, shortcuts, scripts
✅ **Compiler tool** - Build installers from config files
✅ **Uninstaller** - Automatic Windows-compliant uninstaller
✅ **Comprehensive documentation** - Over 300 pages of guides
✅ **Real-world examples** - Game, web app, PowerShell deployment

## Project Structure

```
UniversalInstaller/
├── UniversalInstaller.Core/          # Core library
│   ├── Models/                       # Configuration models
│   ├── Configuration/                # INI parser
│   ├── Installation/                 # Installation engine
│   └── Utilities/                    # Path resolver, shortcuts
│
├── UniversalInstaller.Wizard/        # WPF wizard application
│   └── Pages/                        # Welcome, License, Directory, etc.
│
├── UniversalInstaller.Compiler/      # Compiler CLI tool
├── UniversalInstaller.Uninstaller/   # Uninstaller application
│
├── Documentation/
│   ├── README.md                     # Complete documentation
│   └── QUICK-START.md                # 5-minute quick start
│
└── Examples/
    ├── sample-app.ini                # Basic application
    ├── powershell-script.ini         # PowerShell deployment
    ├── web-application.ini           # IIS web app
    └── game-installer.ini            # Game with prerequisites
```

## Quick Test

### 1. Build the Solution

```bash
cd UniversalInstaller
dotnet build
```

### 2. Create a Test Config

Create `test.ini`:

```ini
[Setup]
AppName=Test Application
AppVersion=1.0
DefaultDirName={pf}\TestApp

[Files]
Source=README.md
DestDir={app}

[Icons]
Name={group}\Test App
Filename={app}\README.md
```

### 3. Build the Installer

```bash
dotnet run --project UniversalInstaller.Compiler test.ini
```

### 4. Test the Installer

Run `Output\setup.exe` to see the wizard in action!

## Next Steps

### Learn the Basics
- Read [QUICK-START.md](Documentation/QUICK-START.md) for common patterns
- Review [README.md](Documentation/README.md) for complete reference

### Study Examples
- [Examples/sample-app.ini](Examples/sample-app.ini) - Basic application
- [Examples/powershell-script.ini](Examples/powershell-script.ini) - Scripts
- [Examples/web-application.ini](Examples/web-application.ini) - IIS deployment
- [Examples/game-installer.ini](Examples/game-installer.ini) - Game with DirectX

### Key Features to Explore

#### File Installation
```ini
[Files]
Source=MyApp.exe
DestDir={app}

Source=*.dll
DestDir={app}

Source=Resources
DestDir={app}\Resources
Recurse=yes
```

#### Registry Operations
```ini
[Registry]
Root=HKLM
Subkey=Software\MyCompany\MyApp
ValueName=InstallPath
ValueType=string
ValueData={app}
```

#### Shortcuts
```ini
[Icons]
Name={userdesktop}\My App
Filename={app}\MyApp.exe

Name={group}\My App
Filename={app}\MyApp.exe
```

#### Script Execution
```ini
[Run]
Filename=powershell.exe
Parameters=-File "{app}\setup.ps1"
Description=Configuring application
```

#### Components
```ini
[Components]
Name=core
Description=Core files
Fixed=yes

Name=plugins
Description=Optional plugins

[Files]
Source=plugin.dll
DestDir={app}
Components=plugins
```

## Path Variables

Use these in any path:

- `{app}` - Installation directory
- `{pf}` - C:\Program Files
- `{userappdata}` - User AppData\Roaming
- `{localappdata}` - User AppData\Local
- `{userdesktop}` - User Desktop
- `{userdocs}` - User Documents
- `{group}` - Start Menu folder

[See complete list](Documentation/README.md#path-variables)

## Building Installers

### Command Line

```bash
# Basic
UniversalInstaller.Compiler.exe config.ini

# Custom output directory
UniversalInstaller.Compiler.exe config.ini C:\Build\Output
```

### Using dotnet run

```bash
dotnet run --project UniversalInstaller.Compiler -- config.ini
```

## Configuration Sections

### [Setup] - Application Info
Required. Basic app information, installer settings, wizard options.

### [Files] - Files to Install
Required. Source files and destination directories.

### [Dirs] - Directories to Create
Optional. Additional directories to create.

### [Icons] - Shortcuts
Optional. Start Menu, Desktop, QuickLaunch shortcuts.

### [Registry] - Registry Entries
Optional. Registry keys and values.

### [Run] - Scripts to Execute
Optional. Commands to run during/after installation.

### [Components] - Optional Components
Optional. User-selectable components.

### [Tasks] - Optional Tasks
Optional. User-configurable tasks (like desktop icons).

## Common Patterns

### Desktop Shortcut (Optional)
```ini
[Tasks]
Name=desktopicon
Description=Create desktop shortcut

[Icons]
Name={userdesktop}\My App
Filename={app}\MyApp.exe
Tasks=desktopicon
```

### File Association
```ini
[Registry]
Root=HKCR
Subkey=.myext
ValueType=string
ValueData=MyApp.File

Root=HKCR
Subkey=MyApp.File\shell\open\command
ValueType=string
ValueData="{app}\MyApp.exe" "%1"
```

### Post-Install Script
```ini
[Run]
Filename={app}\setup.bat
Description=Running setup
```

## Tips

1. **Start simple** - Use minimal config first, then add features
2. **Use examples** - Copy from Examples/ and modify
3. **Test often** - Build and test after each change
4. **Check docs** - Full reference in Documentation/README.md
5. **Use paths** - Always use `{app}` and path variables
6. **License page** - Point to existing file with `LicenseFile=`

## Troubleshooting

### Build Errors
- Ensure .NET 10.0 SDK is installed
- Run `dotnet restore` first
- Check for typos in .csproj files

### Installer Issues
- Verify all `Source` files exist
- Check paths are relative to .ini file
- Test with admin privileges for registry/HKLM

### Runtime Errors
- Check installation log in wizard
- Verify path variables resolve correctly
- Test scripts independently first

## Resources

- [Complete Documentation](Documentation/README.md)
- [Quick Start Guide](Documentation/QUICK-START.md)
- [Example Configurations](Examples/)
- [Project README](README.md)

## What Can You Build?

✅ **Desktop Applications** - Standard Windows apps
✅ **Web Applications** - IIS deployments with database setup
✅ **Games** - With DirectX/runtime prerequisites
✅ **Utilities** - PowerShell modules, CLI tools
✅ **Drivers** - Device drivers with INF registration
✅ **Services** - Windows services installation
✅ **Plugins** - Component-based installations

## Support

Having issues? Check:
1. [Documentation/README.md](Documentation/README.md) - Complete reference
2. [Examples/](Examples/) - Working configurations
3. Build output - Look for error messages

---

**Ready to create your first installer?**

```bash
# 1. Create your config file
notepad myapp.ini

# 2. Build the installer
dotnet run --project UniversalInstaller.Compiler -- myapp.ini

# 3. Test it!
Output\setup.exe
```

Happy installing! 🚀

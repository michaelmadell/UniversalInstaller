# Universal Installer

A powerful, flexible Windows installer creation system similar to Inno Setup, using INI configuration files.

## Features

- **INI-based Configuration**: Simple, human-readable configuration format
- **Modern WPF Wizard**: Professional installation wizard interface
- **Comprehensive Installation Support**:
  - File and directory management
  - Windows Registry operations
  - Shortcut creation (Start Menu, Desktop, etc.)
  - PowerShell/CMD script execution
  - Component-based installation
  - Custom tasks
- **Path Variables**: Extensive support for Windows special folders
- **License & Readme Pages**: Built-in EULA and readme display
- **Automatic Uninstaller**: Windows-compliant uninstaller generation
- **Compression**: Optional file compression
- **Customizable**: Extensive configuration options

## Quick Start

### 1. Build the Project

```bash
dotnet build UniversalInstaller.sln
```

### 2. Create a Configuration File

Create `myapp.ini`:

```ini
[Setup]
AppName=My Application
AppVersion=1.0.0
AppPublisher=My Company
DefaultDirName={pf}\MyApp
OutputBaseFilename=MyAppSetup

[Files]
Source=MyApp.exe
DestDir={app}

[Icons]
Name={group}\My Application
Filename={app}\MyApp.exe
```

### 3. Compile the Installer

```bash
UniversalInstaller.Compiler.exe myapp.ini
```

### 4. Distribute

The installer will be created in the `Output` directory as `MyAppSetup.exe`.

## Project Structure

```
UniversalInstaller/
├── UniversalInstaller.Core/          # Core library (models, parsers, installation engine)
├── UniversalInstaller.Wizard/        # WPF installer wizard application
├── UniversalInstaller.Compiler/      # Compiler tool to build installers
├── Documentation/                    # Complete documentation
│   ├── README.md                    # Full documentation
│   └── QUICK-START.md              # Quick start guide
└── Examples/                        # Example configurations
    ├── sample-app.ini              # Basic application
    ├── powershell-script.ini       # PowerShell deployment
    ├── web-application.ini         # IIS web app
    └── game-installer.ini          # Game with prerequisites
```

## Components

### UniversalInstaller.Core
Core library containing:
- Configuration models
- INI parser
- Installation engine
- Registry operations
- Path resolution utilities

### UniversalInstaller.Wizard
WPF application providing:
- Professional wizard interface
- Welcome, License, Directory, Components, Installation, and Finish pages
- Progress tracking
- Installation logging

### UniversalInstaller.Compiler
Command-line tool to build installers from INI configuration files.

## Documentation

- [Complete Documentation](Documentation/README.md) - Full reference guide
- [Quick Start Guide](Documentation/QUICK-START.md) - Get started in 5 minutes
- [Examples](Examples/) - Sample configurations for various scenarios

## Supported Features

### Path Variables
- `{app}` - Application directory
- `{pf}` - Program Files
- `{userappdata}` - User AppData
- `{localappdata}` - Local AppData
- And many more Windows special folders

### Registry Operations
- Support for HKLM, HKCU, HKCR, HKU, HKCC
- String, DWORD, QWORD, Binary values
- File associations
- Application registration

### Script Execution
- PowerShell scripts
- Batch files
- Executable files
- Pre and post-install hooks

### Components & Tasks
- Optional component selection
- User-configurable tasks
- Conditional file installation

## Example Configurations

### Basic Application
```ini
[Setup]
AppName=Simple App
AppVersion=1.0
DefaultDirName={pf}\SimpleApp

[Files]
Source=SimpleApp.exe
DestDir={app}

[Icons]
Name={group}\Simple App
Filename={app}\SimpleApp.exe
```

### With Desktop Shortcut
```ini
[Tasks]
Name=desktopicon
Description=Create desktop shortcut

[Icons]
Name={userdesktop}\My App
Filename={app}\MyApp.exe
Tasks=desktopicon
```

### PowerShell Deployment
```ini
[Run]
Filename=powershell.exe
Parameters=-File "{app}\setup.ps1"
Description=Configuring application
```

See the [Examples](Examples/) directory for more complete examples.

## Building from Source

### Prerequisites
- .NET 10.0 SDK or later
- Windows 10/11
- Visual Studio 2022 (optional, for development)

### Build Commands

```bash
# Build solution
dotnet build UniversalInstaller.sln

# Build specific project
dotnet build UniversalInstaller.Wizard/UniversalInstaller.Wizard.csproj

# Build in Release mode
dotnet build -c Release

# Run the compiler
dotnet run --project UniversalInstaller.Compiler -- myapp.ini
```

## Usage

### Compiling an Installer

```bash
UniversalInstaller.Compiler.exe <config.ini> [output_directory]
```

Parameters:
- `config.ini` - Path to INI configuration file
- `output_directory` - (Optional) Output directory for installer

Examples:
```bash
# Basic usage
UniversalInstaller.Compiler.exe myapp.ini

# Custom output directory
UniversalInstaller.Compiler.exe myapp.ini C:\Build\Output
```

### Running the Installer

Double-click the generated installer or run from command line:
```bash
MyAppSetup.exe
```

## Configuration File Format

The installer uses INI format with sections:

- `[Setup]` - Application and installer settings
- `[Files]` - Files to install
- `[Dirs]` - Directories to create
- `[Icons]` - Shortcuts to create
- `[Registry]` - Registry entries
- `[Run]` - Scripts to execute
- `[Components]` - Optional components
- `[Tasks]` - Optional tasks

See the [full documentation](Documentation/README.md) for complete reference.

## Contributing

Contributions are welcome! Areas for improvement:
- Additional wizard themes
- More compression options
- Digital signature support
- Update mechanism
- Localization/multi-language support
- Custom action plugins

## License

This project is open-source. See LICENSE file for details.

## Version History

### v1.0.0 (Current)
- Initial release
- INI-based configuration
- File and directory installation
- Registry operations
- Shortcut creation
- Script execution support
- Component and task system
- Modern WPF wizard interface
- Automatic uninstaller generation
- Comprehensive documentation

## Support

For issues, questions, or contributions:
- Check the [documentation](Documentation/README.md)
- Review [examples](Examples/)
- Report bugs via issue tracker

---

**Universal Installer** - Create professional Windows installers with ease!

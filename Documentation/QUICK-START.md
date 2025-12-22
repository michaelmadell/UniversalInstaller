# Universal Installer - Quick Start Guide

## Installation in 5 Minutes

### Step 1: Prepare Your Files

Create a directory structure like this:

```
MyApp/
  ├── MyApp.exe           # Your application
  ├── license.txt         # License agreement
  ├── readme.txt          # Readme file
  ├── installer.ini       # Configuration file
  └── files/              # Additional files
```

### Step 2: Create installer.ini

Create a basic configuration file:

```ini
[Setup]
AppName=My Application
AppVersion=1.0.0
AppPublisher=My Company
DefaultDirName={pf}\MyApp
OutputBaseFilename=MyAppSetup
LicenseFile=license.txt
ShowLicense=yes

[Files]
Source=MyApp.exe
DestDir={app}

Source=files\*.*
DestDir={app}

[Icons]
Name={group}\My Application
Filename={app}\MyApp.exe

[Dirs]
Name={app}\data
```

### Step 3: Build the Installer

Run the compiler:

```bash
UniversalInstaller.Compiler.exe installer.ini
```

### Step 4: Test

Run the generated `Output\MyAppSetup.exe` and test the installation!

---

## Common Configuration Patterns

### Desktop Shortcut

```ini
[Tasks]
Name=desktopicon
Description=Create a desktop shortcut

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
Filename=powershell.exe
Parameters=-File "{app}\setup.ps1"
Description=Configuring application
```

### Optional Components

```ini
[Components]
Name=core
Description=Core files
Fixed=yes

Name=extras
Description=Extra features

[Files]
Source=core.dll
DestDir={app}
Components=core

Source=extras.dll
DestDir={app}
Components=extras
```

---

## Tips & Tricks

### Use Wildcards

```ini
[Files]
Source=*.dll
DestDir={app}
```

### Copy Entire Directories

```ini
[Files]
Source=resources
DestDir={app}\resources
Recurse=yes
```

### Create User Data Directory

```ini
[Dirs]
Name={userappdata}\MyApp
```

### Add Uninstaller to Start Menu

```ini
[Icons]
Name={group}\Uninstall My App
Filename={app}\uninstall.exe
```

---

## Minimal Configuration

The absolute minimum configuration:

```ini
[Setup]
AppName=My App
AppVersion=1.0
DefaultDirName={pf}\MyApp

[Files]
Source=MyApp.exe
DestDir={app}
```

This creates a basic installer that:
- Installs one file
- Creates an uninstaller
- Registers in Add/Remove Programs

---

## Next Steps

1. Read the full [documentation](README.md)
2. Check [sample configurations](../Examples/)
3. Customize your installer
4. Test thoroughly
5. Distribute!

---

**Happy installing!**

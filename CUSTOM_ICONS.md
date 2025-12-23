# Custom Icons Guide

This guide explains how to add custom icons to your Universal Installer executables.

## Overview

The Universal Installer system supports custom icons for:
1. **Wizard/Installer executable** - The main installer that users run
2. **Uninstaller executable** - The uninstaller extracted during installation
3. **Wizard sidebar images** - Images shown on the Welcome and Finish pages

---

## 1. Installer (Wizard) Icon

### Default Icon Location
Place your icon file at: `UniversalInstaller.Wizard/app.ico`

### How It Works
- The `UniversalInstaller.Wizard.csproj` file references `app.ico` via the `<ApplicationIcon>` property
- When you build/publish the Wizard, .NET embeds this icon into the .exe file
- The icon appears in Windows Explorer, taskbar, and title bar

### Creating the Icon
- **Format**: Windows ICO file (`.ico`)
- **Recommended sizes**: Include multiple resolutions in one file:
  - 16x16 (small icons, title bar)
  - 32x32 (standard icons)
  - 48x48 (large icons)
  - 256x256 (high-DPI displays)
- **Tools**: Use free tools like:
  - GIMP (export as ICO)
  - IcoFX
  - Online converters (png-to-ico.com)

### Example
```
UniversalInstaller.Wizard/
├── app.ico          ← Your custom installer icon
├── MainWindow.xaml
└── ...
```

---

## 2. Uninstaller Icon

### Default Icon Location
Place your icon file at: `UniversalInstaller.Uninstaller/uninstall.ico`

### How It Works
- The `UniversalInstaller.Uninstaller.csproj` file references `uninstall.ico`
- When the uninstaller is published and embedded into the wizard, it includes this icon
- The icon appears when viewing the uninstaller in the installation directory

### Typical Uninstaller Icon Design
- Red/orange color scheme (indicates removal/deletion)
- Common symbols: X, trash can, removal arrow, or crossed-out box
- Should be visually distinct from the installer icon

### Example
```
UniversalInstaller.Uninstaller/
├── uninstall.ico    ← Your custom uninstaller icon
├── Program.cs
└── ...
```

---

## 3. Wizard Sidebar Images

### Default Image Location
Place your image file at: `UniversalInstaller.Wizard/Resources/Images/wizard.png`

### How It Works
- Referenced in `WelcomePage.xaml` and `FinishPage.xaml` as `/Resources/Images/wizard.png`
- Displayed on the left sidebar of the Welcome and Finish pages
- Embedded as a WPF resource when the wizard is built

### Image Specifications
- **Format**: PNG (recommended for transparency) or JPG
- **Recommended size**: 164x400 pixels (tall and narrow)
- **Color scheme**: Should match the sidebar background (#2B579A - dark blue)
- **Style**: Can be:
  - Logo/branding
  - Thematic artwork (wizard hat, installation boxes, etc.)
  - Abstract pattern
  - Retro pixel art (if matching a retro aesthetic)

### Adding the Resource
Add this to `UniversalInstaller.Wizard.csproj` inside an `<ItemGroup>`:
```xml
<ItemGroup>
  <Resource Include="Resources\Images\wizard.png" />
</ItemGroup>
```

### Example Structure
```
UniversalInstaller.Wizard/
├── Resources/
│   └── Images/
│       └── wizard.png  ← Your custom sidebar image
├── app.ico
└── ...
```

---

## Quick Start: Retro 64x64 Pixel Art Style

If you want a retro aesthetic, here are some ideas:

### Color Palette
- **Primary Blue**: #2B579A (sidebar background)
- **Accent Gold**: #FFD700 (stars, highlights)
- **White**: #FFFFFF (details, sparkles)
- **Dark**: #1E1E1E (outlines, shadows)

### Icon Themes
1. **Classic Installer**: Floppy disk with sparkles
2. **Wizard Hat**: Traditional wizard hat with stars
3. **Toolbox**: Open toolbox with tools floating out
4. **Gears**: Interlocking gears assembling
5. **Computer**: Retro computer with download arrow

### For Uninstaller
1. **Red X**: Bold red X on a box
2. **Trash Can**: Pixel art trash can with lid
3. **Eraser**: Large eraser removing items
4. **Crossed Box**: Box with diagonal red line

---

## Advanced: Per-Installer Custom Icons

If you want each compiled installer to have its own custom icon (not just the wizard default), you can:

### Option 1: Post-Build Icon Replacement
Use a tool like ResourceHacker or RCEdit to replace the icon after compilation:
```bash
rcedit.exe "YourInstaller.exe" --set-icon "custom-icon.ico"
```

### Option 2: SetupIcon Configuration
Add support for a `SetupIcon` parameter in the INI file:
```ini
[Setup]
SetupIcon=myapp-icon.ico
```

Then modify the compiler to copy the icon and update the wizard's icon before embedding.

---

## Testing Your Icons

After adding custom icons:

1. **Build the projects**:
   ```bash
   dotnet build UniversalInstaller.sln
   ```

2. **Publish the wizard**:
   ```bash
   dotnet publish UniversalInstaller.Wizard/UniversalInstaller.Wizard.csproj -c Debug -r win-x64 --self-contained false -p:PublishSingleFile=true
   ```

3. **Check the icon**:
   - Navigate to `UniversalInstaller.Wizard/bin/Debug/net10.0-windows/win-x64/publish/`
   - Look at `UniversalInstaller.Wizard.exe` in Windows Explorer
   - The icon should be visible

4. **Compile an installer**:
   ```bash
   UniversalInstaller.Compiler.exe your-config.ini OutputDir
   ```

5. **Verify**:
   - Check `OutputDir/YourInstaller.exe` - should have the wizard icon
   - Install the app
   - Check the uninstaller in the installation directory - should have the uninstaller icon

---

## Troubleshooting

### Icon doesn't appear
- Make sure the icon file exists at the specified path
- Rebuild the project completely (`dotnet clean` then `dotnet build`)
- Check that the icon file is a valid Windows ICO format
- Ensure you published with `PublishSingleFile=true`

### Icon is blurry
- Include multiple resolutions in your ICO file (16x16, 32x32, 48x48, 256x256)
- Use proper image editing tools rather than just renaming a PNG to ICO

### Wizard sidebar image doesn't show
- Verify the file exists at `Resources/Images/wizard.png`
- Check that it's marked as a `<Resource>` in the .csproj
- Rebuild the Wizard project
- Republish before compiling a new installer

---

## Example: Adding Icons to CoreStation HX Agent

For your CoreStation HX Agent installer:

1. Create `app.ico` with your company logo
2. Create `uninstall.ico` with a red/orange removal icon
3. Create `wizard.png` (164x400) with your branding
4. Place files in the correct locations
5. Rebuild and publish:
   ```bash
   dotnet publish UniversalInstaller.Uninstaller/UniversalInstaller.Uninstaller.csproj -c Debug -r win-x64 --self-contained false -p:PublishSingleFile=true
   dotnet publish UniversalInstaller.Wizard/UniversalInstaller.Wizard.csproj -c Debug -r win-x64 --self-contained false -p:PublishSingleFile=true
   UniversalInstaller.Compiler.exe Examples/corestation-hx-agent.ini TestOutput
   ```

Your installer will now have custom branding throughout!

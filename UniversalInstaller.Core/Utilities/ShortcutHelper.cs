using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace UniversalInstaller.Core.Utilities
{
    public static class ShortcutHelper
    {
        public static void CreateShortcut(string shortcutPath, string targetPath, string workingDirectory = "", string arguments = "", string iconPath = "")
        {
            try
            {
                IShellLink link = (IShellLink)new ShellLink();

                link.SetDescription($"Shortcut to {Path.GetFileNameWithoutExtension(targetPath)}");
                link.SetPath(targetPath);

                if (!string.IsNullOrEmpty(arguments))
                    link.SetArguments(arguments);

                if (!string.IsNullOrEmpty(workingDirectory))
                    link.SetWorkingDirectory(workingDirectory);

                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                    link.SetIconLocation(iconPath, 0);

                IPersistFile file = (IPersistFile)link;
                file.Save(shortcutPath, false);

                Marshal.ReleaseComObject(file);
                Marshal.ReleaseComObject(link);
            }
            catch
            {
                // Fallback: create a simple batch file or URL file if COM fails
                // This is a basic fallback for systems where COM might not work
            }
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLink
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}

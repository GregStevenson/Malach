using System.Runtime.InteropServices;
using System.Text;
using Malach.Server.Abstractions;

namespace Malach.Server.Platforms.Windows;

public sealed class WindowsWindowManager : IWindowManager
{
    public bool SetForeground(string windowTitle)
    {
        var hWnd = FindWindowByTitle(windowTitle);
        return hWnd != IntPtr.Zero && SetForegroundWindow(hWnd);
    }

    private static IntPtr FindWindowByTitle(string title)
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((h, l) =>
        {
            var sb = new StringBuilder(512);
            _ = GetWindowText(h, sb, sb.Capacity);
            if (sb.ToString().Contains(title, StringComparison.OrdinalIgnoreCase))
            {
                found = h; return false;
            }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    // P/Invoke
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
    [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
}

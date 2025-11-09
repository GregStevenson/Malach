using System.Diagnostics;
using Malach.Server.Abstractions;

namespace Malach.Server.Platforms.Windows;

public sealed class WindowsFileActions : IFileActions
{
    public bool OpenPath(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            return true;
        }
        catch { return false; }
    }

    public bool Shell(string command)
    {
        try
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + command) { UseShellExecute = false, CreateNoWindow = true });
            return true;
        }
        catch { return false; }
    }
}

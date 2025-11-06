using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Malach.Core;

public interface ICommandExecutor
{
    void Launch(string exe, string? args);
    void Open(string path);
}

public class CommandExecutor : ICommandExecutor
{
    public void Launch(string exe, string? args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Start(new ProcessStartInfo(""open""){ ArgumentList = { ""-a"", exe } });
        else
            Start(new ProcessStartInfo(exe){ Arguments = args ?? """", UseShellExecute = true });
    }

    public void Open(string path)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Start(new ProcessStartInfo(""explorer.exe""){ ArgumentList = { path } });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Start(new ProcessStartInfo(""open""){ ArgumentList = { path } });
        else
            Start(new ProcessStartInfo(""xdg-open""){ ArgumentList = { path } });
    }

    static void Start(ProcessStartInfo psi)
    {
        psi.WorkingDirectory = Environment.CurrentDirectory;
        Process.Start(psi);
    }
}

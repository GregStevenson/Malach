using System.Runtime.InteropServices;
using Malach.Server.Abstractions;

namespace Malach.Server.Platforms.Windows;

public sealed class WindowsMouse : IMouse
{
    public bool Move(int x, int y)
    {
        // Absolute move (0..65535)
        var input = new INPUT
        {
            type = 0, // MOUSE
            U = new InputUnion { mi = new MOUSEINPUT { dx = x, dy = y, dwFlags = (uint)(MouseEventFlags.MOVE | MouseEventFlags.ABSOLUTE) } }
        };
        return SendInput(1, new[] { input }, INPUT.Size) > 0;
    }

    public bool Click(string button, bool dbl = false)
    {
        var ok = Down(button) && Up(button);
        if (dbl) ok = Down(button) && Up(button) && ok;
        return ok;
    }

    public bool Down(string button) => SendBtn(button, down: true);
    public bool Up(string button) => SendBtn(button, down: false);

    public bool Scroll(int vertical, int horizontal)
    {
        var events = new List<INPUT>();
        if (vertical != 0)
            events.Add(MouseWheel(vertical, false));
        if (horizontal != 0)
            events.Add(MouseWheel(horizontal, true));
        return events.Count == 0 || SendInput((uint)events.Count, events.ToArray(), INPUT.Size) > 0;
    }

    public (int x, int y)? GetPosition() => null; // not needed yet

    // --- helpers ---
    private static bool SendBtn(string button, bool down)
    {
        var (downFlag, upFlag) = button.ToLowerInvariant() switch
        {
            "right" => (MouseEventFlags.RIGHTDOWN, MouseEventFlags.RIGHTUP),
            "middle" => (MouseEventFlags.MIDDLEDOWN, MouseEventFlags.MIDDLEUP),
            _ => (MouseEventFlags.LEFTDOWN, MouseEventFlags.LEFTUP),
        };
        var flags = (uint)(down ? downFlag : upFlag);
        var input = new INPUT { type = 0, U = new InputUnion { mi = new MOUSEINPUT { dwFlags = flags } } };
        return SendInput(1, new[] { input }, INPUT.Size) > 0;
    }

    private static INPUT MouseWheel(int amount, bool horizontal) =>
        new INPUT
        {
            type = 0,
            U = new InputUnion
            {
                mi = new MOUSEINPUT
                {
                    mouseData = (uint)(amount * 120),
                    dwFlags = (uint)(horizontal ? MouseEventFlags.HWHEEL : MouseEventFlags.WHEEL)
                }
            }
        };

    // P/Invoke
    [StructLayout(LayoutKind.Sequential)] private struct INPUT { public uint type; public InputUnion U; public static int Size => Marshal.SizeOf<INPUT>(); }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; }
    [StructLayout(LayoutKind.Sequential)] private struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

    [Flags] private enum MouseEventFlags : uint { MOVE = 0x0001, LEFTDOWN = 0x0002, LEFTUP = 0x0004, RIGHTDOWN = 0x0008, RIGHTUP = 0x0010, MIDDLEDOWN = 0x0020, MIDDLEUP = 0x0040, WHEEL = 0x0800, HWHEEL = 0x01000, ABSOLUTE = 0x8000 }

    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}

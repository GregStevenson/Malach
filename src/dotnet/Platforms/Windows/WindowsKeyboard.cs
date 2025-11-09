using System.Runtime.InteropServices;
using Malach.Server.Abstractions;
using Malach.Server.Common;

namespace Malach.Server.Platforms.Windows;

public sealed class WindowsKeyboard : IKeyboard
{
    public bool Press(string key, string[] modifiers) => SendChord(key, modifiers);
    public bool Trio(string key, string[] modifiers) => SendChord(key, modifiers);
    public bool Quartet(string key, string[] modifiers) => SendChord(key, modifiers);
    public bool KeyDown(string key) => SendOne(key, true);
    public bool KeyUp(string key) => SendOne(key, false);

    public bool TypeText(string text)
    {
        foreach (var ch in text)
            if (!SendChar(ch)) return false;
        return true;
    }

    private static bool SendChord(string key, string[] mods)
    {
        var down = new List<ushort>();
        try
        {
            foreach (var m in mods)
            {
                if (KeyMap.TryGetModifier(m, out var vk))
                {
                    SendVk(vk, true); down.Add(vk);
                }
            }

            // Uppercase single letters: press Shift unless provided
            if (key is { Length: 1 } && char.IsLetter(key[0]) && char.IsUpper(key[0]) &&
                !mods.Any(s => string.Equals(s, "shift", StringComparison.OrdinalIgnoreCase)))
            {
                SendVk(KeyMap.VK_SHIFT, true);
                down.Add(KeyMap.VK_SHIFT);
                key = char.ToLowerInvariant(key[0]).ToString();
            }

            var vkKey = KeyMap.FromKeyToken(key);
            if (vkKey == 0) return false;
            TapVk(vkKey);
            return true;
        }
        finally
        {
            for (var i = down.Count - 1; i >= 0; i--) SendVk(down[i], false);
        }
    }

    private static bool SendOne(string key, bool isDown)
    {
        var vk = KeyMap.FromKeyToken(key);
        if (vk == 0) return false;
        SendVk(vk, isDown);
        return true;
    }

    private static bool SendChar(char ch)
    {
        var token = ch.ToString();
        var needShift = false;
        if (char.IsLetter(ch) && char.IsUpper(ch))
        {
            needShift = true;
            token = char.ToLowerInvariant(ch).ToString();
        }

        var vk = KeyMap.FromKeyToken(token);
        if (vk == 0) return false;

        if (needShift) SendVk(KeyMap.VK_SHIFT, true);
        TapVk(vk);
        if (needShift) SendVk(KeyMap.VK_SHIFT, false);
        return true;
    }

    private static void TapVk(ushort vk) { SendVk(vk, true); SendVk(vk, false); }

    private static void SendVk(ushort vk, bool down)
    {
        var input = new INPUT
        {
            type = 1,
            U = new InputUnion { ki = new KEYBDINPUT { wVk = vk, dwFlags = down ? 0u : 2u } }
        };
        _ = SendInput(1, new[] { input }, INPUT.Size);
    }

    [StructLayout(LayoutKind.Sequential)] private struct INPUT { public uint type; public InputUnion U; public static int Size => Marshal.SizeOf<INPUT>(); }
    [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; }
    [StructLayout(LayoutKind.Sequential)] private struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
    [DllImport("user32.dll", SetLastError = true)] private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}

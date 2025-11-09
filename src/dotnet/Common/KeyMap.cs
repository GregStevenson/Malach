namespace Malach.Server.Common;

public static class KeyMap
{
    public const ushort VK_SHIFT = 0x10;
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_MENU = 0x12; // Alt
    public const ushort VK_LWIN = 0x5B;

    public static bool TryGetModifier(string? token, out ushort vk)
    {
        switch ((token ?? "").Trim().ToLowerInvariant())
        {
            case "ctrl":
            case "control": vk = VK_CONTROL; return true;
            case "shift": vk = VK_SHIFT; return true;
            case "alt": vk = VK_MENU; return true;
            case "command":
            case "win":
            case "windows": vk = VK_LWIN; return true;
            default: vk = 0; return false;
        }
    }

    public static ushort FromKeyToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return 0;
        token = token.Trim();

        if (token.Length == 1)
        {
            var ch = token[0];
            if (char.IsLetter(ch))
                return (ushort)('A' + (char.ToUpperInvariant(ch) - 'A')); // VK_A..VK_Z
            if (char.IsDigit(ch))
                return (ushort)('0' + (ch - '0')); // VK_0..VK_9
        }

        var t = token.ToLowerInvariant();
        return t switch
        {
            "enter" => 0x0D,
            "tab" => 0x09,
            "escape" or "esc" => 0x1B,
            "space" => 0x20,
            "backspace" => 0x08,
            "delete" => 0x2E,
            "insert" => 0x2D,
            "home" => 0x24,
            "end" => 0x23,
            "up" or "arrow up" => 0x26,
            "down" or "arrow down" => 0x28,
            "left" or "arrow left" => 0x25,
            "right" or "arrow right" => 0x27,
            "pageup" or "page up" => 0x21,
            "pagedown" or "page down" => 0x22,
            "printscreen" => 0x2C,
            "pause" => 0x13,
            "scrolllock" or "scroll lock" => 0x91,
            "f1" => 0x70,
            "f2" => 0x71,
            "f3" => 0x72,
            "f4" => 0x73,
            "f5" => 0x74,
            "f6" => 0x75,
            "f7" => 0x76,
            "f8" => 0x77,
            "f9" => 0x78,
            "f10" => 0x79,
            "f11" => 0x7A,
            "f12" => 0x7B,
            "f13" => 0x7C,
            "f14" => 0x7D,
            "f15" => 0x7E,
            "f16" => 0x7F,
            "f17" => 0x80,
            "f18" => 0x81,
            "f19" => 0x82,
            "f20" => 0x83,
            "f21" => 0x84,
            "f22" => 0x85,
            "f23" => 0x86,
            "f24" => 0x87,
            _ => 0
        };
    }
}

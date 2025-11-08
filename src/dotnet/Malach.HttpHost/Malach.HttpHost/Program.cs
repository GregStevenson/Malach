// Target: .NET (current SDK). Your project says ".NET 10 / C# 14"—this will compile fine on 8/9 too.
// Minimal API with a single POST endpoint: /api/keys
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// App settings
var cfg = builder.Configuration;
var serverAuthToken = cfg["Server:BearerToken"] ?? "";     // HTTP Authorization: Bearer <token>
var vicreoPasswordMd5 = cfg["Server:VicreoPasswordMd5"] ?? ""; // Optional: MD5 expected in payload

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = null; // we use exact property names from VICREO (key, type, modifiers, msg, password)
});
builder.Services.AddLogging();
if (OperatingSystem.IsWindows())
    builder.Services.AddSingleton<IKeySender, WindowsKeySender>();
else
    builder.Services.AddSingleton<IKeySender, NoopKeySender>();
var app = builder.Build();

// Simple auth middleware for Bearer token
app.Use(async (ctx, next) =>
{
    if (string.IsNullOrWhiteSpace(serverAuthToken))
         await next();

    var auth = ctx.Request.Headers.Authorization.ToString();
    if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
        auth["Bearer ".Length..].Trim() != serverAuthToken)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await ctx.Response.WriteAsync("Unauthorized");
        return;
    }

    await next();
});

app.MapPost("/api/keys", async ([FromBody] VicReoRequest req, IKeySender keys, ILoggerFactory lf) =>
{
    var log = lf.CreateLogger("KeyServer");

    // Optional VICREO password check (expected MD5 hex in config)
    if (!string.IsNullOrEmpty(vicreoPasswordMd5))
    {
        var got = (req.password ?? "").Trim().ToLowerInvariant();
        if (!string.Equals(got, vicreoPasswordMd5.Trim().ToLowerInvariant(), StringComparison.Ordinal))
        {
            log.LogWarning("Password MD5 mismatch");
            return Results.BadRequest(new { ok = false, error = "password" });
        }
    }

    var t = (req.type ?? "").Trim().ToLowerInvariant();
    log.LogInformation("Req: {Type} key={Key} mods=[{Mods}] msgLen={Len}",
        t, req.key, req.modifiers != null ? string.Join(",", req.modifiers) : "", req.msg?.Length ?? 0);

    // Normalize modifiers
    var mods = (req.modifiers ?? Array.Empty<string>())
        .Select(s => s?.Trim().ToLowerInvariant())
        .Where(s => !string.IsNullOrEmpty(s))
        .ToArray();

    // Route by type
    switch (t)
    {
        case "press":
        case "pressspecial":
            {
                var ok = keys.PressKey(req.key ?? "", mods);
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "combination":
            {
                var ok = keys.Combination(req.key ?? "", mods);
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "trio":
            {
                var ok = keys.Trio(req.key ?? "", mods);
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "quartet":
            {
                var ok = keys.Quartet(req.key ?? "", mods);
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "down":
            {
                var ok = keys.KeyDown(req.key ?? "");
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "up":
            {
                var ok = keys.KeyUp(req.key ?? "");
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        case "string":
            {
                var text = req.msg ?? "";
                var ok = keys.TypeString(text);
                return ok ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false });
            }

        default:
            return Results.BadRequest(new { ok = false, error = "unsupported type" });
    }
});

app.Run();

// --- Models ---
public record VicReoRequest
{
    public string? key { get; init; }
    public string? type { get; init; }
    public string[]? modifiers { get; init; }
    public string? msg { get; init; }
    public string? password { get; init; }  // MD5 hex
}

// --- Services ---
public interface IKeySender
{
    bool PressKey(string key, string[] modifiers);
    bool Combination(string key, string[] modifiers);
    bool Trio(string key, string[] modifiers);
    bool Quartet(string key, string[] modifiers);
    bool KeyDown(string key);
    bool KeyUp(string key);
    bool TypeString(string text);
}

public sealed class NoopKeySender : IKeySender
{
    public bool PressKey(string key, string[] modifiers) => true;
    public bool Combination(string key, string[] modifiers) => true;
    public bool Trio(string key, string[] modifiers) => true;
    public bool Quartet(string key, string[] modifiers) => true;
    public bool KeyDown(string key) => true;
    public bool KeyUp(string key) => true;
    public bool TypeString(string text) => true;
}

#if WINDOWS
public sealed class WindowsKeySender : IKeySender
{
    public bool PressKey(string key, string[] modifiers)
        => SendChord(key, modifiers);

    public bool Combination(string key, string[] modifiers)
        => SendChord(key, modifiers);

    public bool Trio(string key, string[] modifiers)
        => SendChord(key, modifiers);

    public bool Quartet(string key, string[] modifiers)
        => SendChord(key, modifiers);

    public bool KeyDown(string key)
        => SendOne(key, true);

    public bool KeyUp(string key)
        => SendOne(key, false);

    public bool TypeString(string text)
    {
        foreach (var ch in text)
        {
            if (!SendChar(ch)) return false;
        }
        return true;
    }

    // --- Implementation ---
    private static bool SendChord(string key, string[] mods)
    {
        var keysDown = new List<ushort>();
        try
        {
            foreach (var m in mods)
            {
                if (KeyMap.TryGetModifier(m, out var vk))
                {
                    SendVk(vk, true);
                    keysDown.Add(vk);
                }
            }

            // If key is uppercase single letter, hold SHIFT to produce uppercase.
            if (key is { Length: 1 } && char.IsLetter(key[0]) && char.IsUpper(key[0]) && !mods.Contains("shift", StringComparer.OrdinalIgnoreCase))
            {
                SendVk(KeyMap.VK_SHIFT, true);
                keysDown.Add(KeyMap.VK_SHIFT);
                var baseChar = char.ToLowerInvariant(key[0]);
                var vk = KeyMap.FromKeyToken(baseChar.ToString());
                if (vk == 0) return false;
                TapVk(vk);
            }
            else
            {
                var vk = KeyMap.FromKeyToken(key);
                if (vk == 0) return false;
                TapVk(vk);
            }
            return true;
        }
        finally
        {
            // release in reverse order
            for (var i = keysDown.Count - 1; i >= 0; i--)
                SendVk(keysDown[i], false);
        }
    }

    private static bool SendOne(string key, bool down)
    {
        var vk = KeyMap.FromKeyToken(key);
        if (vk == 0) return false;
        SendVk(vk, down);
        return true;
    }

    private static bool SendChar(char ch)
    {
        // use Vk mapping + shift for upper/digits where needed
        var token = ch.ToString();
        var needShift = false;

        if (char.IsLetter(ch))
        {
            if (char.IsUpper(ch)) { needShift = true; token = char.ToLowerInvariant(ch).ToString(); }
        }
        // digits 0-9 fall through as themselves

        var vk = KeyMap.FromKeyToken(token);
        if (vk == 0) return false;

        if (needShift)
            SendVk(KeyMap.VK_SHIFT, true);

        TapVk(vk);

        if (needShift)
            SendVk(KeyMap.VK_SHIFT, false);

        return true;
    }

    private static void TapVk(ushort vk)
    {
        SendVk(vk, true);
        SendVk(vk, false);
    }

    private static void SendVk(ushort vk, bool down)
    {
        INPUT input = new INPUT
        {
            type = 1, // keyboard
            U = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vk,
                    wScan = 0,
                    dwFlags = down ? 0u : 2u, // KEYEVENTF_KEYUP
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };
        var sent = SendInput(1, new[] { input }, INPUT.Size);
        if (sent == 0)
        {
            // You can add error handling via Marshal.GetLastWin32Error()
        }
    }

    // P/Invoke
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
        public static int Size => System.Runtime.InteropServices.Marshal.SizeOf<INPUT>();
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    private struct InputUnion
    {
        [System.Runtime.InteropServices.FieldOffset(0)] public KEYBDINPUT ki;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
}
#endif

// --- KeyMap ---
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
            case "control":
                vk = VK_CONTROL; return true;
            case "shift":
                vk = VK_SHIFT; return true;
            case "alt":
                vk = VK_MENU; return true;
            case "command":
            case "win":
            case "windows":
                vk = VK_LWIN; return true;
            default:
                vk = 0; return false;
        }
    }

    public static ushort FromKeyToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return 0;
        token = token.Trim();

        // Map well-known names to VK_*
        var t = token.ToLowerInvariant();

        // Single letters/digits to VK
        if (t.Length == 1)
        {
            if (char.IsLetter(t[0]))
                return (ushort)('A' + (char.ToUpperInvariant(t[0]) - 'A')); // VK_A..VK_Z
            if (char.IsDigit(t[0]))
                return (ushort)('0' + (t[0] - '0')); // VK_0..VK_9
        }

        return t switch
        {
            // specials
            "enter" => 0x0D,
            "tab" => 0x09,
            "escape" => 0x1B,
            "esc" => 0x1B,
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

            // F-keys
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

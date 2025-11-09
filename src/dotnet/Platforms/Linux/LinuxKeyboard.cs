using Malach.Server.Abstractions;

namespace Malach.Server.Platforms.Linux;

public sealed class LinuxKeyboard : IKeyboard
{
    public bool Press(string key, string[] modifiers) => true;
    public bool Trio(string key, string[] modifiers) => true;
    public bool Quartet(string key, string[] modifiers) => true;
    public bool KeyDown(string key) => true;
    public bool KeyUp(string key) => true;
    public bool TypeText(string text) => true;
}

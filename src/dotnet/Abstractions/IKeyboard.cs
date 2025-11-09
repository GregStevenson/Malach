namespace Malach.Server.Abstractions;

public interface IKeyboard
{
    bool Press(string key, string[] modifiers);
    bool Trio(string key, string[] modifiers);
    bool Quartet(string key, string[] modifiers);
    bool KeyDown(string key);
    bool KeyUp(string key);
    bool TypeText(string text);
}

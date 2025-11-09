namespace Malach.Server.Abstractions;

public interface IMouse
{
    bool Move(int x, int y);
    bool Click(string button, bool dbl = false);
    bool Down(string button);
    bool Up(string button);
    bool Scroll(int vertical, int horizontal);
    (int x, int y)? GetPosition();
}

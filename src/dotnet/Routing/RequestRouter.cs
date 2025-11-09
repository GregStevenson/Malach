// Routing/RequestRouter.cs
using Malach.Server.Abstractions;
using Malach.Server.Common.Models;

public sealed class RequestRouter {
  private readonly IKeyboard _kbd;
  private readonly IMouse _mouse;
  private readonly IWindowManager _win;
  private readonly IFileActions _file;

  public RequestRouter(IKeyboard kbd, IMouse mouse, IWindowManager win, IFileActions file) {
    _kbd = kbd; _mouse = mouse; _win = win; _file = file;
  }

  public bool Handle(VicReoRequest r) {
    var t = (r.type ?? "").ToLowerInvariant();
    switch (t) {
      case "press":
      case "pressspecial": return _kbd.Press(r.key ?? "", r.modifiers ?? []);
      case "combination":  return _kbd.Press(r.key ?? "", r.modifiers ?? []);
      case "trio":         return _kbd.Trio(r.key ?? "", r.modifiers ?? []);
      case "quartet":      return _kbd.Quartet(r.key ?? "", r.modifiers ?? []);
      case "down":         return _kbd.KeyDown(r.key ?? "");
      case "up":           return _kbd.KeyUp(r.key ?? "");
      case "string":       return _kbd.TypeText(r.msg ?? "");
      case "mouseposition":return _mouse.Move(Parse(r, "x"), Parse(r, "y")); // if you add x/y to model
      case "mouseclick":   return _mouse.Click(r.key ?? "left", false);
      case "mouseclickhold": return _mouse.Down(r.key ?? "left");
      case "mouseclickrelease": return _mouse.Up(r.key ?? "left");
      case "mousescroll":  return _mouse.Scroll(Parse(r, "vertical"), Parse(r, "horizontal"));
      case "setwindowtoforeground": return _win.SetForeground(r.msg ?? "");
      case "file":         return _file.OpenPath(r.msg ?? "");
      case "shell":        return _file.Shell(r.msg ?? "");
      default:             return false;
    }
  }

  private static int Parse(VicReoRequest r, string name) {
    // extend VicReoRequest with x/y/vertical/horizontal if you want strict types
    return 0;
  }
}

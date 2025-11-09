// Program.cs
using Malach.Server.Abstractions;
using Malach.Server.Platforms.Windows;
using Malach.Server.Platforms.Mac;
using Malach.Server.Platforms.Linux;

if (OperatingSystem.IsWindows()) {
  builder.Services.AddSingleton<IKeyboard, WindowsKeyboard>();
  builder.Services.AddSingleton<IMouse, WindowsMouse>();
  builder.Services.AddSingleton<IWindowManager, WindowsWindowManager>();
  builder.Services.AddSingleton<IFileActions, WindowsFileActions>();
}
else if (OperatingSystem.IsMacOS()) {
  builder.Services.AddSingleton<IKeyboard, MacKeyboard>();
  builder.Services.AddSingleton<IMouse, MacMouse>();
  builder.Services.AddSingleton<IWindowManager, MacWindowManager>();
  builder.Services.AddSingleton<IFileActions, MacFileActions>();
}
else {
  builder.Services.AddSingleton<IKeyboard, LinuxKeyboard>();
  builder.Services.AddSingleton<IMouse, LinuxMouse>();
  builder.Services.AddSingleton<IWindowManager, LinuxWindowManager>();
  builder.Services.AddSingleton<IFileActions, LinuxFileActions>();
}

// Always cross-platform
builder.Services.AddSingleton<IUtility, Utility>();

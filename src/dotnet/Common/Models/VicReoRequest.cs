namespace Malach.Server.Common.Models;

public record VicReoRequest
{
    public string? key { get; init; }
    public string? type { get; init; }
    public string[]? modifiers { get; init; }
    public string? msg { get; init; }
    public string? password { get; init; }

    // Optional future fields for mouse/window:
    public int? x { get; init; }
    public int? y { get; init; }
    public int? vertical { get; init; }
    public int? horizontal { get; init; }
}

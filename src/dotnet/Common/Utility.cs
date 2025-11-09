using System.Security.Cryptography;
using System.Text;
using Malach.Server.Abstractions;

namespace Malach.Server.Common;

public sealed class Utility : IUtility
{
    public string Md5Hex(string input)
    {
        using var md5 = MD5.Create();
        var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input ?? ""));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}

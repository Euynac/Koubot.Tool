using System.IO;

namespace Koubot.Tool.Extensions;

public static class ResourcesExtensions
{
    public static MemoryStream ToMemoryStream(this byte[] bytes) => new(bytes);
}
using System;
using System.Reflection;

namespace Koubot.Tool.Extensions;

public static class AssemblyExtensions
{
    public static Version GetVersion(this Assembly assembly) => assembly.GetName().Version;
}
using System.Reflection;

namespace Gatherly.Application.Abstractions;

public static class AssemblyReference
{
    public static readonly Assembly assembly = typeof(AssemblyReference).Assembly;
}

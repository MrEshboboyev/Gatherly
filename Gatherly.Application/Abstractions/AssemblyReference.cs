﻿using System.Reflection;

namespace Gatherly.Application.Abstractions;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}

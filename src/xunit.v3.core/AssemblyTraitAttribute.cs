using System;

namespace Xunit;

/// <summary>
/// This class has been deprecated. Use <see cref="TraitAttribute"/> instead.
/// </summary>
[Obsolete("This class has been deprecated. Use Xunit.TraitAttribute instead", error: true)]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AssemblyTraitAttribute : Attribute { }

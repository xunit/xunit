using System;

namespace Xunit.Runner.Common;

/// <summary>
/// This class is used to decorate runner reporters which should not be considered during the normal
/// reporter enumeration and selection process. This includes <see cref="DefaultRunnerReporter"/>
/// (since it's the default fallback).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HiddenRunnerReporterAttribute : Attribute
{ }

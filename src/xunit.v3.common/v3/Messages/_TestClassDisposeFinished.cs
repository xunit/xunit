using System;

namespace Xunit.v3;

/// <summary>
/// This message indicates that the <see cref="IDisposable.Dispose"/> and/or
/// <see cref="IAsyncDisposable.DisposeAsync"/> method was just called on the test class
/// for the test that just finished executing.
/// </summary>
public class _TestClassDisposeFinished : _TestMessage
{ }


using System;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the <see cref="IDisposable.Dispose"/> or
/// <see cref="IAsyncDisposable.DisposeAsync"/> method was just called on the test class
/// for the test that just finished executing.
/// </summary>
public interface ITestClassDisposeFinished : ITestMessage
{ }

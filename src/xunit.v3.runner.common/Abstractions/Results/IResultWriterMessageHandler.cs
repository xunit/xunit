using System;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a message handler that will receive realtime messages during test execution
/// to collect for reporting. The call to <see cref="IAsyncDisposable.DisposeAsync"/> will
/// be the signal that the execution is complete, and any final file operations should be
/// performed before disposal is complete.
/// </summary>
public interface IResultWriterMessageHandler : IMessageSink, IAsyncDisposable
{ }

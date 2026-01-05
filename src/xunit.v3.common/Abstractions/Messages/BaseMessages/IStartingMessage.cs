using System;

namespace Xunit.Sdk;

/// <summary>
/// Indicates a message which includes start time information.
/// </summary>
public interface IStartingMessage
{
	/// <summary>
	/// Gets the date and time when the event began.
	/// </summary>
	DateTimeOffset StartTime { get; }
}

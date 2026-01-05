using System;

namespace Xunit.Sdk;

/// <summary>
/// Indicates a message which includes finish time information.
/// </summary>
public interface IFinishedMessage
{
	/// <summary>
	/// Gets the date and time when the event finished.
	/// </summary>
	DateTimeOffset FinishTime { get; }
}

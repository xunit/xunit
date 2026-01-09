using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Base interface type for result writer registration attributes.
/// </summary>
public interface IRegisterResultWriterAttribute
{
	/// <summary>
	/// Gets the ID of the result writer.
	/// </summary>
	string ID { get; }

	/// <summary>
	/// Gets the type of the result writer to register.
	/// </summary>
	public Type ResultWriterType { get; }
}

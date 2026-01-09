namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a catastrophic error has occurred.
/// </summary>
public interface IErrorMessage : IMessageSinkMessage, IErrorMetadata
{
	/// <summary>
	/// Gets the assembly's unique ID, if this error is associated with an assembly.
	/// </summary>
	/// <remarks>
	/// Note: This value may also be <see langword="null"/> even if it originated from an assembly,
	/// if the test project it originated from pre-dates when this property was added.
	/// </remarks>
	string? AssemblyUniqueID { get; }
}

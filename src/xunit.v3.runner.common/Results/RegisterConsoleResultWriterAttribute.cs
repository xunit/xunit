using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// result writer (in console mode).
/// </summary>
/// <param name="id">The ID of the result writer. Will add a command line switch for the writer
/// as <c>"-result-{ID}"</c>.</param>
/// <param name="resultWriterType">The type of the result writer to register. The type must
/// implement <see cref="IConsoleResultWriter"/>.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class RegisterConsoleResultWriterAttribute(string id, Type resultWriterType) :
	Attribute, IRegisterConsoleResultWriterAttribute
{
	/// <inheritdoc/>
	public string ID { get; } = id;

	/// <inheritdoc/>
	public Type ResultWriterType { get; } = resultWriterType;
}

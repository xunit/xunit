using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// result writer (in Microsoft Testing Platform mode).
/// </summary>
/// <param name="id">The ID of the result writer. Will add command line switches for the writer
/// as <c>"--xunit-result-{ID}"</c> and <c>"--xunit-result-{ID}-filename"</c>.</param>
/// <param name="resultWriterType">The type of the result writer to register. The type must
/// implement <see cref="IMicrosoftTestingPlatformResultWriter"/>.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class RegisterMicrosoftTestingPlatformResultWriterAttribute(string id, Type resultWriterType) :
	Attribute, IRegisterMicrosoftTestingPlatformResultWriterAttribute
{
	/// <inheritdoc/>
	public string ID { get; } = id;

	/// <inheritdoc/>
	public Type ResultWriterType { get; } = resultWriterType;
}

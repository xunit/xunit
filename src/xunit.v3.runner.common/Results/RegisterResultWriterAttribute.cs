using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// result writer (in both console mode and Microsoft Testing Platform mode).
/// </summary>
/// <param name="id">The ID of the result writer. Will add a console mode command line switch
/// for the writer as <c>"-result-{ID}"</c>, as well as Microsoft Testing Platform command line
/// switches as <c>"--xunit-result-{ID}"</c> and <c>"--xunit-result-{ID}-filename"</c>.</param>
/// <param name="resultWriterType">The type of the result writer to register. The type must
/// implement both <see cref="IConsoleResultWriter"/> and
/// <see cref="IMicrosoftTestingPlatformResultWriter"/>.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class RegisterResultWriterAttribute(string id, Type resultWriterType)
	: Attribute, IRegisterConsoleResultWriterAttribute, IRegisterMicrosoftTestingPlatformResultWriterAttribute
{
	/// <inheritdoc/>
	public string ID { get; } = id;

	/// <inheritdoc/>
	public Type ResultWriterType { get; } = resultWriterType;
}

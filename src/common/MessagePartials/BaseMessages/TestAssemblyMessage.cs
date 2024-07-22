using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestAssemblyMessage"/>.
/// </summary>
abstract partial class TestAssemblyMessage : MessageSinkMessage, ITestAssemblyMessage
{
	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		serializer.Serialize(nameof(AssemblyUniqueID), AssemblyUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, AssemblyUniqueID.Quoted());
}

#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using Xunit.Abstractions;
using Xunit.Internal;
using V2SourceInformationProvider = Xunit.Abstractions.ISourceInformationProvider;
using V3SourceInformationProvider = Xunit.Runner.Common.ISourceInformationProvider;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of xUnit.net v2's <see cref="Abstractions.ISourceInformationProvider"/> which
/// delegates calls to an xUnit.net v3 implementation of <see cref="Common.ISourceInformationProvider"/>.
/// </summary>
/// <param name="v3Provider">The xUnit.net v3 provider that is being wrapped</param>
public sealed class Xunit2SourceInformationProvider(V3SourceInformationProvider v3Provider) :
	MarshalByRefObject, V2SourceInformationProvider
{
#pragma warning disable CA2213 // This object's lifetime isn't owned by the wrapper
	readonly V3SourceInformationProvider v3Provider = Guard.ArgumentNotNull(v3Provider);
#pragma warning restore CA2213

	/// <inheritdoc/>
	public void Dispose()
	{ }

	/// <inheritdoc/>
	public ISourceInformation? GetSourceInformation(Abstractions.ITestCase testCase)
	{
		var className = testCase?.TestMethod?.TestClass?.Class?.Name;
		var methodName = testCase?.TestMethod?.Method?.Name;
		if (className is null || methodName is null)
			return null;

		var sourceInformation = v3Provider.GetSourceInformation(className, methodName);
		return new Xunit2SourceInformation { FileName = sourceInformation.SourceFile, LineNumber = sourceInformation.SourceLine };
	}

#if NETFRAMEWORK
	/// <inheritdoc/>
	[System.Security.SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;
#endif
}

#pragma warning disable xUnit3000 // This class does not have direct access to v2 xunit.runner.utility, so it can't derive from v2's LLMBRO

using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of xUnit.net v2's <see cref="Abstractions.ISourceInformationProvider"/> which
/// delegates calls to an xUnit.net v3 implementation of <see cref="Common.ISourceInformationProvider"/>.
/// </summary>
public class Xunit2SourceInformationProvider : MarshalByRefObject, Abstractions.ISourceInformationProvider
{
#pragma warning disable CA2213 // This is disposed on a background thread (because of the contract of ISourceInformationProvider)
	readonly DisposalTracker disposalTracker = new();
#pragma warning restore CA2213
#pragma warning disable CA2213 // This is disposed by DisposalTracker
	readonly Common.ISourceInformationProvider v3Provider;
#pragma warning restore CA2213

	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit2SourceInformationProvider"/> class.
	/// </summary>
	/// <param name="v3Provider">The xUnit.net v3 provider that is being wrapped</param>
	public Xunit2SourceInformationProvider(Common.ISourceInformationProvider v3Provider)
	{
		this.v3Provider = Guard.ArgumentNotNull(v3Provider);

		disposalTracker.Add(v3Provider);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);

		// Have to do disposal on a background thread, since we can't guarantee that disposal
		// will be synchronous (and we can't change the contract of ISourceInformationProvider).
		Task.Run(async () => await disposalTracker.SafeDisposeAsync());
	}

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

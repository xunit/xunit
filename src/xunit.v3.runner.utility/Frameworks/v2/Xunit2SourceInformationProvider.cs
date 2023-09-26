using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v2;

/// <summary>
/// An implementation of xUnit.net v2's <see cref="ISourceInformationProvider"/> which
/// delegates calls to an xUnit.net v3 implementation of <see cref="_ISourceInformationProvider"/>.
/// </summary>
public class Xunit2SourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
{
#pragma warning disable CA2213 // This is disposed on a background thread (because of the contract of ISourceInformationProvider)
	readonly DisposalTracker disposalTracker = new();
#pragma warning restore CA2213
#pragma warning disable CA2213 // This is disposed by DisposalTracker
	readonly _ISourceInformationProvider v3Provider;
#pragma warning restore CA2213

	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit2SourceInformationProvider"/> class.
	/// </summary>
	/// <param name="v3Provider">The xUnit.net v3 provider that is being wrapped</param>
	public Xunit2SourceInformationProvider(_ISourceInformationProvider v3Provider)
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
		Task.Run(async () => await disposalTracker.DisposeAsync());
	}

	/// <inheritdoc/>
	public ISourceInformation? GetSourceInformation(ITestCase testCase)
	{
		var className = testCase?.TestMethod?.TestClass?.Class?.Name;
		var methodName = testCase?.TestMethod?.Method?.Name;
		if (className is null || methodName is null)
			return null;

		var (sourceFile, sourceLine) = v3Provider.GetSourceInformation(className, methodName);
		return new SourceInformation { FileName = sourceFile, LineNumber = sourceLine };
	}
}

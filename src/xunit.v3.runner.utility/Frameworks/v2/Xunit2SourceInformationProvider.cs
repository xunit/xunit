using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// An implementation of xUnit.net v2's <see cref="ISourceInformationProvider"/> which
	/// delegates calls to an xUnit.net v3 implementation of <see cref="_ISourceInformationProvider"/>.
	/// </summary>
	public class Xunit2SourceInformationProvider : LongLivedMarshalByRefObject, ISourceInformationProvider
	{
		readonly DisposalTracker disposalTracker = new();
		readonly _ISourceInformationProvider v3Provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2SourceInformationProvider"/> class.
		/// </summary>
		/// <param name="v3Provider"></param>
		public Xunit2SourceInformationProvider(_ISourceInformationProvider v3Provider)
		{
			disposalTracker.Add(v3Provider);

			this.v3Provider = Guard.ArgumentNotNull(v3Provider);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			// Have to do disposal on a background thread, since we can't guarantee that disposal
			// will be synchronous (and we can't change the contract of ISourceInformationProvider).
			Task.Run(async () => await disposalTracker.DisposeAsync());
		}

		/// <inheritdoc/>
		public ISourceInformation? GetSourceInformation(ITestCase testCase)
		{
			var className = testCase?.TestMethod?.TestClass?.Class?.Name;
			var methodName = testCase?.TestMethod?.Method?.Name;
			if (className == null || methodName == null)
				return null;

			var (sourceFile, sourceLine) = v3Provider.GetSourceInformation(className, methodName);
			return new SourceInformation { FileName = sourceFile, LineNumber = sourceLine };
		}
	}
}

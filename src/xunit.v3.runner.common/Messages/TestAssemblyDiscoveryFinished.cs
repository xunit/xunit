using System;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Reports that runner has just finished discovery for a test assembly.
	/// </summary>
	public class TestAssemblyDiscoveryFinished : _MessageSinkMessage
	{
		XunitProjectAssembly? assembly;
		_ITestFrameworkDiscoveryOptions? discoveryOptions;

		/// <summary>
		/// Gets information about the assembly being discovered.
		/// </summary>
		public XunitProjectAssembly Assembly
		{
			get => assembly ?? throw new InvalidOperationException($"Attempted to get {nameof(Assembly)} on an uninitialized '{GetType().FullName}' object");
			set => assembly = Guard.ArgumentNotNull(nameof(Assembly), value);
		}

		/// <summary>
		/// Gets the options that were used during discovery.
		/// </summary>
		public _ITestFrameworkDiscoveryOptions DiscoveryOptions
		{
			get => discoveryOptions ?? throw new InvalidOperationException($"Attempted to get {nameof(DiscoveryOptions)} on an uninitialized '{GetType().FullName}' object");
			set => discoveryOptions = Guard.ArgumentNotNull(nameof(DiscoveryOptions), value);
		}

		/// <summary>
		/// Gets the count of the number of discovered test cases.
		/// </summary>
		public int TestCasesDiscovered { get; set; }

		/// <summary>
		/// Gets the count of the number of test cases that will be run (post-filtering).
		/// </summary>
		public int TestCasesToRun { get; set; }
	}
}

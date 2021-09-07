using System;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Reports that runner is about to start execution for a test assembly.
	/// </summary>
	public class TestAssemblyExecutionStarting : _MessageSinkMessage
	{
		XunitProjectAssembly? assembly;
		_ITestFrameworkExecutionOptions? executionOptions;

		/// <summary>
		/// Gets information about the assembly being executed.
		/// </summary>
		public XunitProjectAssembly Assembly
		{
			get => assembly ?? throw new InvalidOperationException($"Attempted to get {nameof(Assembly)} on an uninitialized '{GetType().FullName}' object");
			set => assembly = Guard.ArgumentNotNull(nameof(Assembly), value);
		}

		/// <summary>
		/// Gets the options that will be used during execution.
		/// </summary>
		public _ITestFrameworkExecutionOptions ExecutionOptions
		{
			get => executionOptions ?? throw new InvalidOperationException($"Attempted to get {nameof(ExecutionOptions)} on an uninitialized '{GetType().FullName}' object");
			set => executionOptions = Guard.ArgumentNotNull(nameof(ExecutionOptions), value);
		}
	}
}

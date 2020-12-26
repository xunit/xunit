using System;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Provides a class which wraps <see cref="ITestAssembly"/> instances to implement <see cref="_ITestAssembly"/>.
	/// </summary>
	public class Xunit2TestAssembly : _ITestAssembly
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2TestAssembly"/> class.
		/// </summary>
		/// <param name="v2TestAssembly">The v2 test assembly to wrap.</param>
		public Xunit2TestAssembly(ITestAssembly v2TestAssembly)
		{
			V2TestAssembly = Guard.ArgumentNotNull(nameof(v2TestAssembly), v2TestAssembly);
			UniqueID = UniqueIDGenerator.ForAssembly(v2TestAssembly.Assembly.Name, v2TestAssembly.Assembly.AssemblyPath, v2TestAssembly.ConfigFileName);
		}

		/// <inheritdoc/>
		public _IAssemblyInfo Assembly => throw new NotImplementedException();

		/// <inheritdoc/>
		public string? ConfigFileName => throw new NotImplementedException();

		/// <inheritdoc/>
		public string UniqueID { get; }

		/// <inheritdoc/>
		public Version Version => throw new NotImplementedException();

		/// <summary>
		/// Gets the underlying xUnit.net v2 <see cref="ITestAssembly"/> that this class is wrapping.
		/// </summary>
		public ITestAssembly V2TestAssembly { get; }
	}
}

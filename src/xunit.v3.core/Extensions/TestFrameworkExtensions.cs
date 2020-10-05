using System;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Abstractions
{
	/// <summary>
	/// Extension methods for <see cref="_ITestFramework"/>.
	/// </summary>
	public static class TestFrameworkExtensions
	{
		/// <summary>
		/// This API is obsolete. Please call <see cref="_ITestFramework.GetExecutor(IReflectionAssemblyInfo)"/>
		/// instead.
		/// </summary>
		[Obsolete("Please call the version of this API which accepts IReflectionAssemblyInfo")]
		public static ITestFrameworkExecutor GetExecutor(
			this _ITestFramework testFramework,
			AssemblyName assemblyName)
		{
			Guard.ArgumentNotNull(nameof(testFramework), testFramework);
			Guard.ArgumentNotNull(nameof(assemblyName), assemblyName);

			var assembly = Assembly.Load(assemblyName);

			return testFramework.GetExecutor(Reflector.Wrap(assembly));
		}
	}
}

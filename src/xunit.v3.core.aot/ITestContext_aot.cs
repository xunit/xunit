using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

partial interface ITestContext
{
	/// <summary>
	/// Gets the current test, if the engine is currently in the process of running a test;
	/// will return <see langword="null"/> outside of the context of a test.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTest"/>.
	/// </remarks>
	ITest? Test { get; }

	/// <summary>
	/// Gets the current test assembly, if the engine is currently in the process of running or
	/// discovering tests in assembly; will return <see langword="null"/> out of this context (this typically
	/// means the test framework itself is being created and initialized).
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTestAssembly"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestCollection))]
	ITestAssembly? TestAssembly { get; }

	/// <summary>
	/// Gets the current test case, if the engine is currently in the process of running a
	/// test case; will return <see langword="null"/> outside of the context of a test case.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTestCase"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(Test))]
	ITestCase? TestCase { get; }

	/// <summary>
	/// Gets the current test method, if the engine is currently in the process of running
	/// a test class; will return <see langword="null"/> outside of the context of a test class. Note that
	/// not all test framework implementations require that tests be based on classes, so this
	/// value may be <see langword="null"/> even if <see cref="TestCase"/> is not <see langword="null"/>.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTestClass"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestMethod))]
	ITestClass? TestClass { get; }

	/// <summary>
	/// Gets the current test collection, if the engine is currently in the process of running
	/// a test collection; will return <see langword="null"/> outside of the context of a test collection.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTestCollection"/>.
	/// </remarks>
	[NotNullIfNotNull(nameof(TestClass))]
	[NotNullIfNotNull(nameof(TestCase))]
	ITestCollection? TestCollection { get; }

	/// <summary>
	/// Gets the current test method, if the engine is currently in the process of running
	/// a test method; will return <see langword="null"/> outside of the context of a test method. Note that
	/// not all test framework implementations require that tests be based on methods, so this
	/// value may be <see langword="null"/> even if <see cref="TestCase"/> is not <see langword="null"/>.
	/// </summary>
	/// <remarks>
	/// When running with the default test framework implementation, the value here is likely
	/// to implement <see cref="ICodeGenTestMethod"/>.
	/// </remarks>
	ITestMethod? TestMethod { get; }
}

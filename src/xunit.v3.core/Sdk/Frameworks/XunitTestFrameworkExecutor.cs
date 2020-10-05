using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.v2;

namespace Xunit.Sdk
{
	/// <summary>
	/// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
	/// of unit tests linked against xunit.v3.core.dll.
	/// </summary>
	public class XunitTestFrameworkExecutor : TestFrameworkExecutor<IXunitTestCase>
	{
		readonly Lazy<XunitTestFrameworkDiscoverer> discoverer;
		TestAssembly testAssembly;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
		/// </summary>
		/// <param name="assemblyInfo">The test assembly.</param>
		/// <param name="configFileName">The test configuration file.</param>
		/// <param name="sourceInformationProvider">The source line number information provider.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/> messages.</param>
		public XunitTestFrameworkExecutor(
			IReflectionAssemblyInfo assemblyInfo,
			string? configFileName,
			ISourceInformationProvider sourceInformationProvider,
			IMessageSink diagnosticMessageSink)
				: base(assemblyInfo, sourceInformationProvider, diagnosticMessageSink)
		{
			testAssembly = new TestAssembly(AssemblyInfo, configFileName, assemblyInfo.Assembly.GetName().Version);
			discoverer = new Lazy<XunitTestFrameworkDiscoverer>(() => new XunitTestFrameworkDiscoverer(AssemblyInfo, configFileName, SourceInformationProvider, DiagnosticMessageSink));
		}

		/// <summary>
		/// Gets the test assembly that contains the test.
		/// </summary>
		protected TestAssembly TestAssembly
		{
			get => testAssembly;
			set => testAssembly = Guard.ArgumentNotNull(nameof(TestAssembly), value);
		}

		/// <inheritdoc/>
		protected override ITestFrameworkDiscoverer CreateDiscoverer() => discoverer.Value;

		/// <inheritdoc/>
		public override ITestCase Deserialize(string value)
		{
			if (value.Length > 3 && value.StartsWith(":F:"))
			{
				// Format from XunitTestFrameworkDiscoverer.Serialize: ":F:{typeName}:{methodName}:{defaultMethodDisplay}:{defaultMethodDisplayOptions}:{collectionId}"
				// Colons in values are double-escaped, so we can't use String.Split
				var parts = new List<string>();
				var idx = 3;
				var idxEnd = 3;
				while (idxEnd < value.Length)
				{
					if (value[idxEnd] == ':')
					{
						if (idxEnd + 1 == value.Length || value[idxEnd + 1] != ':')
						{
							if (idx != idxEnd)
								parts.Add(value.Substring(idx, idxEnd - idx).Replace("::", ":"));

							idx = idxEnd + 1;
						}
						else if (value[idxEnd + 1] == ':')
							++idxEnd;
					}

					++idxEnd;
				}

				if (idx != idxEnd)
					parts.Add(value.Substring(idx, idxEnd - idx).Replace("::", ":"));

				if (parts.Count > 4)
				{
					var typeInfo = discoverer.Value.AssemblyInfo.GetType(parts[0]);
					if (typeInfo == null)
						DiagnosticMessageSink.OnMessage(new DiagnosticMessage($"Could not find type {parts[0]} during test case deserialization"));
					else
					{
						var testCollectionUniqueId = Guid.Parse(parts[4]);
						var testClass = discoverer.Value.CreateTestClass(typeInfo, testCollectionUniqueId);
						var methodInfo = testClass.Class.GetMethod(parts[1], true);
						if (methodInfo != null)
						{
							var testMethod = new TestMethod(testClass, methodInfo);
							var defaultMethodDisplay = (TestMethodDisplay)int.Parse(parts[2]);
							var defaultMethodDisplayOptions = (TestMethodDisplayOptions)int.Parse(parts[3]);
							return new XunitTestCase(DiagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod);
						}
					}
				}
			}

			return base.Deserialize(value);
		}

		/// <inheritdoc/>
		protected override async void RunTestCases(
			IEnumerable<IXunitTestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions)
		{
			using var assemblyRunner = new XunitTestAssemblyRunner(TestAssembly, testCases, DiagnosticMessageSink, executionMessageSink, executionOptions);
			await assemblyRunner.RunAsync();
		}
	}
}

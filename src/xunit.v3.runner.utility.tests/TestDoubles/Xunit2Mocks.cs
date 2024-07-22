using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NSubstitute;
using Xunit.Abstractions;
using ExceptionUtility = Xunit.Sdk.ExceptionUtility;

namespace Xunit.Runner.v2
{
	public static class Xunit2Mocks
	{
		static readonly Guid OneGuid = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

		public static IAfterTestFinished AfterTestFinished(
			Abstractions.ITest test,
			string attributeName)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<IAfterTestFinished, InterfaceProxy<IAfterTestFinished>>();
			result.AttributeName.Returns(attributeName);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static IAfterTestStarting AfterTestStarting(
			Abstractions.ITest test,
			string attributeName)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<IAfterTestStarting, InterfaceProxy<IAfterTestStarting>>();
			result.AttributeName.Returns(attributeName);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static IAssemblyInfo AssemblyInfo(
			ITypeInfo[]? types = null,
			IReflectionAttributeInfo[]? attributes = null,
			string? assemblyFileName = null)
		{
			attributes ??= new IReflectionAttributeInfo[0];

			var result = Substitute.For<IAssemblyInfo, InterfaceProxy<IAssemblyInfo>>();
			result.Name.Returns(assemblyFileName is null ? "assembly:" + Guid.NewGuid().ToString("n") : Path.GetFileNameWithoutExtension(assemblyFileName));
			result.AssemblyPath.Returns(assemblyFileName);
			result.GetType("").ReturnsForAnyArgs(types?.FirstOrDefault());
			result.GetTypes(true).ReturnsForAnyArgs(types ?? new ITypeInfo[0]);
			result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
			return result;
		}

		public static IBeforeTestFinished BeforeTestFinished(
			Abstractions.ITest test,
			string attributeName)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<IBeforeTestFinished, InterfaceProxy<IBeforeTestFinished>>();
			result.AttributeName.Returns(attributeName);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static IBeforeTestStarting BeforeTestStarting(
			Abstractions.ITest test,
			string attributeName)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<IBeforeTestStarting, InterfaceProxy<IBeforeTestStarting>>();
			result.AttributeName.Returns(attributeName);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static IDiagnosticMessage DiagnosticMessage(string message)
		{
			var result = Substitute.For<IDiagnosticMessage, InterfaceProxy<IDiagnosticMessage>>();
			result.Message.Returns(message);
			return result;
		}

		public static IDiscoveryCompleteMessage DiscoveryCompleteMessage() =>
			Substitute.For<IDiscoveryCompleteMessage, InterfaceProxy<IDiscoveryCompleteMessage>>();

		static IEnumerable<IAttributeInfo> LookupAttribute(
			string fullyQualifiedTypeName,
			IReflectionAttributeInfo[]? attributes)
		{
			if (attributes is null)
				return Enumerable.Empty<IAttributeInfo>();

			var attributeType = Type.GetType(fullyQualifiedTypeName);
			if (attributeType is null)
				return Enumerable.Empty<IAttributeInfo>();

			return attributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
		}

		public static IErrorMessage ErrorMessage(Exception ex)
		{
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<IErrorMessage, InterfaceProxy<IErrorMessage>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			return result;
		}

		public static IReflectionAttributeInfo TargetFrameworkAttribute(string frameworkName)
		{
			var attribute = new TargetFrameworkAttribute(frameworkName);

			var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
			result.Attribute.Returns(attribute);
			result.GetConstructorArguments().Returns(new object[] { frameworkName });
			return result;
		}

		public static Abstractions.ITest Test(
			Abstractions.ITestCase testCase,
			string displayName)
		{
			var result = Substitute.For<Abstractions.ITest, InterfaceProxy<Abstractions.ITest>>();
			result.DisplayName.Returns(displayName);
			result.TestCase.Returns(testCase);
			return result;
		}

		public static Abstractions.ITestAssembly TestAssembly(
			string? assemblyFileName = null,
			string? configFileName = null,
			string targetFrameworkName = ".MockEnvironment,Version=v21.12",
			ITypeInfo[]? types = null,
			IReflectionAttributeInfo[]? attributes = null)
		{
			assemblyFileName ??= "testAssembly.dll";

			var targetFrameworkAttr = TargetFrameworkAttribute(targetFrameworkName);

			attributes ??= Array.Empty<IReflectionAttributeInfo>();
			attributes = attributes.Concat(new[] { targetFrameworkAttr }).ToArray();

			var assemblyInfo = AssemblyInfo(types, attributes, assemblyFileName);

			var result = Substitute.For<Abstractions.ITestAssembly, InterfaceProxy<Abstractions.ITestAssembly>>();
			result.Assembly.Returns(assemblyInfo);
			result.ConfigFileName.Returns(configFileName);
			return result;
		}

		public static ITestAssemblyCleanupFailure TestAssemblyCleanupFailure(
			Abstractions.ITestAssembly testAssembly,
			Exception ex)
		{
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);
			var result = Substitute.For<ITestAssemblyCleanupFailure, InterfaceProxy<ITestAssemblyCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.TestAssembly.Returns(testAssembly);
			return result;
		}

		public static ITestAssemblyFinished TestAssemblyFinished(
			Abstractions.ITestAssembly? testAssembly = null,
			int testsRun = 0,
			int testsFailed = 0,
			int testsSkipped = 0,
			decimal executionTime = 0m)
		{
			testAssembly ??= TestAssembly("testAssembly.dll");
			var result = Substitute.For<ITestAssemblyFinished, InterfaceProxy<ITestAssemblyFinished>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestsRun.Returns(testsRun);
			result.TestsFailed.Returns(testsFailed);
			result.TestsSkipped.Returns(testsSkipped);
			result.ExecutionTime.Returns(executionTime);
			return result;
		}

		public static ITestAssemblyStarting TestAssemblyStarting(
			Abstractions.ITestAssembly? testAssembly = null,
			DateTime? startTime = null,
			string testEnvironment = "",
			string testFrameworkDisplayName = "")
		{
			testAssembly ??= TestAssembly();

			var result = Substitute.For<ITestAssemblyStarting, InterfaceProxy<ITestAssemblyStarting>>();
			result.StartTime.Returns(startTime ?? new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			result.TestAssembly.Returns(testAssembly);
			result.TestEnvironment.Returns(testEnvironment);
			result.TestFrameworkDisplayName.Returns(testFrameworkDisplayName);
			return result;
		}

		public static Abstractions.ITestCase TestCase(
			Abstractions.ITestMethod testMethod,
			string displayName = "<unset>",
			string? skipReason = null,
			string? sourceFileName = null,
			int? sourceLineNumber = null,
			Dictionary<string, List<string>>? traits = null,
			string uniqueID = "test-case-uniqueid")
		{
			var sourceInformation =
				sourceFileName is not null
					? new Xunit2SourceInformation { FileName = sourceFileName, LineNumber = sourceLineNumber }
					: null;

			var result = Substitute.For<Abstractions.ITestCase, InterfaceProxy<Abstractions.ITestCase>>();
			result.DisplayName.Returns(displayName);
			result.SkipReason.Returns(skipReason);
			result.SourceInformation.Returns(sourceInformation);
			result.TestMethod.Returns(testMethod);
			result.Traits.Returns(traits ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase));
			result.UniqueID.Returns(uniqueID);
			return result;
		}

		public static ITestCaseCleanupFailure TestCaseCleanupFailure(
			Abstractions.ITestCase testCase,
			Exception ex)
		{
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<ITestCaseCleanupFailure, InterfaceProxy<ITestCaseCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestCaseDiscoveryMessage TestCaseDiscoveryMessage(Abstractions.ITestCase testCase)
		{
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestCaseDiscoveryMessage, InterfaceProxy<ITestCaseDiscoveryMessage>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestCaseFinished TestCaseFinished(
			Abstractions.ITestCase testCase,
			decimal executionTime,
			int testsFailed,
			int testsRun,
			int testsSkipped)
		{
			var result = Substitute.For<ITestCaseFinished, InterfaceProxy<ITestCaseFinished>>();
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			result.ExecutionTime.Returns(executionTime);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			result.TestsFailed.Returns(testsFailed);
			result.TestsRun.Returns(testsRun);
			result.TestsSkipped.Returns(testsSkipped);
			return result;
		}

		public static ITestCaseStarting TestCaseStarting(Abstractions.ITestCase testCase)
		{
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestCaseStarting, InterfaceProxy<ITestCaseStarting>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static Abstractions.ITestClass TestClass(
			Abstractions.ITestCollection? testCollection = null,
			ITypeInfo? classType = null)
		{
			testCollection ??= TestCollection();
			classType ??= TypeInfo();

			var result = Substitute.For<Abstractions.ITestClass, InterfaceProxy<Abstractions.ITestClass>>();
			result.Class.Returns(classType);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestClassCleanupFailure TestClassCleanupFailure(
			Abstractions.ITestClass testClass,
			Exception ex)
		{
			var testAssembly = testClass.TestCollection.TestAssembly;
			var testCollection = testClass.TestCollection;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<ITestClassCleanupFailure, InterfaceProxy<ITestClassCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestClassConstructionStarting TestClassConstructionStarting(Abstractions.ITest test)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestClassConstructionStarting, InterfaceProxy<ITestClassConstructionStarting>>();
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestClassConstructionFinished TestClassConstructionFinished(Abstractions.ITest test)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestClassConstructionFinished, InterfaceProxy<ITestClassConstructionFinished>>();
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestClassDisposeFinished TestClassDisposeFinished(Abstractions.ITest test)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestClassDisposeFinished, InterfaceProxy<ITestClassDisposeFinished>>();
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestClassDisposeStarting TestClassDisposeStarting(Abstractions.ITest test)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestClassDisposeStarting, InterfaceProxy<ITestClassDisposeStarting>>();
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestClassFinished TestClassFinished(
			Abstractions.ITestClass testClass,
			decimal executionTime,
			int testsFailed,
			int testsRun,
			int testsSkipped)
		{
			var result = Substitute.For<ITestClassFinished, InterfaceProxy<ITestClassFinished>>();
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			result.ExecutionTime.Returns(executionTime);
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestsFailed.Returns(testsFailed);
			result.TestsRun.Returns(testsRun);
			result.TestsSkipped.Returns(testsSkipped);
			return result;
		}

		public static ITestClassStarting TestClassStarting(Abstractions.ITestClass testClass)
		{
			var result = Substitute.For<ITestClassStarting, InterfaceProxy<ITestClassStarting>>();
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestCleanupFailure TestCleanupFailure(
			Abstractions.ITest test,
			Exception ex)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<ITestCleanupFailure, InterfaceProxy<ITestCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static Abstractions.ITestCollection TestCollection(
			Abstractions.ITestAssembly? assembly = null,
			ITypeInfo? definition = null,
			string? displayName = null)
		{
			assembly ??= TestAssembly();
			displayName ??= "Mock test collection";

			var result = Substitute.For<Abstractions.ITestCollection, InterfaceProxy<Abstractions.ITestCollection>>();
			result.CollectionDefinition.Returns(definition);
			result.DisplayName.Returns(displayName);
			result.TestAssembly.Returns(assembly);
			result.UniqueID.Returns(OneGuid);
			return result;
		}

		public static ITestCollectionCleanupFailure TestCollectionCleanupFailure(
			Abstractions.ITestCollection collection,
			Exception ex)
		{
			var testAssembly = collection.TestAssembly;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);
			var result = Substitute.For<ITestCollectionCleanupFailure, InterfaceProxy<ITestCollectionCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestCollection.Returns(collection);
			return result;
		}

		public static ITestCollectionFinished TestCollectionFinished(
			Abstractions.ITestCollection testCollection,
			int testsRun = 0,
			int testsFailed = 0,
			int testsSkipped = 0,
			decimal executionTime = 0m)
		{
			var testAssembly = testCollection.TestAssembly;
			var result = Substitute.For<ITestCollectionFinished, InterfaceProxy<ITestCollectionFinished>>();
			result.ExecutionTime.Returns(executionTime);
			result.TestAssembly.Returns(testAssembly);
			result.TestCollection.Returns(testCollection);
			result.TestsRun.Returns(testsRun);
			result.TestsFailed.Returns(testsFailed);
			result.TestsSkipped.Returns(testsSkipped);
			return result;
		}

		public static ITestCollectionStarting TestCollectionStarting(Abstractions.ITestCollection testCollection)
		{
			var testAssembly = testCollection.TestAssembly;
			var result = Substitute.For<ITestCollectionStarting, InterfaceProxy<ITestCollectionStarting>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestFailed TestFailed(
			Abstractions.ITest test,
			decimal executionTime,
			string output,
			Exception ex)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<ITestFailed, InterfaceProxy<ITestFailed>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.ExecutionTime.Returns(executionTime);
			result.Messages.Returns(messages);
			result.Output.Returns(output);
			result.StackTraces.Returns(stackTraces);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestFinished TestFinished(
			Abstractions.ITest test,
			decimal executionTime,
			string output)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestFinished, InterfaceProxy<ITestFinished>>();
			result.ExecutionTime.Returns(executionTime);
			result.Output.Returns(output);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static Abstractions.ITestMethod TestMethod(
			Abstractions.ITestClass testClass,
			string methodName)
		{
			var method = Substitute.For<IMethodInfo, InterfaceProxy<IMethodInfo>>();
			method.Name.Returns(methodName);

			var result = Substitute.For<Abstractions.ITestMethod, InterfaceProxy<Abstractions.ITestMethod>>();
			result.Method.Returns(method);
			result.TestClass.Returns(testClass);
			return result;
		}

		public static ITestMethodCleanupFailure TestMethodCleanupFailure(
			Abstractions.ITestMethod testMethod,
			Exception ex)
		{
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

			var result = Substitute.For<ITestMethodCleanupFailure, InterfaceProxy<ITestMethodCleanupFailure>>();
			result.ExceptionParentIndices.Returns(exceptionParentIndices);
			result.ExceptionTypes.Returns(exceptionTypes);
			result.Messages.Returns(messages);
			result.StackTraces.Returns(stackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestMethodFinished TestMethodFinished(
			Abstractions.ITestMethod testMethod,
			int testsRun,
			int testsFailed,
			int testsSkipped,
			decimal executionTime)
		{
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestMethodFinished, InterfaceProxy<ITestMethodFinished>>();
			result.TestAssembly.Returns(testAssembly);
			result.ExecutionTime.Returns(executionTime);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			result.TestsFailed.Returns(testsFailed);
			result.TestsRun.Returns(testsRun);
			result.TestsSkipped.Returns(testsSkipped);
			return result;
		}

		public static ITestMethodStarting TestMethodStarting(Abstractions.ITestMethod testMethod)
		{
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestMethodStarting, InterfaceProxy<ITestMethodStarting>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestOutput TestOutput(
			Abstractions.ITest test,
			string output)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestOutput, InterfaceProxy<ITestOutput>>();
			result.Output.Returns(output);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestPassed TestPassed(
			Abstractions.ITest test,
			decimal executionTime,
			string output)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestPassed, InterfaceProxy<ITestPassed>>();
			result.ExecutionTime.Returns(executionTime);
			result.Output.Returns(output);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestSkipped TestSkipped(
			Abstractions.ITest test,
			string reason)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestSkipped, InterfaceProxy<ITestSkipped>>();
			result.ExecutionTime.Returns(0);
			result.Output.Returns("");
			result.Reason.Returns(reason);
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestStarting TestStarting(Abstractions.ITest test)
		{
			var testCase = test.TestCase;
			var testMethod = testCase.TestMethod;
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;

			var result = Substitute.For<ITestStarting, InterfaceProxy<ITestStarting>>();
			result.Test.Returns(test);
			result.TestAssembly.Returns(testAssembly);
			result.TestCase.Returns(testCase);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITypeInfo TypeInfo(
			string? typeName = null,
			IMethodInfo[]? methods = null,
			IReflectionAttributeInfo[]? attributes = null,
			string? assemblyFileName = null)
		{
			var result = Substitute.For<ITypeInfo, InterfaceProxy<ITypeInfo>>();
			result.Name.Returns(typeName ?? "type:" + Guid.NewGuid().ToString("n"));
			result.GetMethods(false).ReturnsForAnyArgs(methods ?? new IMethodInfo[0]);
			var assemblyInfo = AssemblyInfo(assemblyFileName: assemblyFileName);
			result.Assembly.Returns(assemblyInfo);
			result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
			return result;
		}
	}
}

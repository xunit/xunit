using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	public static class Mocks
	{
		static readonly Guid OneGuid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);

		public static IAssemblyInfo AssemblyInfo(
			ITypeInfo[]? types = null,
			IReflectionAttributeInfo[]? attributes = null,
			string? assemblyFileName = null)
		{
			attributes ??= new IReflectionAttributeInfo[0];

			var result = Substitute.For<IAssemblyInfo, InterfaceProxy<IAssemblyInfo>>();
			result.Name.Returns(assemblyFileName == null ? "assembly:" + Guid.NewGuid().ToString("n") : Path.GetFileNameWithoutExtension(assemblyFileName));
			result.AssemblyPath.Returns(assemblyFileName);
			result.GetType("").ReturnsForAnyArgs(types?.FirstOrDefault());
			result.GetTypes(true).ReturnsForAnyArgs(types ?? new ITypeInfo[0]);
			result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
			return result;
		}

		static IEnumerable<IAttributeInfo> LookupAttribute(
			string fullyQualifiedTypeName,
			IReflectionAttributeInfo[]? attributes)
		{
			if (attributes == null)
				return Enumerable.Empty<IAttributeInfo>();

			var attributeType = Type.GetType(fullyQualifiedTypeName);
			if (attributeType == null)
				return Enumerable.Empty<IAttributeInfo>();

			return attributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
		}

		public static IReflectionAttributeInfo TargetFrameworkAttribute(string frameworkName)
		{
			var attribute = new TargetFrameworkAttribute(frameworkName);

			var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
			result.Attribute.Returns(attribute);
			result.GetConstructorArguments().Returns(new object[] { frameworkName });
			return result;
		}

		public static ITestAssembly TestAssembly(
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

			var result = Substitute.For<ITestAssembly, InterfaceProxy<ITestAssembly>>();
			result.Assembly.Returns(assemblyInfo);
			result.ConfigFileName.Returns(configFileName);
			return result;
		}

		public static ITestAssemblyCleanupFailure TestAssemblyCleanupFailure(
			ITestAssembly testAssembly,
			Exception ex)
		{
			var metadata = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);
			var result = Substitute.For<ITestAssemblyCleanupFailure, InterfaceProxy<ITestAssemblyCleanupFailure>>();
			result.ExceptionParentIndices.Returns(metadata.ExceptionParentIndices);
			result.ExceptionTypes.Returns(metadata.ExceptionTypes);
			result.Messages.Returns(metadata.Messages);
			result.StackTraces.Returns(metadata.StackTraces);
			result.TestAssembly.Returns(testAssembly);
			return result;
		}

		public static ITestAssemblyFinished TestAssemblyFinished(
			ITestAssembly? testAssembly = null,
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
			ITestAssembly? testAssembly = null,
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

		public static ITestCase TestCase(
			ITestMethod testMethod,
			string displayName = "<unset>",
			string? skipReason = null,
			string? sourceFileName = null,
			int? sourceLineNumber = null,
			Dictionary<string, List<string>>? traits = null,
			string uniqueID = "test-case-uniqueid")
		{
			var sourceInformation =
				sourceFileName != null
					? new SourceInformation { FileName = sourceFileName, LineNumber = sourceLineNumber }
					: null;

			var result = Substitute.For<ITestCase, InterfaceProxy<ITestCase>>();
			result.DisplayName.Returns(displayName);
			result.SkipReason.Returns(skipReason);
			result.SourceInformation.Returns(sourceInformation);
			result.TestMethod.Returns(testMethod);
			result.Traits.Returns(traits ?? new Dictionary<string, List<string>>());
			result.UniqueID.Returns(uniqueID);
			return result;
		}

		public static ITestCaseFinished TestCaseFinished(
			ITestCase testCase,
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

		public static ITestCaseStarting TestCaseStarting(ITestCase testCase)
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

		public static ITestClassCleanupFailure TestClassCleanupFailure(
			ITestClass testClass,
			Exception ex)
		{
			var testAssembly = testClass.TestCollection.TestAssembly;
			var testCollection = testClass.TestCollection;
			var errorMetadata = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);

			var result = Substitute.For<ITestClassCleanupFailure, InterfaceProxy<ITestClassCleanupFailure>>();
			result.ExceptionParentIndices.Returns(errorMetadata.ExceptionParentIndices);
			result.ExceptionTypes.Returns(errorMetadata.ExceptionTypes);
			result.Messages.Returns(errorMetadata.Messages);
			result.StackTraces.Returns(errorMetadata.StackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestClass TestClass(
			ITestCollection? testCollection = null,
			ITypeInfo? classType = null)
		{
			testCollection ??= TestCollection();
			classType ??= TypeInfo();

			var result = Substitute.For<ITestClass, InterfaceProxy<ITestClass>>();
			result.Class.Returns(classType);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestClassFinished TestClassFinished(
			ITestClass testClass,
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

		public static ITestClassStarting TestClassStarting(ITestClass testClass)
		{
			var result = Substitute.For<ITestClassStarting, InterfaceProxy<ITestClassStarting>>();
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestCollection TestCollection(
			ITestAssembly? assembly = null,
			ITypeInfo? definition = null,
			string? displayName = null)
		{
			assembly ??= TestAssembly();
			displayName ??= "Mock test collection";

			var result = Substitute.For<ITestCollection, InterfaceProxy<ITestCollection>>();
			result.CollectionDefinition.Returns(definition);
			result.DisplayName.Returns(displayName);
			result.TestAssembly.Returns(assembly);
			result.UniqueID.Returns(OneGuid);
			return result;
		}

		public static ITestCollectionCleanupFailure TestCollectionCleanupFailure(
			ITestCollection collection,
			Exception ex)
		{
			var testAssembly = collection.TestAssembly;
			var metadata = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);
			var result = Substitute.For<ITestCollectionCleanupFailure, InterfaceProxy<ITestCollectionCleanupFailure>>();
			result.ExceptionParentIndices.Returns(metadata.ExceptionParentIndices);
			result.ExceptionTypes.Returns(metadata.ExceptionTypes);
			result.Messages.Returns(metadata.Messages);
			result.StackTraces.Returns(metadata.StackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestCollection.Returns(collection);
			return result;
		}

		public static ITestCollectionFinished TestCollectionFinished(
			ITestCollection testCollection,
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

		public static ITestCollectionStarting TestCollectionStarting(ITestCollection testCollection)
		{
			var testAssembly = testCollection.TestAssembly;
			var result = Substitute.For<ITestCollectionStarting, InterfaceProxy<ITestCollectionStarting>>();
			result.TestAssembly.Returns(testAssembly);
			result.TestCollection.Returns(testCollection);
			return result;
		}

		public static ITestMethod TestMethod(
			ITestClass testClass,
			string methodName)
		{
			var method = Substitute.For<IMethodInfo, InterfaceProxy<IMethodInfo>>();
			method.Name.Returns(methodName);

			var result = Substitute.For<ITestMethod, InterfaceProxy<ITestMethod>>();
			result.Method.Returns(method);
			result.TestClass.Returns(testClass);
			return result;
		}

		public static ITestMethodCleanupFailure TestMethodCleanupFailure(
			ITestMethod testMethod,
			Exception ex)
		{
			var testClass = testMethod.TestClass;
			var testCollection = testClass.TestCollection;
			var testAssembly = testCollection.TestAssembly;
			var errorMetadata = ExceptionUtility.ConvertExceptionToErrorMetadata(ex);

			var result = Substitute.For<ITestMethodCleanupFailure, InterfaceProxy<ITestMethodCleanupFailure>>();
			result.ExceptionParentIndices.Returns(errorMetadata.ExceptionParentIndices);
			result.ExceptionTypes.Returns(errorMetadata.ExceptionTypes);
			result.Messages.Returns(errorMetadata.Messages);
			result.StackTraces.Returns(errorMetadata.StackTraces);
			result.TestAssembly.Returns(testAssembly);
			result.TestClass.Returns(testClass);
			result.TestCollection.Returns(testCollection);
			result.TestMethod.Returns(testMethod);
			return result;
		}

		public static ITestMethodFinished TestMethodFinished(
			ITestMethod testMethod,
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

		public static ITestMethodStarting TestMethodStarting(ITestMethod testMethod)
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

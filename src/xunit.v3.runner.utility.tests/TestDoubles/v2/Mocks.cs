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

		//public static IReflectionAttributeInfo CollectionAttribute(string collectionName)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new CollectionAttribute(collectionName));
		//	result.GetConstructorArguments().Returns(new[] { collectionName });
		//	return result;
		//}

		//public static IReflectionAttributeInfo CollectionBehaviorAttribute(
		//	CollectionBehavior? collectionBehavior = null,
		//	bool disableTestParallelization = false,
		//	int maxParallelThreads = 0)
		//{
		//	CollectionBehaviorAttribute attribute;
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();

		//	if (collectionBehavior.HasValue)
		//	{
		//		attribute = new CollectionBehaviorAttribute(collectionBehavior.GetValueOrDefault());
		//		result.GetConstructorArguments().Returns(new object[] { collectionBehavior });
		//	}
		//	else
		//	{
		//		attribute = new CollectionBehaviorAttribute();
		//		result.GetConstructorArguments().Returns(new object[0]);
		//	}

		//	attribute.DisableTestParallelization = disableTestParallelization;
		//	attribute.MaxParallelThreads = maxParallelThreads;

		//	result.Attribute.Returns(attribute);
		//	result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
		//	result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
		//	return result;
		//}

		//public static IReflectionAttributeInfo CollectionBehaviorAttribute(
		//	Type factoryType,
		//	bool disableTestParallelization = false,
		//	int maxParallelThreads = 0)
		//{
		//	var attribute = new CollectionBehaviorAttribute(factoryType)
		//	{
		//		DisableTestParallelization = disableTestParallelization,
		//		MaxParallelThreads = maxParallelThreads
		//	};
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(attribute);
		//	result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
		//	result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
		//	result.GetConstructorArguments().Returns(new object[] { factoryType });
		//	return result;
		//}

		//public static IReflectionAttributeInfo CollectionBehaviorAttribute(
		//	string factoryTypeName,
		//	string factoryAssemblyName,
		//	bool disableTestParallelization = false,
		//	int maxParallelThreads = 0)
		//{
		//	var attribute = new CollectionBehaviorAttribute(factoryTypeName, factoryAssemblyName)
		//	{
		//		DisableTestParallelization = disableTestParallelization,
		//		MaxParallelThreads = maxParallelThreads
		//	};
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(attribute);
		//	result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
		//	result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
		//	result.GetConstructorArguments().Returns(new object[] { factoryTypeName, factoryAssemblyName });
		//	return result;
		//}

		//public static IReflectionAttributeInfo CollectionDefinitionAttribute(string collectionName)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new CollectionDefinitionAttribute(collectionName));
		//	result.GetConstructorArguments().Returns(new[] { collectionName });
		//	return result;
		//}

		//public static IReflectionAttributeInfo DataAttribute(IEnumerable<object[]>? data = null)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	var dataAttribute = Substitute.For<DataAttribute>();
		//	dataAttribute.GetData(null!).ReturnsForAnyArgs(data);
		//	result.Attribute.Returns(dataAttribute);
		//	return result;
		//}

		//public static ExecutionErrorTestCase ExecutionErrorTestCase(
		//	string message,
		//	_IMessageSink? diagnosticMessageSink = null)
		//{
		//	var testMethod = TestMethod();
		//	return new ExecutionErrorTestCase(
		//		diagnosticMessageSink ?? new _NullMessageSink(),
		//		TestMethodDisplay.ClassAndMethod,
		//		TestMethodDisplayOptions.None,
		//		testMethod,
		//		message
		//	);
		//}

		//public static IReflectionAttributeInfo FactAttribute(
		//	string? displayName = null,
		//	string? skip = null,
		//	int timeout = 0)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
		//	result.GetNamedArgument<string>("DisplayName").Returns(displayName);
		//	result.GetNamedArgument<string>("Skip").Returns(skip);
		//	result.GetNamedArgument<int>("Timeout").Returns(timeout);
		//	return result;
		//}

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

		//public static IMethodInfo MethodInfo(
		//	string? methodName = null,
		//	IReflectionAttributeInfo[]? attributes = null,
		//	IParameterInfo[]? parameters = null,
		//	ITypeInfo? type = null,
		//	ITypeInfo? returnType = null,
		//	bool isAbstract = false,
		//	bool isPublic = true,
		//	bool isStatic = false)
		//{
		//	var result = Substitute.For<IMethodInfo, InterfaceProxy<IMethodInfo>>();

		//	attributes ??= new IReflectionAttributeInfo[0];
		//	parameters ??= new IParameterInfo[0];

		//	result.IsAbstract.Returns(isAbstract);
		//	result.IsPublic.Returns(isPublic);
		//	result.IsStatic.Returns(isStatic);
		//	result.Name.Returns(methodName ?? "method:" + Guid.NewGuid().ToString("n"));
		//	result.ReturnType.Returns(returnType);
		//	result.Type.Returns(type);
		//	result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
		//	result.GetParameters().Returns(parameters);

		//	return result;
		//}

		//public static IParameterInfo ParameterInfo(string name)
		//{
		//	var result = Substitute.For<IParameterInfo, InterfaceProxy<IParameterInfo>>();
		//	result.Name.Returns(name);
		//	return result;
		//}

		//public static IReflectionMethodInfo ReflectionMethodInfo<TClass>(string methodName)
		//{
		//	return Reflector.Wrap(typeof(TClass).GetMethod(methodName)!);
		//}

		//public static IReflectionTypeInfo ReflectionTypeInfo<TClass>()
		//{
		//	return Reflector.Wrap(typeof(TClass));
		//}

		public static IReflectionAttributeInfo TargetFrameworkAttribute(string frameworkName)
		{
			var attribute = new TargetFrameworkAttribute(frameworkName);

			var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
			result.Attribute.Returns(attribute);
			result.GetConstructorArguments().Returns(new object[] { frameworkName });
			return result;
		}

		//public static IRunnerReporter RunnerReporter(
		//	string? runnerSwitch = null,
		//	string? description = null,
		//	bool isEnvironmentallyEnabled = false,
		//	IMessageSinkWithTypes? messageSink = null)
		//{
		//	var result = Substitute.For<IRunnerReporter, InterfaceProxy<IRunnerReporter>>();
		//	result.Description.Returns(description ?? "The runner reporter description");
		//	result.IsEnvironmentallyEnabled.ReturnsForAnyArgs(isEnvironmentallyEnabled);
		//	result.RunnerSwitch.Returns(runnerSwitch);
		//	var dualSink = MessageSinkAdapter.Wrap(messageSink ?? Substitute.For<IMessageSinkWithTypes, InterfaceProxy<IMessageSinkWithTypes>>());
		//	result.CreateMessageHandler(null!).ReturnsForAnyArgs(dualSink);
		//	return result;
		//}

		//public static ITest Test(
		//	ITestCase testCase,
		//	string displayName)
		//{
		//	var result = Substitute.For<ITest, InterfaceProxy<ITest>>();
		//	result.DisplayName.Returns(displayName);
		//	result.TestCase.Returns(testCase);
		//	return result;
		//}

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

		//public static ITestAssemblyDiscoveryFinished TestAssemblyDiscoveryFinished(
		//	bool diagnosticMessages = false,
		//	int toRun = 42,
		//	int discovered = 2112)
		//{
		//	var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
		//	var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, ShadowCopy = true };
		//	var result = Substitute.For<ITestAssemblyDiscoveryFinished, InterfaceProxy<ITestAssemblyDiscoveryFinished>>();
		//	result.Assembly.Returns(assembly);
		//	result.DiscoveryOptions.Returns(_TestFrameworkOptions.ForDiscovery(config));
		//	result.TestCasesDiscovered.Returns(discovered);
		//	result.TestCasesToRun.Returns(toRun);
		//	return result;
		//}

		//public static ITestAssemblyDiscoveryStarting TestAssemblyDiscoveryStarting(
		//	bool diagnosticMessages = false,
		//	AppDomainOption appDomain = AppDomainOption.Disabled,
		//	bool shadowCopy = false)
		//{
		//	var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
		//	var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, MethodDisplay = TestMethodDisplay.ClassAndMethod, MaxParallelThreads = 42, ParallelizeTestCollections = true, ShadowCopy = shadowCopy };
		//	var result = Substitute.For<ITestAssemblyDiscoveryStarting, InterfaceProxy<ITestAssemblyDiscoveryStarting>>();
		//	result.AppDomain.Returns(appDomain);
		//	result.Assembly.Returns(assembly);
		//	result.DiscoveryOptions.Returns(_TestFrameworkOptions.ForDiscovery(config));
		//	result.ShadowCopy.Returns(shadowCopy);
		//	return result;
		//}

		//public static ITestAssemblyExecutionFinished TestAssemblyExecutionFinished(
		//	bool diagnosticMessages = false,
		//	int total = 2112,
		//	int failed = 42,
		//	int skipped = 8,
		//	int errors = 6,
		//	decimal time = 123.456M)
		//{
		//	var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
		//	var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, ShadowCopy = true };
		//	var summary = new ExecutionSummary { Total = total, Failed = failed, Skipped = skipped, Errors = errors, Time = time };
		//	var result = Substitute.For<ITestAssemblyExecutionFinished, InterfaceProxy<ITestAssemblyExecutionFinished>>();
		//	result.Assembly.Returns(assembly);
		//	result.ExecutionOptions.Returns(_TestFrameworkOptions.ForExecution(config));
		//	result.ExecutionSummary.Returns(summary);
		//	return result;
		//}

		//public static ITestAssemblyExecutionStarting TestAssemblyExecutionStarting(
		//	bool diagnosticMessages = false,
		//	string? assemblyFilename = null)
		//{
		//	var assembly = new XunitProjectAssembly { AssemblyFilename = assemblyFilename ?? "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
		//	var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, MethodDisplay = TestMethodDisplay.ClassAndMethod, MaxParallelThreads = 42, ParallelizeTestCollections = true, ShadowCopy = true };
		//	var result = Substitute.For<ITestAssemblyExecutionStarting, InterfaceProxy<ITestAssemblyExecutionStarting>>();
		//	result.Assembly.Returns(assembly);
		//	result.ExecutionOptions.Returns(_TestFrameworkOptions.ForExecution(config));
		//	return result;
		//}

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

		//public static IReflectionAttributeInfo TestCaseOrdererAttribute(string typeName, string assemblyName)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TestCaseOrdererAttribute(typeName, assemblyName));
		//	result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		//	return result;
		//}

		//public static IReflectionAttributeInfo TestCaseOrdererAttribute(Type type)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TestCaseOrdererAttribute(type));
		//	result.GetConstructorArguments().Returns(new object[] { type });
		//	return result;
		//}

		//public static IReflectionAttributeInfo TestCaseOrdererAttribute<TOrderer>() =>
		//	TestCaseOrdererAttribute(typeof(TOrderer));

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

		//public static TestClass TestClass(
		//	Type type,
		//	ITestCollection? collection = null)
		//{
		//	if (collection == null)
		//	{
		//		var assembly = TestAssembly();
		//		collection = TestCollection(assembly);
		//	}

		//	return new TestClass(collection, Reflector.Wrap(type));
		//}

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

		//public static IReflectionAttributeInfo TestCollectionOrdererAttribute(string typeName, string assemblyName)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TestCollectionOrdererAttribute(typeName, assemblyName));
		//	result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		//	return result;
		//}

		//public static IReflectionAttributeInfo TestCollectionOrdererAttribute(Type type)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TestCollectionOrdererAttribute(type));
		//	result.GetConstructorArguments().Returns(new object[] { type });
		//	return result;
		//}

		//public static IReflectionAttributeInfo TestCollectionOrdererAttribute<TOrderer>() =>
		//	TestCollectionOrdererAttribute(typeof(TOrderer));

		//public static ITestFailed TestFailed(
		//	Type type,
		//	string methodName,
		//	string? displayName = null,
		//	string? output = null,
		//	decimal executionTime = 0M,
		//	Exception? ex = null)
		//{
		//	var testCase = TestCase(type, methodName);
		//	var test = Test(testCase, displayName ?? "NO DISPLAY NAME");
		//	var failureInfo = ExceptionUtility.ConvertExceptionToFailureInformation(ex ?? new Exception());

		//	var result = Substitute.For<ITestFailed, InterfaceProxy<ITestFailed>>();
		//	result.ExceptionParentIndices.Returns(failureInfo.ExceptionParentIndices);
		//	result.ExceptionTypes.Returns(failureInfo.ExceptionTypes);
		//	result.ExecutionTime.Returns(executionTime);
		//	result.Messages.Returns(failureInfo.Messages);
		//	result.Output.Returns(output);
		//	result.StackTraces.Returns(failureInfo.StackTraces);
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static ITestFailed TestFailed(
		//	string displayName,
		//	decimal executionTime,
		//	string? exceptionType = null,
		//	string? exceptionMessage = null,
		//	string? stackTrace = null,
		//	string? output = null)
		//{
		//	var testCase = TestCase();
		//	var test = Test(testCase, displayName);
		//	var result = Substitute.For<ITestFailed, InterfaceProxy<ITestFailed>>();
		//	result.ExceptionParentIndices.Returns(new[] { -1 });
		//	result.ExceptionTypes.Returns(new[] { exceptionType });
		//	result.ExecutionTime.Returns(executionTime);
		//	result.Messages.Returns(new[] { exceptionMessage });
		//	result.Output.Returns(output);
		//	result.StackTraces.Returns(new[] { stackTrace });
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static IReflectionAttributeInfo TestFrameworkAttribute(Type type)
		//{
		//	var attribute = Activator.CreateInstance(type);
		//	if (attribute == null)
		//		throw new InvalidOperationException($"Unable to create attribute instance: '{type.FullName}'");

		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(attribute);
		//	result.GetCustomAttributes(null).ReturnsForAnyArgs(
		//		callInfo => LookupAttribute(
		//			callInfo.Arg<string>(),
		//			CustomAttributeData.GetCustomAttributes(attribute.GetType()).Select(x => Reflector.Wrap(x)).ToArray()
		//		)
		//	);
		//	return result;
		//}

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

		//public static ITestPassed TestPassed(
		//	Type type,
		//	string methodName,
		//	string? displayName = null,
		//	string? output = null,
		//	decimal executionTime = 0M)
		//{
		//	var testCase = TestCase(type, methodName);
		//	var test = Test(testCase, displayName ?? "NO DISPLAY NAME");

		//	var result = Substitute.For<ITestPassed, InterfaceProxy<ITestPassed>>();
		//	result.ExecutionTime.Returns(executionTime);
		//	result.Output.Returns(output);
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static ITestPassed TestPassed(
		//	string displayName,
		//	string? output = null)
		//{
		//	var testCase = TestCase();
		//	var test = Test(testCase, displayName);
		//	var result = Substitute.For<ITestPassed, InterfaceProxy<ITestPassed>>();
		//	result.Test.Returns(test);
		//	result.ExecutionTime.Returns(1.2345M);
		//	result.Output.Returns(output);
		//	return result;
		//}

		//public static ITestResultMessage TestResult<TClassUnderTest>(
		//	string methodName,
		//	string displayName,
		//	decimal executionTime)
		//{
		//	var testCase = TestCase<TClassUnderTest>(methodName);
		//	var test = Test(testCase, displayName);
		//	var result = Substitute.For<ITestResultMessage, InterfaceProxy<ITestResultMessage>>();
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	result.ExecutionTime.Returns(executionTime);
		//	return result;
		//}

		//public static ITestSkipped TestSkipped(
		//	Type type,
		//	string methodName,
		//	string? displayName = null,
		//	string? output = null,
		//	decimal executionTime = 0M,
		//	string? skipReason = null)
		//{
		//	var testCase = TestCase(type, methodName);
		//	var test = Test(testCase, displayName ?? "NO DISPLAY NAME");

		//	var result = Substitute.For<ITestSkipped, InterfaceProxy<ITestSkipped>>();
		//	result.ExecutionTime.Returns(executionTime);
		//	result.Output.Returns(output);
		//	result.Reason.Returns(skipReason);
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static ITestSkipped TestSkipped(
		//	string displayName,
		//	string? skipReason = null)
		//{
		//	var testCase = TestCase();
		//	var test = Test(testCase, displayName);

		//	var result = Substitute.For<ITestSkipped, InterfaceProxy<ITestSkipped>>();
		//	result.Reason.Returns(skipReason);
		//	result.TestCase.Returns(testCase);
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static ITestStarting TestStarting(string displayName)
		//{
		//	var testCase = TestCase();
		//	var test = Test(testCase, displayName);
		//	var result = Substitute.For<ITestStarting, InterfaceProxy<ITestStarting>>();
		//	result.Test.Returns(test);
		//	return result;
		//}

		//public static IReflectionAttributeInfo TheoryAttribute(
		//	string? displayName = null,
		//	string? skip = null,
		//	int timeout = 0)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TheoryAttribute { DisplayName = displayName, Skip = skip });
		//	result.GetNamedArgument<string>("DisplayName").Returns(displayName);
		//	result.GetNamedArgument<string>("Skip").Returns(skip);
		//	result.GetNamedArgument<int>("Timeout").Returns(timeout);
		//	return result;
		//}

		//public static IReflectionAttributeInfo TraitAttribute<T>()
		//	where T : Attribute, new()
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new T());
		//	return result;
		//}

		//public static IReflectionAttributeInfo TraitAttribute(
		//	string name,
		//	string value)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	var traitDiscovererAttributes = new[] { TraitDiscovererAttribute() };
		//	result.GetCustomAttributes(typeof(TraitDiscovererAttribute)).Returns(traitDiscovererAttributes);
		//	result.Attribute.Returns(new TraitAttribute(name, value));
		//	result.GetConstructorArguments().Returns(new object[] { name, value });
		//	return result;
		//}

		//public static IAttributeInfo TraitDiscovererAttribute(
		//	string typeName = "Xunit.Sdk.TraitDiscoverer",
		//	string assemblyName = "xunit.v3.core")
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TraitDiscovererAttribute(typeName, assemblyName));
		//	result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		//	return result;
		//}

		//public static IAttributeInfo TraitDiscovererAttribute(Type discovererType)
		//{
		//	var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
		//	result.Attribute.Returns(new TraitDiscovererAttribute(discovererType));
		//	result.GetConstructorArguments().Returns(new object[] { discovererType });
		//	return result;
		//}

		//public static IAttributeInfo TraitDiscovererAttribute<TDiscoverer>() =>
		//	TraitDiscovererAttribute(typeof(TDiscoverer));

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

		//public static XunitTestCase XunitTestCase<TClassUnderTest>(
		//	string methodName,
		//	ITestCollection? collection = null,
		//	object[]? testMethodArguments = null,
		//	_IMessageSink? diagnosticMessageSink = null)
		//{
		//	var method = TestMethod(typeof(TClassUnderTest), methodName, collection);

		//	return new XunitTestCase(diagnosticMessageSink ?? new _NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, method, testMethodArguments);
		//}

		//public static XunitTheoryTestCase XunitTheoryTestCase<TClassUnderTest>(
		//	string methodName,
		//	ITestCollection? collection = null,
		//	_IMessageSink? diagnosticMessageSink = null)
		//{
		//	var method = TestMethod(typeof(TClassUnderTest), methodName, collection);

		//	return new XunitTheoryTestCase(diagnosticMessageSink ?? new _NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, method);
		//}

		// Helpers

		//static Dictionary<string, List<string>> GetTraits(IMethodInfo method)
		//{
		//	var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		//	foreach (var traitAttribute in method.GetCustomAttributes(typeof(TraitAttribute)))
		//	{
		//		var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
		//		result.Add((string)ctorArgs[0], (string)ctorArgs[1]);
		//	}

		//	return result;
		//}
	}
}

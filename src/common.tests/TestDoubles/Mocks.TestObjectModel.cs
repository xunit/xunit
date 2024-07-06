using System;
using System.Collections.Generic;
using System.Reflection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of the test object model interfaces. The generic version based on a
// real test class will use live objects from TestData for the parents.
public static partial class Mocks
{
	public static IAssemblyMetadata AssemblyMetadata(
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		string? configFilePath = null)
	{
		var result = Substitute.For<IAssemblyMetadata, InterfaceProxy<IAssemblyMetadata>>();
		result.AssemblyName.Returns(assemblyName);
		result.AssemblyPath.Returns(assemblyPath);
		result.ConfigFilePath.Returns(configFilePath);
		return result;
	}

	// ITestXxx

	public static ITest Test(
		ITestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID)
	{
		testCase ??= TestCase();

		var result = Substitute.For<ITest, InterfaceProxy<ITest>>();
		result.TestCase.Returns(testCase);
		result.TestDisplayName.Returns(testDisplayName);
		result.Traits.Returns(traits ?? TestData.EmptyTraits);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static ITestAssembly TestAssembly(
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		string? configFilePath = null,
		Guid? moduleVersionID = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID)
	{
		var result = Substitute.For<ITestAssembly, InterfaceProxy<ITestAssembly>>();
		result.AssemblyName.Returns(assemblyName);
		result.AssemblyPath.Returns(assemblyPath);
		result.ConfigFilePath.Returns(configFilePath);
		result.ModuleVersionID.Returns(moduleVersionID ?? TestData.DefaultModuleVersionID);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static ITestCase TestCase(
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		ITestMethod? testMethod = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= TestMethod();

		var testCollection = testMethod.TestClass.TestCollection;
		var testClass = testMethod.TestClass;
		var testClassName = testClass.TestClassName;
		var testClassNamespace = testClass.TestClassNamespace;

		var result = Substitute.For<ITestCase, InterfaceProxy<ITestCase>>();
		result.SkipReason.Returns(skipReason);
		result.SourceFilePath.Returns(sourceFilePath);
		result.SourceLineNumber.Returns(sourceLineNumber);
		result.TestCaseDisplayName.Returns(testCaseDisplayName);
		result.TestClass.Returns(testClass);
		result.TestClassName.Returns(testClassName);
		result.TestClassNamespace.Returns(testClassNamespace);
		result.TestCollection.Returns(testCollection);
		result.Traits.Returns(traits ?? TestData.DefaultTraits);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static ITestClass TestClass(
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		ITestCollection? testCollection = null,
		string uniqueID = TestData.DefaultTestClassUniqueID)
	{
		testCollection ??= TestCollection();

		var result = Substitute.For<ITestClass, InterfaceProxy<ITestClass>>();
		result.TestClassName.Returns(testClassName);
		result.TestClassNamespace.Returns(testClassNamespace);
		result.TestCollection.Returns(testCollection);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static ITestCollection TestCollection(
		ITestAssembly? testAssembly = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		string uniqueID = TestData.DefaultTestCollectionUniqueID)
	{
		testAssembly ??= TestAssembly();

		var result = Substitute.For<ITestCollection, InterfaceProxy<ITestCollection>>();
		result.TestAssembly.Returns(testAssembly);
		result.TestCollectionClassName.Returns(testCollectionClassName);
		result.TestCollectionDisplayName.Returns(testCollectionDisplayName);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static ITestMethod TestMethod(
		string methodName = TestData.DefaultTestMethodName,
		ITestClass? testClass = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID)
	{
		testClass ??= TestClass();

		var result = Substitute.For<ITestMethod, InterfaceProxy<ITestMethod>>();
		result.MethodName.Returns(methodName);
		result.TestClass.Returns(testClass);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	// IXunitTestXxx

	public static IXunitTest XunitTest(
		bool @explicit = false,
		IXunitTestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID)
	{
		testCase ??= XunitTestCase();
		traits ??= testCase.Traits;

		var testMethod = testCase.TestMethod;

		var result = Substitute.For<IXunitTest, InterfaceProxy<IXunitTest>>();
		result.Explicit.Returns(@explicit);
		result.TestCase.Returns(testCase);
		result.TestDisplayName.Returns(testDisplayName);
		result.TestMethod.Returns(testMethod);
		result.Timeout.Returns(timeout);
		result.Traits.Returns(traits);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static IXunitTest XunitTest<TClassUnderTest>(
		string methodName,
		bool @explicit = false,
		string testDisplayName = TestData.DefaultTestDisplayName,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID) =>
			XunitTest(@explicit, TestData.XunitTestCase<TClassUnderTest>(methodName), testDisplayName, timeout, traits, uniqueID);

	public static IXunitTestAssembly XunitTestAssembly(
		IReadOnlyCollection<Type>? assemblyFixtureTypes = null,
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		ICollectionBehaviorAttribute? collectionBehavior = null,
		IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>? collectionDefinitions = null,
		string? configFilePath = null,
		Guid? moduleVersionID = null,
		string targetFramework = TestData.DefaultTargetFramework,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestCollectionOrderer? testCollectionOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID,
		Version? version = null)
	{
		version ??= new Version(2112, 42, 2600);

		var result = Substitute.For<IXunitTestAssembly, InterfaceProxy<IXunitTestAssembly>>();
		result.Assembly.Throws(new InvalidOperationException("Using IXunitTestAssembly.Assembly while testing is prohibited"));
		result.AssemblyFixtureTypes.Returns(assemblyFixtureTypes ?? []);
		result.AssemblyName.Returns(assemblyName);
		result.AssemblyPath.Returns(assemblyPath);
		result.BeforeAfterTestAttributes.Returns(beforeAfterTestAttributes ?? []);
		result.CollectionBehavior.Returns(collectionBehavior);
		result.CollectionDefinitions.Returns(collectionDefinitions ?? TestData.EmptyCollectionDefinitions);
		result.ConfigFilePath.Returns(configFilePath);
		result.ModuleVersionID.Returns(moduleVersionID ?? TestData.DefaultModuleVersionID);
		result.TargetFramework.Returns(targetFramework);
		result.TestCaseOrderer.Returns(testCaseOrderer);
		result.TestCollectionOrderer.Returns(testCollectionOrderer);
		result.Traits.Returns(traits ?? TestData.EmptyTraits);
		result.UniqueID.Returns(uniqueID);
		result.Version.Returns(version);
		return result;
	}

	public static IXunitTestCase XunitTestCase(
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		IXunitTestMethod? testMethod = null,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= XunitTestMethod();
		traits ??= testMethod.Traits;

		var testCollection = testMethod.TestClass.TestCollection;
		var testClass = testMethod.TestClass;
		var testClassName = testClass.TestClassName;
		var testClassNamespace = testClass.TestClassNamespace;

		var result = Substitute.For<IXunitTestCase, InterfaceProxy<IXunitTestCase>>();
		result.Explicit.Returns(@explicit);
		result.SkipReason.Returns(skipReason);
		result.SourceFilePath.Returns(sourceFilePath);
		result.SourceLineNumber.Returns(sourceLineNumber);
		result.TestCaseDisplayName.Returns(testCaseDisplayName);
		result.TestClass.Returns(testClass);
		result.TestClassName.Returns(testClassName);
		result.TestClassNamespace.Returns(testClassNamespace);
		result.TestCollection.Returns(testCollection);
		result.Timeout.Returns(timeout);
		result.Traits.Returns(traits);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static IXunitTestCase XunitTestCase<TClassUnderTest>(
		string methodName,
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID) =>
			XunitTestCase(@explicit, skipReason, sourceFilePath, sourceLineNumber, testCaseDisplayName, TestData.XunitTestMethod<TClassUnderTest>(methodName), timeout, traits, uniqueID);

	public static IXunitTestClass XunitTestClass(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<ConstructorInfo>? constructors = null,
		IReadOnlyCollection<MethodInfo>? methods = null,
		ITestCaseOrderer? testCaseOrderer = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		IXunitTestCollection? testCollection = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID)
	{
		testCollection ??= XunitTestCollection();
		traits ??= testCollection.TestAssembly.Traits;

		var result = Substitute.For<IXunitTestClass, InterfaceProxy<IXunitTestClass>>();
		result.BeforeAfterTestAttributes.Returns(beforeAfterTestAttributes ?? []);
		result.Class.Throws(new InvalidOperationException("Using IXunitTestClass.Class while testing is prohibited"));
		result.ClassFixtureTypes.Returns(classFixtureTypes ?? []);
		result.Constructors.Returns(constructors);
		result.Methods.Returns(methods ?? []);
		result.TestCaseOrderer.Returns(testCaseOrderer);
		result.TestClassName.Returns(testClassName);
		result.TestClassNamespace.Returns(testClassNamespace);
		result.TestCollection.Returns(testCollection);
		result.Traits.Returns(traits);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static IXunitTestClass XunitTestClass<TClassUnderTest>(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<ConstructorInfo>? constructors = null,
		IReadOnlyCollection<MethodInfo>? methods = null,
		ITestCaseOrderer? testCaseOrderer = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID) =>
			XunitTestClass(beforeAfterTestAttributes, classFixtureTypes, constructors, methods, testCaseOrderer, testClassName, testClassNamespace, TestData.XunitTestCollection(TestData.XunitTestAssembly(typeof(TClassUnderTest).Assembly)), traits, uniqueID);

	public static IXunitTestCollection XunitTestCollection(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<Type>? collectionFixtureTypes = null,
		bool disableParallelization = false,
		IXunitTestAssembly? testAssembly = null,
		ITestCaseOrderer? testCaseOrderer = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		string uniqueID = TestData.DefaultTestCollectionUniqueID)
	{
		testAssembly ??= XunitTestAssembly();

		var result = Substitute.For<IXunitTestCollection, InterfaceProxy<IXunitTestCollection>>();
		result.BeforeAfterTestAttributes.Returns(beforeAfterTestAttributes ?? []);
		result.ClassFixtureTypes.Returns(classFixtureTypes ?? []);
		result.CollectionDefinition.Throws(new InvalidOperationException("Using IXunitTestCollection.CollectionDefinition while testing is prohibited"));
		result.CollectionFixtureTypes.Returns(collectionFixtureTypes ?? []);
		result.DisableParallelization.Returns(disableParallelization);
		result.TestAssembly.Returns(testAssembly);
		result.TestCaseOrderer.Returns(testCaseOrderer);
		result.TestCollectionClassName.Returns(testCollectionClassName);
		result.TestCollectionDisplayName.Returns(testCollectionDisplayName);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static IXunitTestCollection XunitTestCollection<TClassUnderTest>(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<Type>? collectionFixtureTypes = null,
		bool disableParallelization = false,
		ITestCaseOrderer? testCaseOrderer = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		string uniqueID = TestData.DefaultTestCollectionUniqueID) =>
			XunitTestCollection(beforeAfterTestAttributes, classFixtureTypes, collectionFixtureTypes, disableParallelization, TestData.XunitTestAssembly(typeof(TClassUnderTest).Assembly), testCaseOrderer, testCollectionClassName, testCollectionDisplayName, uniqueID);

	public static IXunitTestMethod XunitTestMethod(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<IDataAttribute>? dataAttributes = null,
		string? displayName = null,
		IReadOnlyCollection<IFactAttribute>? factAttributes = null,
		bool isGenericMethodDefinition = false,
		string methodName = TestData.DefaultTestMethodName,
		IReadOnlyCollection<ParameterInfo>? parameters = null,
		IXunitTestClass? testClass = null,
		object?[]? testMethodArguments = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID)
	{
		factAttributes ??= [FactAttribute()];
		testClass ??= XunitTestClass();
		traits ??= testClass.Traits;

		var result = Substitute.For<IXunitTestMethod, InterfaceProxy<IXunitTestMethod>>();
		result.BeforeAfterTestAttributes.Returns(beforeAfterTestAttributes ?? []);
		result.DataAttributes.Returns(dataAttributes ?? []);
		result.FactAttributes.Returns(factAttributes);
		result.IsGenericMethodDefinition.Returns(isGenericMethodDefinition);
		result.Method.Throws(new InvalidOperationException("Using IXunitTestMethod.Method while testing is prohibited"));
		result.MethodName.Returns(methodName);
		result.Parameters.Returns(parameters ?? []);
		result.TestMethodArguments.Returns(testMethodArguments ?? []);
		result.TestClass.Returns(testClass);
		result.Traits.Returns(traits);
		result.UniqueID.Returns(uniqueID);
		result.GetDisplayName("", null, null).ReturnsForAnyArgs(args => displayName ?? args.Arg<string>());
		// No simple way to implement these, so throw for now until we find a test that needs them
		result.MakeGenericMethod([]).ThrowsForAnyArgs(new InvalidOperationException("Using IXunitTestMethod.MakeGenericMethod while testing is prohibited"));
		result.ResolveGenericTypes([]).ThrowsForAnyArgs(new InvalidOperationException("Using IXunitTestMethod.ResolveGenericTypes while testing is prohibited"));
		result.ResolveMethodArguments([]).ThrowsForAnyArgs(new InvalidOperationException("Using IXunitTestMethod.ResolveMethodArguments while testing is prohibited"));
		return result;
	}

	public static IXunitTestMethod XunitTestMethod<TClassUnderTest>(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<IDataAttribute>? dataAttributes = null,
		string? displayName = null,
		IReadOnlyCollection<IFactAttribute>? factAttributes = null,
		bool isGenericMethodDefinition = false,
		string methodName = TestData.DefaultTestMethodName,
		IReadOnlyCollection<ParameterInfo>? parameters = null,
		object?[]? testMethodArguments = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID) =>
			XunitTestMethod(beforeAfterTestAttributes, dataAttributes, displayName, factAttributes, isGenericMethodDefinition, methodName, parameters, TestData.XunitTestClass<TClassUnderTest>(), testMethodArguments, traits, uniqueID);
}

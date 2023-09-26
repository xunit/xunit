using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

// This file contains mocks of test case object hierarchy interfaces.
public static partial class Mocks
{
	public static _ITest Test(
		_ITestCase? testCase = null,
		string? displayName = null,
		string? uniqueID = null)
	{
		testCase ??= TestCase();
		displayName ??= "test-display-name";
		uniqueID ??= "test-id";

		var result = Substitute.For<_ITest, InterfaceProxy<_ITest>>();
		result.TestDisplayName.Returns(displayName);
		result.TestCase.Returns(testCase);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static _ITestAssembly TestAssembly(
		string? assemblyFileName,
		_ITypeInfo[]? types = null,
		_IReflectionAttributeInfo[]? assemblyAttributes = null,
		string? configFileName = null,
		Version? version = null,
		string? uniqueID = null) =>
			TestAssembly(AssemblyInfo(types, assemblyAttributes, assemblyFileName), configFileName, version, uniqueID);

	public static _ITestAssembly TestAssembly(
		Assembly assembly,
		string? configFileName = null,
		Version? version = null,
		string? uniqueID = null) =>
			TestAssembly(Reflector.Wrap(assembly), configFileName, version, uniqueID);

	public static _ITestAssembly TestAssembly(
		_IAssemblyInfo? assemblyInfo = null,
		string? configFileName = null,
		Version? version = null,
		string? uniqueID = null)
	{
		assemblyInfo ??= AssemblyInfo();
		uniqueID ??= "assembly-id";
		version ??= new Version(2112, 42, 2600);

		var result = Substitute.For<_ITestAssembly, InterfaceProxy<_ITestAssembly>>();
		result.Assembly.Returns(assemblyInfo);
		result.ConfigFileName.Returns(configFileName);
		result.UniqueID.Returns(uniqueID);
		result.Version.Returns(version);
		return result;
	}

	public static _ITestCase TestCase<TClassUnderTest>(
		string methodName,
		string? displayName = null,
		string? skipReason = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string? fileName = null,
		int? lineNumber = null,
		string? uniqueID = null) =>
			TestCase(TestMethod<TClassUnderTest>(methodName), displayName, skipReason, traits, fileName, lineNumber, uniqueID);

	public static _ITestCase TestCase(
		string typeName,
		string methodName,
		string? displayName = null,
		string? skipReason = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string? fileName = null,
		int? lineNumber = null,
		string? uniqueID = null) =>
			TestCase(TestMethod(typeName, methodName), displayName, skipReason, traits, fileName, lineNumber, uniqueID);

	public static _ITestCase TestCase(
		_ITestMethod? testMethod = null,
		string? displayName = null,
		string? skipReason = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null,
		string? fileName = null,
		int? lineNumber = null,
		string? uniqueID = null)
	{
		testMethod ??= TestMethod();
		displayName ??= $"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}";
		traits ??= GetTraits(testMethod.Method);
		uniqueID ??= "case-id";
		var testCollection = testMethod.TestClass.TestCollection;
		var testClass = testMethod.TestClass;

		var result = Substitute.For<_ITestCase, InterfaceProxy<_ITestCase>>();
		result.TestCaseDisplayName.Returns(displayName);
		result.SkipReason.Returns(skipReason);
		result.SourceFilePath.Returns(fileName);
		result.SourceLineNumber.Returns(lineNumber);
		result.TestCollection.Returns(testCollection);
		result.TestClass.Returns(testClass);
		result.TestMethod.Returns(testMethod);
		result.Traits.Returns(traits);
		result.UniqueID.Returns(uniqueID);

		return result;
	}

	public static _ITestClass TestClass<TClassUnderTest>(
		_ITestCollection? collection = null,
		string? uniqueID = null) =>
			TestClass(
				TestData.TypeInfo<TClassUnderTest>(),
				collection,
				uniqueID
			);

	public static _ITestClass TestClass(
		string? typeName,
		_IMethodInfo[]? methods = null,
		_IReflectionAttributeInfo[]? attributes = null,
		_ITypeInfo? baseType = null,
		_ITestCollection? collection = null,
		string? uniqueID = null) =>
			TestClass(TypeInfo(typeName, methods, attributes, baseType), collection, uniqueID);

	public static _ITestClass TestClass(
		_ITypeInfo? typeInfo = null,
		_ITestCollection? collection = null,
		string? uniqueID = null)
	{
		typeInfo ??= TypeInfo();
		collection ??= TestCollection();
		uniqueID ??= "class-id";

		var result = Substitute.For<_ITestClass, InterfaceProxy<_ITestClass>>();
		result.Class.Returns(typeInfo);
		result.TestCollection.Returns(collection);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static _ITestCollection TestCollection(
		Assembly assembly,
		_ITypeInfo? definition = null,
		string? displayName = null,
		string? uniqueID = null) =>
			TestCollection(TestAssembly(assembly), definition, displayName, uniqueID);

	public static _ITestCollection TestCollection(
		_ITestAssembly? assembly = null,
		_ITypeInfo? definition = null,
		string? displayName = null,
		string? uniqueID = null)
	{
		assembly ??= TestAssembly();
		displayName ??= "Mock test collection";
		uniqueID ??= "collection-id";

		var result = Substitute.For<_ITestCollection, InterfaceProxy<_ITestCollection>>();
		result.CollectionDefinition.Returns(definition);
		result.DisplayName.Returns(displayName);
		result.TestAssembly.Returns(assembly);
		result.UniqueID.Returns(uniqueID);
		return result;
	}

	public static _ITestMethod TestMethod(
		string? typeName = null,
		string? methodName = null,
		string? displayName = null,
		bool? @explicit = null,
		string? skip = null,
		int timeout = 0,
		_IParameterInfo[]? parameters = null,
		_IReflectionAttributeInfo[]? classAttributes = null,
		_IReflectionAttributeInfo[]? methodAttributes = null,
		_ITestCollection? collection = null,
		string? uniqueID = null)
	{
		parameters ??= EmptyParameterInfos;
		classAttributes ??= EmptyAttributeInfos;
		methodAttributes ??= EmptyAttributeInfos;

		// Ensure that there's a FactAttribute, or else it's not technically a test method
		var factAttribute = methodAttributes.FirstOrDefault(attr => typeof(FactAttribute).IsAssignableFrom(attr.AttributeType));
		if (factAttribute is null)
		{
			factAttribute = FactAttribute(displayName, @explicit, skip, timeout);
			methodAttributes = methodAttributes.Concat(new[] { factAttribute }).ToArray();
		}

		var testClass = TestClass(typeName, attributes: classAttributes, collection: collection);
		var methodInfo = MethodInfo(methodName, methodAttributes, parameters, testClass.Class);

		return TestMethod(methodInfo, testClass, uniqueID);
	}

	public static _ITestMethod TestMethod<TClassUnderTest>(
		string methodName,
		_ITestCollection? collection = null,
		string? uniqueID = null) =>
			TestMethod(TestData.MethodInfo<TClassUnderTest>(methodName), TestClass<TClassUnderTest>(collection), uniqueID);

	public static _ITestMethod TestMethod(
		_IMethodInfo methodInfo,
		_ITestClass testClass,
		string? uniqueID = null)
	{
		uniqueID ??= "method-id";

		var result = Substitute.For<_ITestMethod, InterfaceProxy<_ITestMethod>>();
		result.Method.Returns(methodInfo);
		result.TestClass.Returns(testClass);
		result.UniqueID.Returns(uniqueID);
		return result;
	}
}

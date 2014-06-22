﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public static class Mocks
{
    public static IAssemblyInfo AssemblyInfo(ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null, string assemblyFileName = null)
    {
        attributes = attributes ?? new IReflectionAttributeInfo[0];

        var result = Substitute.For<IAssemblyInfo>();
        result.AssemblyPath.Returns(assemblyFileName);
        result.GetType("").ReturnsForAnyArgs(types == null ? null : types.FirstOrDefault());
        result.GetTypes(true).ReturnsForAnyArgs(types ?? new ITypeInfo[0]);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        return result;
    }

    public static IReflectionAttributeInfo CollectionAttribute(string collectionName)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new CollectionAttribute(collectionName));
        result.GetConstructorArguments().Returns(new[] { collectionName });
        return result;
    }

    public static IReflectionAttributeInfo CollectionBehaviorAttribute(CollectionBehavior? collectionBehavior = null, bool disableTestParallelization = false, int maxParallelThreads = 0)
    {
        CollectionBehaviorAttribute attribute;
        var result = Substitute.For<IReflectionAttributeInfo>();

        if (collectionBehavior.HasValue)
        {
            attribute = new CollectionBehaviorAttribute(collectionBehavior.GetValueOrDefault());
            result.GetConstructorArguments().Returns(new object[] { collectionBehavior });
        }
        else
        {
            attribute = new CollectionBehaviorAttribute();
            result.GetConstructorArguments().Returns(new object[0]);
        }

        attribute.DisableTestParallelization = disableTestParallelization;
        attribute.MaxParallelThreads = maxParallelThreads;

        result.Attribute.Returns(attribute);
        result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
        result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
        return result;
    }

    public static IReflectionAttributeInfo CollectionBehaviorAttribute(string factoryTypeName, string factoryAssemblyName, bool disableTestParallelization = false, int maxParallelThreads = 0)
    {
        var attribute = new CollectionBehaviorAttribute(factoryTypeName, factoryAssemblyName)
        {
            DisableTestParallelization = disableTestParallelization,
            MaxParallelThreads = maxParallelThreads
        };
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(attribute);
        result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
        result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
        result.GetConstructorArguments().Returns(new object[] { factoryTypeName, factoryAssemblyName });
        return result;
    }

    public static IReflectionAttributeInfo CollectionDefinitionAttribute(string collectionName)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new CollectionDefinitionAttribute(collectionName));
        result.GetConstructorArguments().Returns(new[] { collectionName });
        return result;
    }

    public static IReflectionAttributeInfo DataAttribute(IEnumerable<object[]> data = null)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        var dataAttribute = Substitute.For<DataAttribute>();
        dataAttribute.GetData(null).ReturnsForAnyArgs(data);
        result.Attribute.Returns(dataAttribute);
        return result;
    }

    public static IReflectionAttributeInfo FactAttribute(string displayName = null, string skip = null)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        return result;
    }

    public static LambdaTestCase LambdaTestCase(Action lambda)
    {
        var testMethod = Mocks.TestMethod();

        return new LambdaTestCase(testMethod, lambda);
    }

    static IEnumerable<IAttributeInfo> LookupAttribute(string fullyQualifiedTypeName, IReflectionAttributeInfo[] attributes)
    {
        if (attributes == null)
            return Enumerable.Empty<IAttributeInfo>();

        var attributeType = GetType(fullyQualifiedTypeName);
        return attributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
    }

    public static IMethodInfo MethodInfo(string methodName = "MockMethod",
                                         IReflectionAttributeInfo[] attributes = null,
                                         IParameterInfo[] parameters = null,
                                         ITypeInfo type = null,
                                         ITypeInfo returnType = null,
                                         bool isAbstract = false,
                                         bool isPublic = true,
                                         bool isStatic = false)
    {
        var result = Substitute.For<IMethodInfo>();

        attributes = attributes ?? new IReflectionAttributeInfo[0];
        parameters = parameters ?? new IParameterInfo[0];

        result.IsAbstract.Returns(isAbstract);
        result.IsPublic.Returns(isPublic);
        result.IsStatic.Returns(isStatic);
        result.Name.Returns(methodName);
        result.ReturnType.Returns(returnType);
        result.Type.Returns(type);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        result.GetParameters().Returns(parameters);

        return result;
    }

    public static IParameterInfo ParameterInfo(string name)
    {
        var result = Substitute.For<IParameterInfo>();
        result.Name.Returns(name);
        return result;
    }

    public static ITestAssembly TestAssembly(ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null)
    {
        var assemblyInfo = Mocks.AssemblyInfo(types, attributes);

        var result = Substitute.For<ITestAssembly>();
        result.Assembly.Returns(assemblyInfo);
        return result;
    }

    public static ITestAssembly TestAssembly(Assembly assembly, string configFileName = null)
    {
        var result = Substitute.For<ITestAssembly>();
        result.Assembly.Returns(Reflector.Wrap(assembly));
        result.ConfigFileName.Returns(configFileName ?? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
        return result;
    }

    public static ITestAssembly TestAssembly(string assemblyFileName, string configFileName = null, ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null)
    {
        var assemblyInfo = Mocks.AssemblyInfo(types, attributes, assemblyFileName);

        var result = Substitute.For<ITestAssembly>();
        result.Assembly.Returns(assemblyInfo);
        result.ConfigFileName.Returns(configFileName);
        return result;
    }

    public static ITestCase TestCase(ITestCollection collection)
    {
        var result = Substitute.For<ITestCase>();
        result.TestMethod.TestClass.TestCollection.Returns(collection);
        return result;
    }

    public static ITestCase TestCase<TClassUnderTest>(string methodName, string displayName = null, string skipReason = null, string uniqueID = null)
    {
        return TestCase(typeof(TClassUnderTest), methodName, displayName, skipReason, uniqueID);
    }

    public static ITestCase TestCase(Type type, string methodName, string displayName = null, string skipReason = null, string uniqueID = null)
    {
        var testMethod = Mocks.TestMethod(type, methodName);
        var traits = GetTraits(testMethod.Method);

        var result = Substitute.For<ITestCase>();
        result.DisplayName.Returns(displayName ?? String.Format("{0}.{1}", type, methodName));
        result.SkipReason.Returns(skipReason);
        result.TestMethod.Returns(testMethod);
        result.Traits.Returns(traits);
        result.UniqueID.Returns(uniqueID ?? Guid.NewGuid().ToString());
        return result;
    }

    public static ITestClass TestClass(string typeName, IReflectionAttributeInfo[] attributes = null)
    {
        var testCollection = Mocks.TestCollection();
        var typeInfo = Mocks.TypeInfo(typeName, attributes: attributes);

        var result = Substitute.For<ITestClass>();
        result.Class.Returns(typeInfo);
        result.TestCollection.Returns(testCollection);
        return result;
    }

    public static ITestClass TestClass(Type type, ITestCollection testCollection = null)
    {
        if (testCollection == null)
        {
            var assembly = Mocks.TestAssembly(type.Assembly);
            testCollection = Mocks.TestCollection(testAssembly: assembly);
        }

        var result = Substitute.For<ITestClass>();
        result.Class.Returns(Reflector.Wrap(type));
        result.TestCollection.Returns(testCollection);
        return result;
    }

    public static ITestCollection TestCollection(Guid? id = null, ITypeInfo definition = null, ITestAssembly testAssembly = null)
    {
        if (testAssembly == null)
            testAssembly = Mocks.TestAssembly();

        var result = Substitute.For<ITestCollection>();
        result.TestAssembly.Returns(testAssembly);
        result.UniqueID.Returns(id ?? Guid.NewGuid());
        result.CollectionDefinition.Returns(definition);
        return result;
    }

    public static ITestFailed TestFailed(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M, Exception ex = null)
    {
        var testCase = Mocks.TestCase(type, methodName);
        var failureInfo = Xunit.Sdk.ExceptionUtility.ConvertExceptionToFailureInformation(ex ?? new Exception());

        var result = Substitute.For<ITestFailed>();
        result.ExceptionParentIndices.Returns(failureInfo.ExceptionParentIndices);
        result.ExceptionTypes.Returns(failureInfo.ExceptionTypes);
        result.ExecutionTime.Returns(executionTime);
        result.Messages.Returns(failureInfo.Messages);
        result.Output.Returns(output);
        result.StackTraces.Returns(failureInfo.StackTraces);
        result.TestCase.Returns(testCase);
        result.TestDisplayName.Returns(displayName ?? "NO DISPLAY NAME");
        return result;
    }

    public static ITestMethod TestMethod(string typeName = "MockType",
                                         string methodName = "MockMethod",
                                         string displayName = null,
                                         string skip = null,
                                         IEnumerable<IParameterInfo> parameters = null,
                                         IEnumerable<IReflectionAttributeInfo> classAttributes = null,
                                         IEnumerable<IReflectionAttributeInfo> methodAttributes = null)
    {
        if (classAttributes == null)
            classAttributes = Enumerable.Empty<IReflectionAttributeInfo>();
        if (methodAttributes == null)
            methodAttributes = Enumerable.Empty<IReflectionAttributeInfo>();
        if (parameters == null)
            parameters = Enumerable.Empty<IParameterInfo>();

        var factAttribute = methodAttributes.FirstOrDefault(attr => typeof(FactAttribute).IsAssignableFrom(attr.Attribute.GetType()));
        if (factAttribute == null)
        {
            factAttribute = Mocks.FactAttribute(displayName, skip);
            methodAttributes = methodAttributes.Concat(new[] { factAttribute });
        }

        var testClass = Mocks.TestClass(typeName, attributes: classAttributes.ToArray());
        var methodInfo = Mocks.MethodInfo(methodName, methodAttributes.ToArray(), parameters.ToArray(), testClass.Class);

        var result = Substitute.For<ITestMethod>();
        result.Method.Returns(methodInfo);
        result.TestClass.Returns(testClass);
        return result;
    }

    public static ITestMethod TestMethod(Type type, string methodName, ITestCollection collection = null)
    {
        var methodInfo = type.GetMethod(methodName);
        if (methodInfo == null)
            throw new Exception(String.Format("Unknown method: {0}.{1}", type.FullName, methodName));

        var testClass = Mocks.TestClass(type, collection);

        var result = Substitute.For<ITestMethod>();
        result.Method.Returns(Reflector.Wrap(methodInfo));
        result.TestClass.Returns(testClass);
        return result;
    }

    public static ITestPassed TestPassed(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M)
    {
        var testCase = Mocks.TestCase(type, methodName);

        var result = Substitute.For<ITestPassed>();
        result.ExecutionTime.Returns(executionTime);
        result.Output.Returns(output);
        result.TestCase.Returns(testCase);
        result.TestDisplayName.Returns(displayName ?? "NO DISPLAY NAME");
        return result;
    }

    public static ITestResultMessage TestResult<TClassUnderTest>(string methodName, string displayName, decimal executionTime)
    {
        var testCase = Mocks.TestCase<TClassUnderTest>(methodName);
        var result = Substitute.For<ITestResultMessage>();
        result.TestCase.Returns(testCase);
        result.TestDisplayName.Returns(displayName);
        result.ExecutionTime.Returns(executionTime);
        return result;
    }

    public static ITestSkipped TestSkipped(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M, string skipReason = null)
    {
        var testCase = Mocks.TestCase(type, methodName);

        var result = Substitute.For<ITestSkipped>();
        result.ExecutionTime.Returns(executionTime);
        result.Output.Returns(output);
        result.Reason.Returns(skipReason);
        result.TestCase.Returns(testCase);
        result.TestDisplayName.Returns(displayName ?? "NO DISPLAY NAME");
        return result;
    }

    public static IReflectionAttributeInfo TheoryAttribute(string displayName = null, string skip = null)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new TheoryAttribute { DisplayName = displayName, Skip = skip });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        return result;
    }

    public static IReflectionAttributeInfo TraitAttribute(string name, string value)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        var traitDiscovererAttributes = new[] { Mocks.TraitDiscovererAttribute() };
        result.GetCustomAttributes(typeof(TraitDiscovererAttribute)).Returns(traitDiscovererAttributes);
        result.Attribute.Returns(new TraitAttribute(name, value));
        result.GetConstructorArguments().Returns(new object[] { name, value });
        return result;
    }

    public static IAttributeInfo TraitDiscovererAttribute(string typeName = "Xunit.Sdk.TraitDiscoverer", string assemblyName = "xunit.core")
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new TraitDiscovererAttribute(typeName, assemblyName));
        result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
        return result;
    }

    public static ITypeInfo TypeInfo(string typeName = "MockType", IMethodInfo[] methods = null, IReflectionAttributeInfo[] attributes = null)
    {
        var result = Substitute.For<ITypeInfo>();
        result.Name.Returns(typeName);
        result.GetMethods(false).ReturnsForAnyArgs(methods ?? new IMethodInfo[0]);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        return result;
    }

    public static XunitTestAssembly XunitTestAssembly(Assembly assembly = null, string configFileName = null)
    {
        return new XunitTestAssembly(Reflector.Wrap(assembly ?? Assembly.GetExecutingAssembly()), configFileName);
    }

    public static XunitTestCase XunitTestCase<TClassUnderTest>(string methodName, ITestCollection collection = null, object[] testMethodArguments = null)
    {
        var method = Mocks.XunitTestMethod(typeof(TClassUnderTest), methodName, collection);

        return new XunitTestCase(method, testMethodArguments);
    }

    public static XunitTestCollection XunitTestCollection(Assembly assembly = null, ITypeInfo collectionDefinition = null, string displayName = null)
    {
        if (assembly == null)
            assembly = Assembly.GetExecutingAssembly();
        if (displayName == null)
            displayName = "Mock test collection for " + assembly.GetLocalCodeBase();

        return new XunitTestCollection(Mocks.XunitTestAssembly(assembly), collectionDefinition, displayName);
    }

    public static XunitTestClass XunitTestClass(Type type, ITestCollection collection = null)
    {
        if (collection == null)
            collection = Mocks.XunitTestCollection(type.Assembly);

        return new XunitTestClass(collection, Reflector.Wrap(type));
    }

    public static XunitTestMethod XunitTestMethod(Type type, string methodName, ITestCollection collection = null)
    {
        var @class = Mocks.XunitTestClass(type, collection);
        var methodInfo = type.GetMethod(methodName);
        if (methodInfo == null)
            throw new Exception(String.Format("Unknown method: {0}.{1}", type.FullName, methodName));

        return new XunitTestMethod(@class, Reflector.Wrap(methodInfo));
    }

    public static XunitTheoryTestCase XunitTheoryTestCase<TClassUnderTest>(string methodName, ITestCollection collection = null)
    {
        var method = Mocks.XunitTestMethod(typeof(TClassUnderTest), methodName, collection);

        return new XunitTheoryTestCase(method);
    }

    private static Dictionary<string, List<string>> GetTraits(IMethodInfo method)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var traitAttribute in method.GetCustomAttributes(typeof(TraitAttribute)))
        {
            var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
            result.Add((string)ctorArgs[0], (string)ctorArgs[1]);
        }

        return result;
    }

    private static Type GetType(string assemblyQualifiedAttributeTypeName)
    {
        var parts = assemblyQualifiedAttributeTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
        if (parts.Count == 0)
            return null;

        if (parts.Count == 1)
            return Type.GetType(parts[0]);

        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == parts[1]);
        if (assembly == null)
            return null;

        return assembly.GetType(parts[0]);
    }
}
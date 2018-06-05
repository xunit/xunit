using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;
using TestMethodDisplayOptions = Xunit.Sdk.TestMethodDisplayOptions;

public static class Mocks
{
    public static IAssemblyInfo AssemblyInfo(ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null, string assemblyFileName = null)
    {
        attributes = attributes ?? new IReflectionAttributeInfo[0];

        var result = Substitute.For<IAssemblyInfo, InterfaceProxy<IAssemblyInfo>>();
        result.Name.Returns(assemblyFileName == null ? "assembly:" + Guid.NewGuid().ToString("n") : Path.GetFileNameWithoutExtension(assemblyFileName));
        result.AssemblyPath.Returns(assemblyFileName);
        result.GetType("").ReturnsForAnyArgs(types?.FirstOrDefault());
        result.GetTypes(true).ReturnsForAnyArgs(types ?? new ITypeInfo[0]);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        return result;
    }

    public static IReflectionAttributeInfo CollectionAttribute(string collectionName)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new CollectionAttribute(collectionName));
        result.GetConstructorArguments().Returns(new[] { collectionName });
        return result;
    }

    public static IReflectionAttributeInfo CollectionBehaviorAttribute(CollectionBehavior? collectionBehavior = null, bool disableTestParallelization = false, int maxParallelThreads = 0)
    {
        CollectionBehaviorAttribute attribute;
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();

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
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(attribute);
        result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
        result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
        result.GetConstructorArguments().Returns(new object[] { factoryTypeName, factoryAssemblyName });
        return result;
    }

    public static IReflectionAttributeInfo CollectionDefinitionAttribute(string collectionName)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new CollectionDefinitionAttribute(collectionName));
        result.GetConstructorArguments().Returns(new[] { collectionName });
        return result;
    }

    public static IReflectionAttributeInfo DataAttribute(IEnumerable<object[]> data = null)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        var dataAttribute = Substitute.For<DataAttribute>();
        dataAttribute.GetData(null).ReturnsForAnyArgs(data);
        result.Attribute.Returns(dataAttribute);
        return result;
    }

    public static ExecutionErrorTestCase ExecutionErrorTestCase(string message, IMessageSink diagnosticMessageSink = null)
    {
        var testMethod = TestMethod();
        return new ExecutionErrorTestCase(diagnosticMessageSink ?? new Xunit.NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, testMethod, message);
    }

    public static IReflectionAttributeInfo FactAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        result.GetNamedArgument<int>("Timeout").Returns(timeout);
        return result;
    }

    static IEnumerable<IAttributeInfo> LookupAttribute(string fullyQualifiedTypeName, IReflectionAttributeInfo[] attributes)
    {
        if (attributes == null)
            return Enumerable.Empty<IAttributeInfo>();

        var attributeType = Type.GetType(fullyQualifiedTypeName);
        if (attributeType == null)
            return Enumerable.Empty<IAttributeInfo>();

        return attributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
    }

    public static IMethodInfo MethodInfo(string methodName = null,
                                         IReflectionAttributeInfo[] attributes = null,
                                         IParameterInfo[] parameters = null,
                                         ITypeInfo type = null,
                                         ITypeInfo returnType = null,
                                         bool isAbstract = false,
                                         bool isPublic = true,
                                         bool isStatic = false)
    {
        var result = Substitute.For<IMethodInfo, InterfaceProxy<IMethodInfo>>();

        attributes = attributes ?? new IReflectionAttributeInfo[0];
        parameters = parameters ?? new IParameterInfo[0];

        result.IsAbstract.Returns(isAbstract);
        result.IsPublic.Returns(isPublic);
        result.IsStatic.Returns(isStatic);
        result.Name.Returns(methodName ?? "method:" + Guid.NewGuid().ToString("n"));
        result.ReturnType.Returns(returnType);
        result.Type.Returns(type);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        result.GetParameters().Returns(parameters);

        return result;
    }

    public static IParameterInfo ParameterInfo(string name)
    {
        var result = Substitute.For<IParameterInfo, InterfaceProxy<IParameterInfo>>();
        result.Name.Returns(name);
        return result;
    }

    public static IReflectionMethodInfo ReflectionMethodInfo<TClass>(string methodName)
    {
        return Reflector.Wrap(typeof(TClass).GetMethod(methodName));
    }

    public static IReflectionTypeInfo ReflectionTypeInfo<TClass>()
    {
        return Reflector.Wrap(typeof(TClass));
    }

    public static IRunnerReporter RunnerReporter(string runnerSwitch = null,
                                                 string description = null,
                                                 bool isEnvironmentallyEnabled = false,
                                                 IMessageSinkWithTypes messageSink = null)
    {
        var result = Substitute.For<IRunnerReporter, InterfaceProxy<IRunnerReporter>>();
        result.Description.Returns(description ?? "The runner reporter description");
        result.IsEnvironmentallyEnabled.ReturnsForAnyArgs(isEnvironmentallyEnabled);
        result.RunnerSwitch.Returns(runnerSwitch);
        var dualSink = MessageSinkAdapter.Wrap(messageSink ?? Substitute.For<IMessageSinkWithTypes, InterfaceProxy<IMessageSinkWithTypes>>());
        result.CreateMessageHandler(null).ReturnsForAnyArgs(dualSink);
        return result;
    }

    public static ITest Test(ITestCase testCase, string displayName)
    {
        var result = Substitute.For<ITest, InterfaceProxy<ITest>>();
        result.DisplayName.Returns(displayName);
        result.TestCase.Returns(testCase);
        return result;
    }

    public static ITestAssembly TestAssembly(IReflectionAttributeInfo[] attributes)
    {
        var assemblyInfo = AssemblyInfo(attributes: attributes);

        var result = Substitute.For<ITestAssembly, InterfaceProxy<ITestAssembly>>();
        result.Assembly.Returns(assemblyInfo);
        return result;
    }

    public static ITestAssembly TestAssembly(string assemblyFileName, string configFileName = null, ITypeInfo[] types = null, IReflectionAttributeInfo[] attributes = null)
    {
        var assemblyInfo = AssemblyInfo(types, attributes, assemblyFileName);

        var result = Substitute.For<ITestAssembly, InterfaceProxy<ITestAssembly>>();
        result.Assembly.Returns(assemblyInfo);
        result.ConfigFileName.Returns(configFileName);
        return result;
    }

    public static TestAssembly TestAssembly(Assembly assembly = null, string configFileName = null)
    {
#if NETFRAMEWORK
        if (configFileName == null)
            configFileName = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
#endif

        return new TestAssembly(Reflector.Wrap(assembly ?? typeof(Mocks).GetTypeInfo().Assembly), configFileName);
    }

    public static ITestAssemblyDiscoveryFinished TestAssemblyDiscoveryFinished(bool diagnosticMessages = false, int toRun = 42, int discovered = 2112)
    {
        var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
        var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, ShadowCopy = true };
        var result = Substitute.For<ITestAssemblyDiscoveryFinished, InterfaceProxy<ITestAssemblyDiscoveryFinished>>();
        result.Assembly.Returns(assembly);
        result.DiscoveryOptions.Returns(TestFrameworkOptions.ForDiscovery(config));
        result.TestCasesDiscovered.Returns(discovered);
        result.TestCasesToRun.Returns(toRun);
        return result;
    }

    public static ITestAssemblyDiscoveryStarting TestAssemblyDiscoveryStarting(bool diagnosticMessages = false, bool appDomain = false)
    {
        var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
        var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, MethodDisplay = Xunit.TestMethodDisplay.ClassAndMethod, MaxParallelThreads = 42, ParallelizeTestCollections = true, ShadowCopy = true };
        var result = Substitute.For<ITestAssemblyDiscoveryStarting, InterfaceProxy<ITestAssemblyDiscoveryStarting>>();
        result.AppDomain.Returns(appDomain);
        result.Assembly.Returns(assembly);
        result.DiscoveryOptions.Returns(TestFrameworkOptions.ForDiscovery(config));
        return result;
    }

    public static ITestAssemblyExecutionFinished TestAssemblyExecutionFinished(bool diagnosticMessages = false, int total = 2112, int failed = 42, int skipped = 8, int errors = 6, decimal time = 123.456M)
    {
        var assembly = new XunitProjectAssembly { AssemblyFilename = "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
        var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, ShadowCopy = true };
        var summary = new ExecutionSummary { Total = total, Failed = failed, Skipped = skipped, Errors = errors, Time = time };
        var result = Substitute.For<ITestAssemblyExecutionFinished, InterfaceProxy<ITestAssemblyExecutionFinished>>();
        result.Assembly.Returns(assembly);
        result.ExecutionOptions.Returns(TestFrameworkOptions.ForExecution(config));
        result.ExecutionSummary.Returns(summary);
        return result;
    }

    public static ITestAssemblyExecutionStarting TestAssemblyExecutionStarting(bool diagnosticMessages = false, string assemblyFilename = null)
    {
        var assembly = new XunitProjectAssembly { AssemblyFilename = assemblyFilename ?? "testAssembly.dll", ConfigFilename = "testAssembly.dll.config" };
        var config = new TestAssemblyConfiguration { DiagnosticMessages = diagnosticMessages, MethodDisplay = Xunit.TestMethodDisplay.ClassAndMethod, MaxParallelThreads = 42, ParallelizeTestCollections = true, ShadowCopy = true };
        var result = Substitute.For<ITestAssemblyExecutionStarting, InterfaceProxy<ITestAssemblyExecutionStarting>>();
        result.Assembly.Returns(assembly);
        result.ExecutionOptions.Returns(TestFrameworkOptions.ForExecution(config));
        return result;
    }

    public static ITestAssemblyFinished TestAssemblyFinished(int testsRun = 2112, int testsFailed = 42, int testsSkipped = 6, decimal executionTime = 123.4567M)
    {
        var testAssembly = TestAssembly("testAssembly.dll");
        var result = Substitute.For<ITestAssemblyFinished, InterfaceProxy<ITestAssemblyFinished>>();
        result.TestAssembly.Returns(testAssembly);
        result.TestsRun.Returns(testsRun);
        result.TestsFailed.Returns(testsFailed);
        result.TestsSkipped.Returns(testsSkipped);
        result.ExecutionTime.Returns(executionTime);
        return result;
    }

    public static ITestAssemblyStarting TestAssemblyStarting()
    {
        var testAssembly = TestAssembly("testAssembly.dll");
        var result = Substitute.For<ITestAssemblyStarting, InterfaceProxy<ITestAssemblyStarting>>();
        result.TestAssembly.Returns(testAssembly);
        return result;
    }

    public static ITestCase TestCase(ITestCollection collection = null)
    {
        if (collection == null)
            collection = TestCollection();

        var result = Substitute.For<ITestCase, InterfaceProxy<ITestCase>>();
        result.TestMethod.TestClass.TestCollection.Returns(collection);
        return result;
    }

    public static ITestCase TestCase(ITestMethod testMethod)
    {
        var result = Substitute.For<ITestCase, InterfaceProxy<ITestCase>>();
        result.TestMethod.Returns(testMethod);
        return result;
    }

    public static ITestCase TestCase<TClassUnderTest>(string methodName, string displayName = null, string skipReason = null, string uniqueID = null)
    {
        return TestCase(typeof(TClassUnderTest), methodName, displayName, skipReason, uniqueID);
    }

    public static ITestCase TestCase(Type type, string methodName, string displayName = null, string skipReason = null, string uniqueID = null)
    {
        var testMethod = TestMethod(type, methodName);
        var traits = GetTraits(testMethod.Method);

        var result = Substitute.For<ITestCase, InterfaceProxy<ITestCase>>();
        result.DisplayName.Returns(displayName ?? $"{type}.{methodName}");
        result.SkipReason.Returns(skipReason);
        result.TestMethod.Returns(testMethod);
        result.Traits.Returns(traits);
        result.UniqueID.Returns(uniqueID ?? Guid.NewGuid().ToString());
        return result;
    }

    public static IReflectionAttributeInfo TestCaseOrdererAttribute(string typeName, string assemblyName)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new TestCaseOrdererAttribute(typeName, assemblyName));
        result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
        return result;
    }

    public static IReflectionAttributeInfo TestCaseOrdererAttribute<TOrderer>()
    {
        var ordererType = typeof(TOrderer);
        return TestCaseOrdererAttribute(ordererType.FullName, ordererType.GetTypeInfo().Assembly.FullName);
    }

    public static ITestClass TestClass(string typeName, IReflectionAttributeInfo[] attributes = null)
    {
        var testCollection = TestCollection();
        var typeInfo = TypeInfo(typeName, attributes: attributes);

        var result = Substitute.For<ITestClass, InterfaceProxy<ITestClass>>();
        result.Class.Returns(typeInfo);
        result.TestCollection.Returns(testCollection);
        return result;
    }

    public static TestClass TestClass(Type type, ITestCollection collection = null)
    {
        if (collection == null)
            collection = TestCollection(type.GetTypeInfo().Assembly);

        return new TestClass(collection, Reflector.Wrap(type));
    }

    public static TestCollection TestCollection(Assembly assembly = null, ITypeInfo definition = null, string displayName = null)
    {
        if (assembly == null)
            assembly = typeof(Mocks).GetTypeInfo().Assembly;
        if (displayName == null)
            displayName = "Mock test collection for " + assembly.CodeBase;

        return new TestCollection(TestAssembly(assembly), definition, displayName);
    }

    public static ITestCollectionFinished TestCollectionFinished(string displayName = "Display Name", int testsRun = 2112, int testsFailed = 42, int testsSkipped = 6, decimal executionTime = 123.4567M)
    {
        var result = Substitute.For<ITestCollectionFinished, InterfaceProxy<ITestCollectionFinished>>();
        result.TestsRun.Returns(testsRun);
        result.TestsFailed.Returns(testsFailed);
        result.TestsSkipped.Returns(testsSkipped);
        result.ExecutionTime.Returns(executionTime);
        result.TestCollection.DisplayName.Returns(displayName);
        return result;
    }

    public static ITestCollectionStarting TestCollectionStarting()
    {
        var result = Substitute.For<ITestCollectionStarting, InterfaceProxy<ITestCollectionStarting>>();
        result.TestCollection.DisplayName.Returns("Display Name");
        return result;
    }

    public static IReflectionAttributeInfo TestCollectionOrdererAttribute(string typeName, string assemblyName)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new TestCollectionOrdererAttribute(typeName, assemblyName));
        result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
        return result;
    }

    public static IReflectionAttributeInfo TestCollectionOrdererAttribute<TOrderer>()
    {
        var ordererType = typeof(TOrderer);
        return TestCollectionOrdererAttribute(ordererType.FullName, ordererType.GetTypeInfo().Assembly.FullName);
    }

    public static ITestFailed TestFailed(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M, Exception ex = null)
    {
        var testCase = TestCase(type, methodName);
        var test = Test(testCase, displayName ?? "NO DISPLAY NAME");
        var failureInfo = Xunit.Sdk.ExceptionUtility.ConvertExceptionToFailureInformation(ex ?? new Exception());

        var result = Substitute.For<ITestFailed, InterfaceProxy<ITestFailed>>();
        result.ExceptionParentIndices.Returns(failureInfo.ExceptionParentIndices);
        result.ExceptionTypes.Returns(failureInfo.ExceptionTypes);
        result.ExecutionTime.Returns(executionTime);
        result.Messages.Returns(failureInfo.Messages);
        result.Output.Returns(output);
        result.StackTraces.Returns(failureInfo.StackTraces);
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        return result;
    }

    public static ITestFailed TestFailed(string displayName, decimal executionTime, string exceptionType = null, string exceptionMessage = null, string stackTrace = null, string output = null)
    {
        var testCase = TestCase();
        var test = Test(testCase, displayName);
        var result = Substitute.For<ITestFailed, InterfaceProxy<ITestFailed>>();
        result.ExceptionParentIndices.Returns(new[] { -1 });
        result.ExceptionTypes.Returns(new[] { exceptionType });
        result.ExecutionTime.Returns(executionTime);
        result.Messages.Returns(new[] { exceptionMessage });
        result.Output.Returns(output);
        result.StackTraces.Returns(new[] { stackTrace });
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        return result;
    }

    public static IReflectionAttributeInfo TestFrameworkAttribute(Type type)
    {
        var attribute = Activator.CreateInstance(type);
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(attribute);
        result.GetCustomAttributes(null).ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), CustomAttributeData.GetCustomAttributes(attribute.GetType().GetTypeInfo()).Select(Reflector.Wrap).ToArray()));
        return result;
    }

    public static ITestMethod TestMethod(string typeName = null,
                                         string methodName = null,
                                         string displayName = null,
                                         string skip = null,
                                         int timeout = 0,
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
            factAttribute = FactAttribute(displayName, skip, timeout);
            methodAttributes = methodAttributes.Concat(new[] { factAttribute });
        }

        var testClass = TestClass(typeName, attributes: classAttributes.ToArray());
        var methodInfo = MethodInfo(methodName, methodAttributes.ToArray(), parameters.ToArray(), testClass.Class);

        var result = Substitute.For<ITestMethod, InterfaceProxy<ITestMethod>>();
        result.Method.Returns(methodInfo);
        result.TestClass.Returns(testClass);
        return result;
    }

    public static TestMethod TestMethod(Type type, string methodName, ITestCollection collection = null)
    {
        var @class = TestClass(type, collection);
        var methodInfo = type.GetMethod(methodName);
        if (methodInfo == null)
            throw new Exception($"Unknown method: {type.FullName}.{methodName}");

        return new TestMethod(@class, Reflector.Wrap(methodInfo));
    }

    public static ITestPassed TestPassed(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M)
    {
        var testCase = TestCase(type, methodName);
        var test = Test(testCase, displayName ?? "NO DISPLAY NAME");

        var result = Substitute.For<ITestPassed, InterfaceProxy<ITestPassed>>();
        result.ExecutionTime.Returns(executionTime);
        result.Output.Returns(output);
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        return result;
    }

    public static ITestPassed TestPassed(string displayName, string output = null)
    {
        var testCase = TestCase();
        var test = Test(testCase, displayName);
        var result = Substitute.For<ITestPassed, InterfaceProxy<ITestPassed>>();
        result.Test.Returns(test);
        result.ExecutionTime.Returns(1.2345M);
        result.Output.Returns(output);
        return result;
    }

    public static ITestResultMessage TestResult<TClassUnderTest>(string methodName, string displayName, decimal executionTime)
    {
        var testCase = TestCase<TClassUnderTest>(methodName);
        var test = Test(testCase, displayName);
        var result = Substitute.For<ITestResultMessage, InterfaceProxy<ITestResultMessage>>();
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        result.ExecutionTime.Returns(executionTime);
        return result;
    }

    public static ITestSkipped TestSkipped(Type type, string methodName, string displayName = null, string output = null, decimal executionTime = 0M, string skipReason = null)
    {
        var testCase = TestCase(type, methodName);
        var test = Test(testCase, displayName ?? "NO DISPLAY NAME");

        var result = Substitute.For<ITestSkipped, InterfaceProxy<ITestSkipped>>();
        result.ExecutionTime.Returns(executionTime);
        result.Output.Returns(output);
        result.Reason.Returns(skipReason);
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        return result;
    }

    public static ITestSkipped TestSkipped(string displayName, string skipReason = null)
    {
        var testCase = TestCase();
        var test = Test(testCase, displayName);

        var result = Substitute.For<ITestSkipped, InterfaceProxy<ITestSkipped>>();
        result.Reason.Returns(skipReason);
        result.TestCase.Returns(testCase);
        result.Test.Returns(test);
        return result;
    }

    public static ITestStarting TestStarting(string displayName)
    {
        var testCase = TestCase();
        var test = Test(testCase, displayName);
        var result = Substitute.For<ITestStarting, InterfaceProxy<ITestStarting>>();
        result.Test.Returns(test);
        return result;
    }

    public static IReflectionAttributeInfo TheoryAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new TheoryAttribute { DisplayName = displayName, Skip = skip });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        result.GetNamedArgument<int>("Timeout").Returns(timeout);
        return result;
    }

    public static IReflectionAttributeInfo TraitAttribute<T>()
        where T : Attribute, new()
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new T());
        return result;
    }

    public static IReflectionAttributeInfo TraitAttribute(string name, string value)
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        var traitDiscovererAttributes = new[] { TraitDiscovererAttribute() };
        result.GetCustomAttributes(typeof(TraitDiscovererAttribute)).Returns(traitDiscovererAttributes);
        result.Attribute.Returns(new TraitAttribute(name, value));
        result.GetConstructorArguments().Returns(new object[] { name, value });
        return result;
    }

    public static IAttributeInfo TraitDiscovererAttribute(string typeName = "Xunit.Sdk.TraitDiscoverer", string assemblyName = "xunit.core")
    {
        var result = Substitute.For<IReflectionAttributeInfo, InterfaceProxy<IReflectionAttributeInfo>>();
        result.Attribute.Returns(new TraitDiscovererAttribute(typeName, assemblyName));
        result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
        return result;
    }

    public static ITypeInfo TypeInfo(string typeName = null,
                                     IMethodInfo[] methods = null,
                                     IReflectionAttributeInfo[] attributes = null,
                                     string assemblyFileName = null)
    {
        var result = Substitute.For<ITypeInfo, InterfaceProxy<ITypeInfo>>();
        result.Name.Returns(typeName ?? "type:" + Guid.NewGuid().ToString("n"));
        result.GetMethods(false).ReturnsForAnyArgs(methods ?? new IMethodInfo[0]);
        var assemblyInfo = AssemblyInfo(assemblyFileName: assemblyFileName);
        result.Assembly.Returns(assemblyInfo);
        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
        return result;
    }

    public static XunitTestCase XunitTestCase<TClassUnderTest>(string methodName, ITestCollection collection = null, object[] testMethodArguments = null, IMessageSink diagnosticMessageSink = null)
    {
        var method = TestMethod(typeof(TClassUnderTest), methodName, collection);

        return new XunitTestCase(diagnosticMessageSink ?? new Xunit.NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, method, testMethodArguments);
    }

    public static XunitTheoryTestCase XunitTheoryTestCase<TClassUnderTest>(string methodName, ITestCollection collection = null, IMessageSink diagnosticMessageSink = null)
    {
        var method = TestMethod(typeof(TClassUnderTest), methodName, collection);

        return new XunitTheoryTestCase(diagnosticMessageSink ?? new Xunit.NullMessageSink(), TestMethodDisplay.ClassAndMethod, TestMethodDisplayOptions.None, method);
    }

    // Helpers

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
}

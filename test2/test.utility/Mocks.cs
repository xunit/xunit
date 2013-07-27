using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public static class Mocks
{
    public static IAssemblyInfo AssemblyInfo(ITypeInfo[] types = null, IAttributeInfo[] attributes = null)
    {
        var result = Substitute.For<IAssemblyInfo>();
        result.GetType("").ReturnsForAnyArgs(types == null ? null : types.FirstOrDefault());
        result.GetTypes(true).ReturnsForAnyArgs(types ?? new ITypeInfo[0]);
        result.GetCustomAttributes("").ReturnsForAnyArgs(attributes ?? new IAttributeInfo[0]);
        return result;
    }

    public static IReflectionAttributeInfo CollectionAttribute(string collectionName)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new CollectionAttribute(collectionName));
        result.GetConstructorArguments().Returns(new[] { collectionName });
        return result;
    }

    public static IReflectionAttributeInfo CollectionBehaviorAttribute(CollectionBehavior collectionBehavior)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new CollectionBehaviorAttribute(collectionBehavior));
        result.GetConstructorArguments().Returns(new object[] { collectionBehavior });
        return result;
    }

    public static IReflectionAttributeInfo CollectionBehaviorAttribute(string factoryTypeName, string factoryAssemblyName)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new CollectionBehaviorAttribute(factoryTypeName, factoryAssemblyName));
        result.GetConstructorArguments().Returns(new object[] { factoryTypeName, factoryAssemblyName });
        return result;
    }

    public static IReflectionAttributeInfo FactAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        result.GetNamedArgument<int>("Timeout").Returns(timeout);
        return result;
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

        result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo =>
        {
            var attributeType = GetType((string)callInfo[0]);
            var attributeResults = new List<IAttributeInfo>();

            foreach (var attribute in attributes)
                if (attributeType.IsAssignableFrom(attribute.Attribute.GetType()))
                    attributeResults.Add(attribute);

            return attributeResults;
        });

        result.GetParameters().Returns(parameters);

        return result;
    }

    public static IParameterInfo ParameterInfo(string name)
    {
        var result = Substitute.For<IParameterInfo>();
        result.Name.Returns(name);
        return result;
    }

    public static ITestCase TestCase<TClassUnderTest>(string methodName)
    {
        var typeUnderTest = typeof(TClassUnderTest);
        var methodInfo = typeUnderTest.GetMethod(methodName);
        if (methodInfo == null)
            throw new Exception("Unknown method: " + typeUnderTest.FullName + "." + methodName);

        var result = Substitute.For<ITestCase>();
        result.Class.Returns(Reflector.Wrap(typeUnderTest));
        result.Method.Returns(Reflector.Wrap(methodInfo));
        return result;
    }

    public static IReflectionAttributeInfo TheoryAttribute(string displayName = null, string skip = null, int timeout = 0)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new TheoryAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
        result.GetNamedArgument<string>("DisplayName").Returns(displayName);
        result.GetNamedArgument<string>("Skip").Returns(skip);
        result.GetNamedArgument<int>("Timeout").Returns(timeout);
        return result;
    }

    public static IReflectionAttributeInfo TraitAttribute(string name, string value)
    {
        var result = Substitute.For<IReflectionAttributeInfo>();
        result.Attribute.Returns(new TraitAttribute(name, value));
        result.GetConstructorArguments().Returns(new object[] { name, value });
        return result;
    }

    public static ITypeInfo TypeInfo(string typeName = "MockType", IMethodInfo[] methods = null, IAttributeInfo[] attributes = null)
    {
        var result = Substitute.For<ITypeInfo>();
        result.Name.Returns(typeName);
        result.GetMethods(false).ReturnsForAnyArgs(methods ?? new IMethodInfo[0]);
        result.GetCustomAttributes("").ReturnsForAnyArgs(attributes ?? new IAttributeInfo[0]);
        return result;
    }

    private static Type GetType(string assemblyQualifiedAttributeTypeName)
    {
        var parts = assemblyQualifiedAttributeTypeName.Split(new[] { ',' }, 2).Select(x => x.Trim()).ToList();
        if (parts.Count == 0)
            return null;

        if (parts.Count == 1)
            return System.Type.GetType(parts[0]);

        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == parts[1]);
        if (assembly == null)
            return null;

        return assembly.GetType(parts[0]);
    }
}
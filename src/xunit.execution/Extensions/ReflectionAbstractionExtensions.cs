using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Abstractions;
using Xunit.Sdk;

/// <summary>
/// This class represents utility methods needed to supplement the
/// reflection capabilities provided by the CLR
/// </summary>
public static class ReflectionAbstractionExtensions
{
    /// <summary>
    /// Creates an instance of the test class for the given test case. Sends the <see cref="ITestClassConstructionStarting"/>
    /// and <see cref="ITestClassConstructionFinished"/> messages as appropriate.
    /// </summary>
    /// <param name="test">The test</param>
    /// <param name="testClassType">The type of the test class</param>
    /// <param name="constructorArguments">The constructor arguments for the test class</param>
    /// <param name="messageBus">The message bus used to send the test messages</param>
    /// <param name="timer">The timer used to measure the time taken for construction</param>
    /// <param name="cancellationTokenSource">The cancellation token source</param>
    /// <returns></returns>
    public static object CreateTestClass(this ITest test,
                                         Type testClassType,
                                         object[] constructorArguments,
                                         IMessageBus messageBus,
                                         ExecutionTimer timer,
                                         CancellationTokenSource cancellationTokenSource)
    {
        object testClass = null;

        if (!messageBus.QueueMessage(new TestClassConstructionStarting(test)))
            cancellationTokenSource.Cancel();
        else
        {
            try
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                    timer.Aggregate(() => testClass = Activator.CreateInstance(testClassType, constructorArguments));
            }
            finally
            {
                if (!messageBus.QueueMessage(new TestClassConstructionFinished(test)))
                    cancellationTokenSource.Cancel();
            }
        }

        return testClass;
    }

    /// <summary>
    /// Disposes the test class instance. Sends the <see cref="ITestClassDisposeStarting"/> and <see cref="ITestClassDisposeFinished"/>
    /// messages as appropriate.
    /// </summary>
    /// <param name="test">The test</param>
    /// <param name="testClass">The test class instance to be disposed</param>
    /// <param name="messageBus">The message bus used to send the test messages</param>
    /// <param name="timer">The timer used to measure the time taken for construction</param>
    /// <param name="cancellationTokenSource">The cancellation token source</param>
    public static void DisposeTestClass(this ITest test,
                                        object testClass,
                                        IMessageBus messageBus,
                                        ExecutionTimer timer,
                                        CancellationTokenSource cancellationTokenSource)
    {
        var disposable = testClass as IDisposable;
        if (disposable == null)
            return;

        if (!messageBus.QueueMessage(new TestClassDisposeStarting(test)))
            cancellationTokenSource.Cancel();
        else
        {
            try
            {
                timer.Aggregate(disposable.Dispose);
            }
            finally
            {
                if (!messageBus.QueueMessage(new TestClassDisposeFinished(test)))
                    cancellationTokenSource.Cancel();
            }
        }
    }

    static MethodInfo GetMethodInfoFromIMethodInfo(this Type type, IMethodInfo methodInfo)
    {
        // The old logic only flattened hierarchy for static methods
        var methods = from method in methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetTypeInfo().DeclaredMethods
                      where method.IsPublic == methodInfo.IsPublic &&
                            method.IsStatic == methodInfo.IsStatic &&
                            method.Name == methodInfo.Name
                      select method;

        return methods.FirstOrDefault();
    }

    /// <summary>
    /// Gets methods in the target type that match the protection level of the supplied method
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="methodInfo">The method</param>
    /// <returns>The reflection method informations that match</returns>
    public static IEnumerable<MethodInfo> GetMatchingMethods(this Type type, MethodInfo methodInfo)
    {
        var methods = from method in methodInfo.IsStatic ? type.GetRuntimeMethods() : type.GetTypeInfo().DeclaredMethods
                      where method.IsPublic == methodInfo.IsPublic &&
                            method.IsStatic == methodInfo.IsStatic
                      select method;

        return methods;
    }

    /// <summary>
    /// Gets all the custom attributes for the given assembly.
    /// </summary>
    /// <param name="assemblyInfo">The assembly</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the assembly</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAssemblyInfo assemblyInfo, Type attributeType)
    {
        return assemblyInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given attribute.
    /// </summary>
    /// <param name="attributeInfo">The attribute</param>
    /// <param name="attributeType">The type of the attribute to find</param>
    /// <returns>The matching attributes that decorate the attribute</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IAttributeInfo attributeInfo, Type attributeType)
    {
        return attributeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the method that are of the given type.
    /// </summary>
    /// <param name="methodInfo">The method</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the method</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this IMethodInfo methodInfo, Type attributeType)
    {
        return methodInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Gets all the custom attributes for the given type.
    /// </summary>
    /// <param name="typeInfo">The type</param>
    /// <param name="attributeType">The type of the attribute</param>
    /// <returns>The matching attributes that decorate the type</returns>
    public static IEnumerable<IAttributeInfo> GetCustomAttributes(this ITypeInfo typeInfo, Type attributeType)
    {
        return typeInfo.GetCustomAttributes(attributeType.AssemblyQualifiedName);
    }

    /// <summary>
    /// Converts an <see cref="IMethodInfo"/> into a <see cref="MethodInfo"/>, if possible (for example, this
    /// will not work when the test method is based on source code rather than binaries).
    /// </summary>
    /// <param name="methodInfo">The method to convert</param>
    /// <returns>The runtime method, if available; <c>null</c>, otherwise</returns>
    public static MethodInfo ToRuntimeMethod(this IMethodInfo methodInfo)
    {
        var reflectionMethodInfo = methodInfo as IReflectionMethodInfo;
        if (reflectionMethodInfo != null)
            return reflectionMethodInfo.MethodInfo;

        return methodInfo.Type.ToRuntimeType().GetMethodInfoFromIMethodInfo(methodInfo);
    }

    /// <summary>
    /// Converts an <see cref="ITypeInfo"/> into a <see cref="Type"/>, if possible (for example, this
    /// will not work when the test class is based on source code rather than binaries).
    /// </summary>
    /// <param name="typeInfo">The type to convert</param>
    /// <returns>The runtime type, if available, <c>null</c>, otherwise</returns>
    public static Type ToRuntimeType(this ITypeInfo typeInfo)
    {
        var reflectionTypeInfo = typeInfo as IReflectionTypeInfo;
        if (reflectionTypeInfo != null)
            return reflectionTypeInfo.Type;

        return SerializationHelper.GetType(typeInfo.Assembly.Name, typeInfo.Name);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    internal class DiaSessionHelperForAsyncMethods
    {
        static readonly Func<MethodInfo, Type> GetStateMachineType = InitializeGetStateMachineType();

        readonly IAssemblyInfo assemblyInfo = null;
        readonly Dictionary<string, ITypeInfo> typeNameMap = null;

        public DiaSessionHelperForAsyncMethods(IAssemblyInfo assemblyInfo)
        {
            this.assemblyInfo = assemblyInfo;

            var types = assemblyInfo.GetTypes(true);
            typeNameMap = types.Where(t => t != null && !string.IsNullOrEmpty(t.Name))
                               .ToDictionary(k => k.Name);
        }

        /// <summary>
        /// Normalizes test class and test method names if required and gets Source Information
        /// </summary>
        /// <param name="sourceInfoProvider"></param>
        /// <param name="testCase"></param>        
        public ISourceInformation NormalizeAndGetSourceInformation(ISourceInformationProvider sourceInfoProvider,
                                                                   ITestCase testCase)
        {
            try
            {
                if (assemblyInfo == null)
                    return null;

                var typeName = testCase.TestMethod.TestClass.Class.Name;
                var methodName = testCase.TestMethod.Method.Name;
                var assemblyPath = assemblyInfo.AssemblyPath;

                ITypeInfo type;
                if (typeNameMap.TryGetValue(typeName, out type) && type != null)
                {
                    var method = type.GetMethod(methodName, true);
                    if (method != null)
                    {
                        // DiaSession only ever wants you to ask for the declaring type
                        typeName = method.Type.Name;
                        assemblyPath = method.Type.Assembly.AssemblyPath;

                        // See if this is an async method by looking for [AsyncStateMachine] on the method,
                        // which means we need to pass the state machine's "MoveNext" method.
                        var stateMachineType = GetStateMachineType?.Invoke(method.ToRuntimeMethod());
                        if (stateMachineType != null)
                            return sourceInfoProvider.GetSourceInformation(new XunitTestCase(null, TestMethodDisplay.ClassAndMethod, GetTestMethod(stateMachineType, "MoveNext")));

                        return sourceInfoProvider.GetSourceInformation(testCase);
                    }
                }
            }
            catch { }

            return null;
        }

        TestMethod GetTestMethod(Type type, string methodName)
        {
            var testClass = new TestClass(new TestCollection(new TestAssembly(assemblyInfo), null, assemblyInfo.Name), Reflector.Wrap(type));
            var methodInfo = testClass.Class.GetMethod(methodName, true);

            return (methodInfo != null) ? new TestMethod(testClass, methodInfo) : null;
        }

        private static Func<MethodInfo, Type> InitializeGetStateMachineType()
        {
            var asyncStateMachineAttribute = Type.GetType("System.Runtime.CompilerServices.AsyncStateMachineAttribute");
            if (asyncStateMachineAttribute == null)
                return null;

            var asyncStateMachineAttributeConstructor =
                asyncStateMachineAttribute.GetTypeInfo().DeclaredConstructors
                                                        .FirstOrDefault(c => c.GetParameters().Length == 1
                                                                          && c.GetParameters()[0].ParameterType == typeof(Type));
            if (asyncStateMachineAttributeConstructor == null)
                return null;

            // Types and constants used in the expression construction
            var customAttributeDataType = typeof(CustomAttributeData);
            var methodInfoType = typeof(MethodInfo);
            var memberInfoType = typeof(MemberInfo);
            var objectType = typeof(object);
            var typeType = typeof(Type);
            var enumerableType = typeof(Enumerable);

            // Lambda input parameter: MethodInfo method
            var methodParam = Expression.Parameter(methodInfoType, "method");

            // CustomAttributeData.GetCustomAttributes(method)
            var attributes = Expression.Call(
                customAttributeDataType,
                "GetCustomAttributes",
                new Type[0],
                new Expression[] { Expression.Convert(methodParam, memberInfoType) }
            );

            var attrParameter = Expression.Parameter(customAttributeDataType, "attr");
            var constructorProperty = Expression.Property(attrParameter, "Constructor");
            var asyncMachineAttributeConstructorValue = Expression.Constant(asyncStateMachineAttributeConstructor);
            var constructorEquality = Expression.Equal(constructorProperty, asyncMachineAttributeConstructorValue);
            var attributeSelector = Expression.Lambda<Func<CustomAttributeData, bool>>(constructorEquality, new[] { attrParameter });

            var attribute = Expression.Call(
                enumerableType,
                "SingleOrDefault",
                new Type[] { customAttributeDataType },
                new Expression[] { attributes, attributeSelector }
            );

            // attribute == null
            var attributeNullEquality = Expression.Equal(attribute, Expression.Constant(null, objectType));

            // (Type)attribute.ConstructorArguments[0].Value
            var constructorArguments = Expression.Property(attribute, "ConstructorArguments");
            var firstConstructorArgument = Expression.Call(constructorArguments, "get_Item", new Type[0], Expression.Constant(0));
            var firstConstructorArgumentValue = Expression.Property(firstConstructorArgument, "Value");
            var firstConstructorArgumentValueAsType = Expression.Convert(firstConstructorArgumentValue, typeType);

            var conditional = Expression.Condition(
                attributeNullEquality,
                Expression.Constant(null, typeType),
                firstConstructorArgumentValueAsType
            );

            return Expression.Lambda<Func<MethodInfo, Type>>(conditional, methodParam).Compile();
        }
    }
}
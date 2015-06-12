using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Xunit
{
    internal class DiaSessionWrapperHelper : LongLivedMarshalByRefObject
    {
        static readonly Func<MethodInfo, Type> GetStateMachineType = InitializeGetStateMachineType();

        readonly Assembly assembly;

        readonly Dictionary<string, Type> typeNameMap;

        public DiaSessionWrapperHelper(string assemblyFileName)
        {
            try
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFileName);
                string assemblyDirectory = Path.GetDirectoryName(assemblyFileName);

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) =>
                {
                    try
                    {
                        // First try to load it normally
                        return Assembly.ReflectionOnlyLoad(args.Name);
                    }
                    catch
                    {
                        try
                        {
                            // If a normal implicit load fails, try to load it from the directory that
                            // the test assembly lives in
                            return Assembly.ReflectionOnlyLoadFrom(
                                Path.Combine(
                                    assemblyDirectory,
                                    new AssemblyName(args.Name).Name + ".dll"
                                )
                            );
                        }
                        catch
                        {
                            // If all else fails, say we couldn't find it
                            return null;
                        }
                    }
                };
            }
            catch { }

            
            if (assembly != null)
            {
                Type[] types = null;
                try
                {
                    // Try to get all types early
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // if we get here, we couldn't load all of the types, so get what we can
                    types = ex.Types;
                }
                catch
                {
                } // we don't care about anything else

                if (types != null)
                {
                    typeNameMap = types.ToDictionary(k => k.FullName);
                }
                else
                {
                    typeNameMap = new Dictionary<string, Type>();
                }
            }
        }
        
        static Type GetStateMachineType_NoOp(MethodInfo method)
        {
            return null;
        }

        static Func<MethodInfo, Type> InitializeGetStateMachineType()
        {
            Type asyncStateMachineAttribute = Type.GetType("System.Runtime.CompilerServices.AsyncStateMachineAttribute");
            if (asyncStateMachineAttribute == null)
                return GetStateMachineType_NoOp;

            ConstructorInfo asyncStateMachineAttributeConstructor = asyncStateMachineAttribute.GetConstructor(new[] { typeof(Type) });
            if (asyncStateMachineAttributeConstructor == null)
                return GetStateMachineType_NoOp;

            // Types and constants used in the expression construction
            Type customAttributeDataType = typeof(CustomAttributeData);
            Type methodInfoType = typeof(MethodInfo);
            Type memberInfoType = typeof(MemberInfo);
            Type objectType = typeof(object);
            Type typeType = typeof(Type);
            Type enumerableType = typeof(Enumerable);

            // Lambda input parameter: MethodInfo method
            var methodParam = Expression.Parameter(methodInfoType, "method");

            // CustomAttributeData.GetCustomAttributes(method)
            var attributes = Expression.Call(
                customAttributeDataType,
                "GetCustomAttributes",
                new Type[0],
                new Expression[] { Expression.Convert(methodParam, memberInfoType) }
            );

            // attr => attr.Constructor == asyncStateMachineAttributeConstructor
            var attrParameter = Expression.Parameter(customAttributeDataType, "attr");
            var constructorProperty = Expression.Property(attrParameter, "Constructor");
            var asyncMachineAttributeConstructorValue = Expression.Constant(asyncStateMachineAttributeConstructor);
            var constructorEquality = Expression.Equal(constructorProperty, asyncMachineAttributeConstructorValue);
            var attributeSelector = Expression.Lambda<Func<CustomAttributeData, bool>>(constructorEquality, new[] { attrParameter });

            // var attribute = CustomAttributeData.GetCustomAttributes(method)
            //                                    .SingleOrDefault(attr => attr.Constructor == asyncStateMachineAttributeConstructor);
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

            // if (attribute == null)
            //     return null;
            // else
            //     return (Type)attribute.ConstructorArguments[0].Value;
            var conditional = Expression.Condition(
                attributeNullEquality,
                Expression.Constant(null, typeType),
                firstConstructorArgumentValueAsType
            );

            return Expression.Lambda<Func<MethodInfo, Type>>(conditional, methodParam).Compile();
        }

        public void Normalize(ref string typeName, ref string methodName)
        {
            try
            {
                if (assembly == null)
                    return;

                Type type;
                if(typeNameMap.TryGetValue(typeName, out type) && type != null)
                {
                    MethodInfo method = type.GetMethod(methodName);
                    if (method != null)
                    {
                        // DiaSession only ever wants you to ask for the declaring type
                        typeName = method.DeclaringType.FullName;

                        // See if this is an async method by looking for [AsyncStateMachine] on the method,
                        // which means we need to pass the state machine's "MoveNext" method.
                        Type stateMachineType = GetStateMachineType(method);
                        if (stateMachineType != null)
                        {
                            typeName = stateMachineType.FullName;
                            methodName = "MoveNext";
                        }
                    }
                }
            }
            catch { }
        }
    }
}
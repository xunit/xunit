using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;

namespace Xunit.Sdk
{
    /// <summary>
    /// Wrapper to implement <see cref="IMethodInfo"/> and <see cref="ITypeInfo"/> using reflection.
    /// </summary>
    public static class Reflector
    {
        /// <summary>
        /// Converts an <see cref="Attribute"/> into an <see cref="IAttributeInfo"/> using reflection.
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static IAttributeInfo Wrap(Attribute attribute)
        {
            return new ReflectionAttributeInfo(attribute);
        }

        /// <summary>
        /// Converts a <see cref="MethodInfo"/> into an <see cref="IMethodInfo"/> using reflection.
        /// </summary>
        /// <param name="method">The method to wrap</param>
        /// <returns>The wrapper</returns>
        public static IMethodInfo Wrap(MethodInfo method)
        {
            return new ReflectionMethodInfo(method);
        }

        /// <summary>
        /// Converts a <see cref="Type"/> into an <see cref="ITypeInfo"/> using reflection.
        /// </summary>
        /// <param name="type">The type to wrap</param>
        /// <returns>The wrapper</returns>
        public static ITypeInfo Wrap(Type type)
        {
            return new ReflectionTypeInfo(type);
        }

        class ReflectionAttributeInfo : IAttributeInfo
        {
            readonly Attribute attribute;

            public ReflectionAttributeInfo(Attribute attribute)
            {
                this.attribute = attribute;
            }

            public T GetInstance<T>() where T : Attribute
            {
                return (T)attribute;
            }

            public TValue GetPropertyValue<TValue>(string propertyName)
            {
                PropertyInfo propInfo = attribute.GetType().GetProperty(propertyName);
                if (propInfo == null)
                    throw new ArgumentException("Could not find property " + propertyName + " on instance of " + attribute.GetType().FullName, "propertyName");

                return (TValue)propInfo.GetValue(attribute, new object[0]);
            }

            public override string ToString()
            {
                return attribute.ToString();
            }
        }

        class ReflectionMethodInfo : IMethodInfo
        {
            static Type aggregateExceptionType = Type.GetType("System.AggregateException, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            static PropertyInfo aggregateExceptionInnerExceptions;
            static Type taskType = Type.GetType("System.Threading.Tasks.Task, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            static MethodInfo taskWaitMethod;

            readonly MethodInfo method;

            public ReflectionMethodInfo(MethodInfo method)
            {
                this.method = method;
            }

            public ITypeInfo Class
            {
                get { return new ReflectionTypeInfo(method.ReflectedType); }
            }

            public bool IsAbstract
            {
                get { return method.IsAbstract; }
            }

            public bool IsStatic
            {
                get { return method.IsStatic; }
            }

            public MethodInfo MethodInfo
            {
                get { return method; }
            }

            public string Name
            {
                get { return method.Name; }
            }

            public string ReturnType
            {
                get { return method.ReturnType.FullName; }
            }

            public string TypeName
            {
                get { return method.ReflectedType.FullName; }
            }

            public object CreateInstance()
            {
                return Activator.CreateInstance(method.ReflectedType);
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                foreach (Attribute attribute in method.GetCustomAttributes(attributeType, false))
                    yield return Wrap(attribute);
            }

            public bool HasAttribute(Type attributeType)
            {
                return method.IsDefined(attributeType, false);
            }

            public void Invoke(object testClass, params object[] parameters)
            {
                if (taskType != null && taskWaitMethod == null)
                {
                    taskWaitMethod = taskType.GetMethod("Wait", new Type[0]);
                    aggregateExceptionInnerExceptions = aggregateExceptionType.GetProperty("InnerExceptions");
                }

                try
                {
                    var oldSyncContext = SynchronizationContext.Current;

					using (var asyncSyncContext = new AsyncTestSyncContext(method))
                    {
                        SynchronizationContext.SetSynchronizationContext(asyncSyncContext);

                        try
                        {
                            object result = method.Invoke(testClass, parameters);

                            if (taskType != null && result != null && taskType.IsAssignableFrom(result.GetType()))
                                taskWaitMethod.Invoke(result, null);
                            else
                            {
                                var ex = asyncSyncContext.WaitForCompletion();
                                if (ex != null)
                                    ExceptionUtility.RethrowWithNoStackTraceLoss(ex);
                            }
                        }
                        catch (TargetParameterCountException)
                        {
                            throw new ParameterCountMismatchException();
                        }
                        catch (TargetInvocationException ex)
                        {
                            ExceptionUtility.RethrowWithNoStackTraceLoss(ex.InnerException);
                        }
                        finally
                        {
                            SynchronizationContext.SetSynchronizationContext(oldSyncContext);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (aggregateExceptionType != null && aggregateExceptionType.IsAssignableFrom(ex.GetType()))
                    {
                        ReadOnlyCollection<Exception> exceptions = (ReadOnlyCollection<Exception>)aggregateExceptionInnerExceptions.GetValue(ex, null);
                        ExceptionUtility.RethrowWithNoStackTraceLoss(exceptions[0]);
                    }

                    throw;
                }
            }

            public override bool Equals(object obj)
            {
                ReflectionMethodInfo other = obj as ReflectionMethodInfo;
                if (other == null)
                    return false;

                return other.method == this.method;
            }

            public override int GetHashCode()
            {
                return method.GetHashCode();
            }

            public override string ToString()
            {
                return method.ToString();
            }
        }

        class ReflectionTypeInfo : ITypeInfo
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            readonly Type type;

            public ReflectionTypeInfo(Type type)
            {
                this.type = type;
            }

            public bool IsAbstract
            {
                get { return type.IsAbstract; }
            }

            public bool IsSealed
            {
                get { return type.IsSealed; }
            }

            public Type Type
            {
                get { return type; }
            }

            public IEnumerable<IAttributeInfo> GetCustomAttributes(Type attributeType)
            {
                foreach (Attribute attribute in type.GetCustomAttributes(attributeType, true))
                    yield return Wrap(attribute);
            }

            public IMethodInfo GetMethod(string methodName)
            {
                MethodInfo method = type.GetMethod(methodName, bindingFlags);
                if (method == null)
                    return null;

                return Wrap(method);
            }

            public IEnumerable<IMethodInfo> GetMethods()
            {
                foreach (MethodInfo method in type.GetMethods(bindingFlags))
                    yield return Wrap(method);
            }

            public bool HasAttribute(Type attributeType)
            {
                return type.IsDefined(attributeType, true);
            }

            public bool HasInterface(Type interfaceType)
            {
                foreach (Type implementedInterfaceType in type.GetInterfaces())
                    if (implementedInterfaceType == interfaceType)
                        return true;

                return false;
            }

            public override string ToString()
            {
                return type.ToString();
            }
        }
    }
}
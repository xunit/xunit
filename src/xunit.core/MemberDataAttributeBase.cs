using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Provides a base class for attributes that will provide member data. The member data must return
    /// something compatible with <see cref="IEnumerable"/>.
    /// Caution: the property is completely enumerated by .ToList() before any test is run. Hence it should return independent object sets.
    /// </summary>
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public abstract class MemberDataAttributeBase : DataAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDataAttributeBase"/> class.
        /// </summary>
        /// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
        /// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
        protected MemberDataAttributeBase(string memberName, object[] parameters)
        {
            MemberName = memberName;
            Parameters = parameters;
        }

        /// <summary>
        /// Returns <c>true</c> if the data attribute wants to skip enumerating data during discovery.
        /// This will cause the theory to yield a single test case for all data, and the data discovery
        /// will be during test execution instead of discovery.
        /// </summary>
        public bool DisableDiscoveryEnumeration { get; set; }

        /// <summary>
        /// Gets the member name.
        /// </summary>
        public string MemberName { get; private set; }

        /// <summary>
        /// Gets or sets the type to retrieve the member from. If not set, then the property will be
        /// retrieved from the unit test class.
        /// </summary>
        public Type MemberType { get; set; }

        /// <summary>
        /// Gets or sets the parameters passed to the member. Only supported for static methods.
        /// </summary>
        public object[] Parameters { get; private set; }

        /// <inheritdoc/>
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            Guard.ArgumentNotNull("testMethod", testMethod);

            var type = MemberType ?? testMethod.DeclaringType;
            var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
            if (accessor == null)
            {
                var parameterText = Parameters?.Length > 0 ? $" with parameter types: {string.Join(", ", Parameters.Select(p => p?.GetType().FullName ?? "(null)"))}" : "";
                throw new ArgumentException($"Could not find public static member (property, field, or method) named '{MemberName}' on {type.FullName}{parameterText}");
            }

            var obj = accessor();
            if (obj == null)
                return null;

            var dataItems = obj as IEnumerable;
            if (dataItems == null)
                throw new ArgumentException($"Property {MemberName} on {type.FullName} did not return IEnumerable");

            return dataItems.Cast<object>().Select(item => ConvertDataItem(testMethod, item));
        }

        /// <summary>
        /// Converts an item yielded by the data member to an object array, for return from <see cref="GetData"/>.
        /// </summary>
        /// <param name="testMethod">The method that is being tested.</param>
        /// <param name="item">An item yielded from the data member.</param>
        /// <returns>An <see cref="T:object[]"/> suitable for return from <see cref="GetData"/>.</returns>
        protected abstract object[] ConvertDataItem(MethodInfo testMethod, object item);

        Func<object> GetFieldAccessor(Type type)
        {
            FieldInfo fieldInfo = null;
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
            {
                fieldInfo = reflectionType.GetRuntimeField(MemberName);
                if (fieldInfo != null)
                    break;
            }

            if (fieldInfo == null || !fieldInfo.IsStatic)
                return null;

            return () => fieldInfo.GetValue(null);
        }

        Func<object> GetMethodAccessor(Type type)
        {
            MethodInfo methodInfo = null;
            var parameterTypes = Parameters == null ? new Type[0] : Parameters.Select(p => p?.GetType()).ToArray();
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
            {
                methodInfo = reflectionType.GetRuntimeMethods()
                                           .FirstOrDefault(m => m.Name == MemberName && ParameterTypesCompatible(m.GetParameters(), parameterTypes));
                if (methodInfo != null)
                    break;
            }

            if (methodInfo == null || !methodInfo.IsStatic)
                return null;

            return () => methodInfo.Invoke(null, Parameters);
        }

        Func<object> GetPropertyAccessor(Type type)
        {
            PropertyInfo propInfo = null;
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
            {
                propInfo = reflectionType.GetRuntimeProperty(MemberName);
                if (propInfo != null)
                    break;
            }

            if (propInfo == null || propInfo.GetMethod == null || !propInfo.GetMethod.IsStatic)
                return null;

            return () => propInfo.GetValue(null, null);
        }

        static bool ParameterTypesCompatible(ParameterInfo[] parameters, Type[] parameterTypes)
        {
            if (parameters?.Length != parameterTypes.Length)
                return false;

            for (int idx = 0; idx < parameters.Length; ++idx)
                if (parameterTypes[idx] != null && !parameters[idx].ParameterType.GetTypeInfo().IsAssignableFrom(parameterTypes[idx].GetTypeInfo()))
                    return false;

            return true;
        }
    }
}

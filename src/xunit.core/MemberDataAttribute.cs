using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Provides a data source for a data theory, with the data coming from one of the following sources:
    /// 1. A static property
    /// 2. A static field
    /// 3. A static method (with parameters)
    /// The member must return something compatible with IEnumerable&lt;object[]&gt; with the test data.
    /// </summary>
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class MemberDataAttribute : DataAttribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="MemberDataAttribute"/>.
        /// </summary>
        /// <param name="memberName">The name of the public static member on the test class that will provide the test data</param>
        /// <param name="parameters">The parameters for the member (only supported for methods; ignored for everything else)</param>
        public MemberDataAttribute(string memberName, params object[] parameters)
        {
            MemberName = memberName;
            Parameters = parameters;
        }

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
            Guard.ArgumentNotNull("methodUnderTest", testMethod);

            var type = MemberType ?? testMethod.DeclaringType;
            var accessor = GetPropertyAccessor(type) ?? GetFieldAccessor(type) ?? GetMethodAccessor(type);
            if (accessor == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Could not find public static member (property, field, or method) named '{0}' on {1}", MemberName, type.FullName));

            object obj = accessor();
            if (obj == null)
                return null;

            var dataItems = obj as IEnumerable<object[]>;
            if (dataItems == null)
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, "Property {0} on {1} did not return IEnumerable<object[]>", MemberName, type.FullName));

            return dataItems;
        }

        private Func<IEnumerable<object[]>> GetFieldAccessor(Type type)
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

            return () => (IEnumerable<object[]>)fieldInfo.GetValue(null);
        }

        private Func<IEnumerable<object[]>> GetMethodAccessor(Type type)
        {
            MethodInfo methodInfo = null;
            var parameterTypes = Parameters == null ? new Type[0] : Parameters.Select(ToParameterType).ToArray();
            for (var reflectionType = type; reflectionType != null; reflectionType = reflectionType.GetTypeInfo().BaseType)
            {
                methodInfo = reflectionType.GetRuntimeMethod(MemberName, parameterTypes);
                if (methodInfo != null)
                    break;
            }

            if (methodInfo == null || !methodInfo.IsStatic)
                return null;

            return () => (IEnumerable<object[]>)methodInfo.Invoke(null, Parameters);
        }

        private Func<IEnumerable<object[]>> GetPropertyAccessor(Type type)
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

            return () => (IEnumerable<object[]>)propInfo.GetValue(null, null);
        }

        private Type ToParameterType(object value)
        {
            return value == null ? typeof(object) : value.GetType();
        }
    }
}
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Utility class which inspects types for test information
    /// </summary>
    public static class TypeUtility
    {
        /// <summary>
        /// Determines if a type contains any test methods
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>True if the class contains any test methods; false, otherwise</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "method", Justification = "No can do.")]
        public static bool ContainsTestMethods(ITypeInfo type)
        {
#pragma warning disable 168
            foreach (IMethodInfo method in GetTestMethods(type))
                return true;
#pragma warning restore 168

            return false;
        }

        /// <summary>
        /// Retrieves the type to run the test class with from the <see cref="RunWithAttribute"/>, if present.
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>The type of the test class runner, if present; null, otherwise</returns>
        public static ITypeInfo GetRunWith(ITypeInfo type)
        {
            Guard.ArgumentNotNull("type", type);

            foreach (IAttributeInfo attributeInfo in type.GetCustomAttributes(typeof(RunWithAttribute)))
            {
                RunWithAttribute attribute = attributeInfo.GetInstance<RunWithAttribute>();
                if (attribute == null || attribute.TestClassCommand == null)
                    continue;

                ITypeInfo typeInfo = Reflector.Wrap(attribute.TestClassCommand);
                if (ImplementsITestClassCommand(typeInfo))
                    return typeInfo;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a list of the test methods from the test class.
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>The test methods</returns>
        public static IEnumerable<IMethodInfo> GetTestMethods(ITypeInfo type)
        {
            foreach (IMethodInfo method in type.GetMethods())
                if (MethodUtility.IsTest(method))
                    yield return method;
        }

        /// <summary>
        /// Determines if the test class has a <see cref="RunWithAttribute"/> applied to it.
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>True if the test class has a run with attribute; false, otherwise</returns>
        public static bool HasRunWith(ITypeInfo type)
        {
            Guard.ArgumentNotNull("type", type);

            return type.HasAttribute(typeof(RunWithAttribute));
        }

        /// <summary>
        /// Determines if the type implements <see cref="ITestClassCommand"/>.
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>True if the type implements <see cref="ITestClassCommand"/>; false, otherwise</returns>
        public static bool ImplementsITestClassCommand(ITypeInfo type)
        {
            Guard.ArgumentNotNull("type", type);

            return (type.HasInterface(typeof(ITestClassCommand)));
        }

        /// <summary>
        /// Determines whether the specified type is abstract.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is abstract; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAbstract(ITypeInfo type)
        {
            Guard.ArgumentNotNull("type", type);

            return type.IsAbstract;
        }

        /// <summary>
        /// Determines whether the specified type is static.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// 	<c>true</c> if the specified type is static; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStatic(ITypeInfo type)
        {
            Guard.ArgumentNotNull("type", type);

            return type.IsAbstract && type.IsSealed;
        }

        /// <summary>
        /// Determines if a class is a test class.
        /// </summary>
        /// <param name="type">The type to be inspected</param>
        /// <returns>True if the type is a test class; false, otherwise</returns>
        public static bool IsTestClass(ITypeInfo type)
        {
            return (IsStatic(type) || !IsAbstract(type)) && (HasRunWith(type) || ContainsTestMethods(type));
        }
    }
}
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Sdk
{
    /// <summary>
    /// Utility class which inspects methods for test information
    /// </summary>
    public static class MethodUtility
    {
        /// <summary>
        /// Gets the display name.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>The display name</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static string GetDisplayName(IMethodInfo method)
        {
            foreach (IAttributeInfo attribute in method.GetCustomAttributes(typeof(FactAttribute)))
                return attribute.GetPropertyValue<string>("Name");

            return null;
        }

        /// <summary>
        /// Gets the skip reason from a test method.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>The skip reason</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static string GetSkipReason(IMethodInfo method)
        {
            foreach (IAttributeInfo attribute in method.GetCustomAttributes(typeof(FactAttribute)))
                return attribute.GetPropertyValue<string>("Skip");

            return null;
        }

        /// <summary>
        /// Gets the test commands for a test method.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>The <see cref="ITestCommand"/> objects for the test method</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static IEnumerable<ITestCommand> GetTestCommands(IMethodInfo method)
        {
            foreach (IAttributeInfo attribute in method.GetCustomAttributes(typeof(FactAttribute)))
                foreach (ITestCommand command in attribute.GetInstance<FactAttribute>().CreateTestCommands(method))
                    yield return command;
        }

        /// <summary>
        /// Gets the timeout value for a test method.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>The timeout, in milliseconds</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static int GetTimeoutParameter(IMethodInfo method)
        {
            foreach (IAttributeInfo attribute in method.GetCustomAttributes(typeof(FactAttribute)))
                return attribute.GetPropertyValue<int>("Timeout");

            return -1;
        }

        /// <summary>
        /// Gets the traits on a test method.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>A dictionary of the traits</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static MultiValueDictionary<string, string> GetTraits(IMethodInfo method)
        {
            var traits = new MultiValueDictionary<string, string>();

            foreach (IAttributeInfo attribute in method.GetCustomAttributes(typeof(TraitAttribute)))
                traits.AddValue(attribute.GetPropertyValue<string>("Name"),
                                attribute.GetPropertyValue<string>("Value"));

            foreach (IAttributeInfo attribute in method.Class.GetCustomAttributes(typeof(TraitAttribute)))
                traits.AddValue(attribute.GetPropertyValue<string>("Name"),
                                attribute.GetPropertyValue<string>("Value"));

            return traits;
        }

        /// <summary>
        /// Determines whether a test method has a timeout.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>True if the method has a timeout; false, otherwise</returns>
        public static bool HasTimeout(IMethodInfo method)
        {
            if (!IsTest(method))
                return false;

            return (GetTimeoutParameter(method) != 0);
        }

        /// <summary>
        /// Determines whether a test method has traits.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>True if the method has traits; false, otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static bool HasTraits(IMethodInfo method)
        {
            return method.HasAttribute(typeof(TraitAttribute))
                || method.Class.HasAttribute(typeof(TraitAttribute));
        }

        /// <summary>
        /// Determines whether a test method should be skipped.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>True if the method should be skipped; false, otherwise</returns>
        public static bool IsSkip(IMethodInfo method)
        {
            if (!IsTest(method))
                return false;

            return GetSkipReason(method) != null;
        }

        /// <summary>
        /// Determines whether a method is a test method. A test method must be decorated
        /// with the <see cref="FactAttribute"/> (or derived class) and must not be abstract.
        /// </summary>
        /// <param name="method">The method to be inspected</param>
        /// <returns>True if the method is a test method; false, otherwise</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public static bool IsTest(IMethodInfo method)
        {
            return !method.IsAbstract &&
                    method.HasAttribute(typeof(FactAttribute));
        }
    }
}
using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// Factory for <see cref="ITestClassCommand"/> objects, based on the type under test.
    /// </summary>
    public static class TestClassCommandFactory
    {
        /// <summary>
        /// Creates the test class command, which implements <see cref="ITestClassCommand"/>, for a given type.
        /// </summary>
        /// <param name="type">The type under test</param>
        /// <returns>The test class command, if the class is a test class; null, otherwise</returns>
        public static ITestClassCommand Make(Type type)
        {
            return Make(Reflector.Wrap(type));
        }

        /// <summary>
        /// Creates the test class command, which implements <see cref="ITestClassCommand"/>, for a given type.
        /// </summary>
        /// <param name="typeInfo">The type under test</param>
        /// <returns>The test class command, if the class is a test class; null, otherwise</returns>
        public static ITestClassCommand Make(ITypeInfo typeInfo)
        {
            if (!TypeUtility.IsTestClass(typeInfo))
                return null;

            ITestClassCommand command = null;

            if (TypeUtility.HasRunWith(typeInfo))
            {
                ITypeInfo runWithTypeInfo = TypeUtility.GetRunWith(typeInfo);
                if (runWithTypeInfo == null)
                    return null;

                Type runWithCommandType = runWithTypeInfo.Type;
                if (runWithCommandType != null)
                    command = (ITestClassCommand)Activator.CreateInstance(runWithCommandType);
            }
            else if (TypeUtility.ContainsTestMethods(typeInfo))
                command = new TestClassCommand();

            if (command != null)
                command.TypeUnderTest = typeInfo;

            return command;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an xUnit.net test class
    /// </summary>
    public class TestClassCommand : ITestClassCommand
    {
        readonly Dictionary<MethodInfo, object> fixtures = new Dictionary<MethodInfo, object>();
        Random randomizer = new Random();
        ITypeInfo typeUnderTest;

        /// <summary>
        /// Creates a new instance of the <see cref="TestClassCommand"/> class.
        /// </summary>
        public TestClassCommand()
            : this((ITypeInfo)null) { }

        /// <summary>
        /// Creates a new instance of the <see cref="TestClassCommand"/> class.
        /// </summary>
        /// <param name="typeUnderTest">The type under test</param>
        public TestClassCommand(Type typeUnderTest)
            : this(Reflector.Wrap(typeUnderTest)) { }

        /// <summary>
        /// Creates a new instance of the <see cref="TestClassCommand"/> class.
        /// </summary>
        /// <param name="typeUnderTest">The type under test</param>
        public TestClassCommand(ITypeInfo typeUnderTest)
        {
            this.typeUnderTest = typeUnderTest;
        }

        /// <summary>
        /// Gets the object instance that is under test. May return null if you wish
        /// the test framework to create a new object instance for each test method.
        /// </summary>
        public object ObjectUnderTest
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the randomizer used to determine the order in which tests are run.
        /// </summary>
        public Random Randomizer
        {
            get { return randomizer; }
            set { randomizer = value; }
        }

        /// <summary>
        /// Sets the type that is being tested
        /// </summary>
        public ITypeInfo TypeUnderTest
        {
            get { return typeUnderTest; }
            set { typeUnderTest = value; }
        }

        /// <summary>
        /// Chooses the next test to run, randomly, using the <see cref="Randomizer"/>.
        /// </summary>
        /// <param name="testsLeftToRun">The tests remaining to be run</param>
        /// <returns>The index of the test that should be run</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            return randomizer.Next(testsLeftToRun.Count);
        }

        /// <summary>
        /// Execute actions to be run after all the test methods of this test class are run.
        /// </summary>
        /// <returns>Returns the <see cref="Exception"/> thrown during execution, if any; null, otherwise</returns>
        public Exception ClassFinish()
        {
            foreach (object fixtureData in fixtures.Values)
                try
                {
                    IDisposable disposable = fixtureData as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
                catch (Exception ex)
                {
                    return ex;
                }

            return null;
        }

        /// <summary>
        /// Execute actions to be run before any of the test methods of this test class are run.
        /// </summary>
        /// <returns>Returns the <see cref="Exception"/> thrown during execution, if any; null, otherwise</returns>
        public Exception ClassStart()
        {
            try
            {
                foreach (Type @interface in typeUnderTest.Type.GetInterfaces())
                {
                    if (@interface.IsGenericType)
                    {
                        Type genericDefinition = @interface.GetGenericTypeDefinition();

                        if (genericDefinition == typeof(IUseFixture<>))
                        {
                            Type dataType = @interface.GetGenericArguments()[0];
                            if (dataType == typeUnderTest.Type)
                                throw new InvalidOperationException("Cannot use a test class as its own fixture data");

                            object fixtureData = null;

                            try
                            {
                                fixtureData = Activator.CreateInstance(dataType);
                            }
                            catch (TargetInvocationException ex)
                            {
                                return ex.InnerException;
                            }

                            MethodInfo method = @interface.GetMethod("SetFixture", new Type[] { dataType });
                            fixtures[method] = fixtureData;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Enumerates the test commands for a given test method in this test class.
        /// </summary>
        /// <param name="testMethod">The method under test</param>
        /// <returns>The test commands for the given test method</returns>
        public virtual IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            string skipReason = MethodUtility.GetSkipReason(testMethod);

            if (skipReason != null)
                yield return new SkipCommand(testMethod, MethodUtility.GetDisplayName(testMethod), skipReason);
            else
                foreach (ITestCommand command in MethodUtility.GetTestCommands(testMethod))
                    yield return new FixtureCommand(command, fixtures);
        }

        /// <summary>
        /// Enumerates the methods which are test methods in this test class.
        /// </summary>
        /// <returns>The test methods</returns>
        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            return TypeUtility.GetTestMethods(typeUnderTest);
        }

        /// <summary>
        /// Determines if a given <see cref="IMethodInfo"/> refers to a test method.
        /// </summary>
        /// <param name="testMethod">The test method to validate</param>
        /// <returns>True if the method is a test method; false, otherwise</returns>
        public bool IsTestMethod(IMethodInfo testMethod)
        {
            return MethodUtility.IsTest(testMethod);
        }
    }
}
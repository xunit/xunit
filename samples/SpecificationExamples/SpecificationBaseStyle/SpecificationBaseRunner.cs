using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace SpecificationBaseStyle
{
    class SpecificationBaseRunner : ITestClassCommand
    {
        SpecificationBase objectUnderTest;

        public SpecificationBase ObjectUnderTest
        {
            get
            {
                if (objectUnderTest == null)
                {
                    GuardTypeUnderTest();
                    objectUnderTest = (SpecificationBase)Activator.CreateInstance(TypeUnderTest.Type);
                }

                return objectUnderTest;
            }
        }

        object ITestClassCommand.ObjectUnderTest
        {
            get { return ObjectUnderTest; }
        }

        public ITypeInfo TypeUnderTest { get; set; }

        public int ChooseNextTest(ICollection<IMethodInfo> testsLeftToRun)
        {
            return 0;
        }

        public Exception ClassFinish()
        {
            try
            {
                ObjectUnderTest.OnFinish();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public Exception ClassStart()
        {
            try
            {
                ObjectUnderTest.OnStart();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        public IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo testMethod)
        {
            string displayName = (TypeUnderTest.Type.Name + ", it " + testMethod.Name).Replace('_', ' ');
            return new[] { new SpecTestCommand(testMethod, displayName) };
        }

        public IEnumerable<IMethodInfo> EnumerateTestMethods()
        {
            GuardTypeUnderTest();

            return TypeUnderTest.GetMethods().Where(IsTestMethod);
        }

        void GuardTypeUnderTest()
        {
            if (TypeUnderTest == null)
                throw new InvalidOperationException("Forgot to set TypeUnderTest before calling ObjectUnderTest");

            if (!typeof(SpecificationBase).IsAssignableFrom(TypeUnderTest.Type))
                throw new InvalidOperationException("SpecificationBaseRunner can only be used with types that derive from SpecificationBase");
        }

        public bool IsTestMethod(IMethodInfo testMethod)
        {
            // The ObservationAttribute is really not even required; we could just say any void-returning public
            // method is a test method (or you could say it has to start with "should").

            return testMethod.HasAttribute(typeof(ObservationAttribute));
        }

        class SpecTestCommand : TestCommand
        {
            public SpecTestCommand(IMethodInfo testMethod, string displayName)
                : base(testMethod, displayName, 0) { }

            public override MethodResult Execute(object testClass)
            {
                try
                {
                    testMethod.Invoke(testClass, null);
                }
                catch (ParameterCountMismatchException)
                {
                    throw new InvalidOperationException("Observation " + TypeName + "." + MethodName + " cannot have parameters");
                }

                return new PassedResult(testMethod, DisplayName);
            }
        }
    }
}
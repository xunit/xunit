using System;
using System.Reflection;

namespace Xunit
{
    internal class DiaSession : IDisposable
    {
        static readonly MethodInfo methodGetNavigationData;
        static readonly PropertyInfo propertyFileName;
        static readonly PropertyInfo propertyMinLineNumber;
        static readonly Type typeDiaSession;
        static readonly Type typeDiaNavigationData;

        readonly string assemblyFileName;
        bool sessionHasErrors;
        IDisposable wrappedSession;

        static DiaSession()
        {
            typeDiaSession = Type.GetType("Microsoft.VisualStudio.TestPlatform.ObjectModel.DiaSession, Microsoft.VisualStudio.TestPlatform.ObjectModel", throwOnError: false);
            typeDiaNavigationData = Type.GetType("Microsoft.VisualStudio.TestPlatform.ObjectModel.DiaNavigationData, Microsoft.VisualStudio.TestPlatform.ObjectModel", throwOnError: false);

            if (typeDiaSession != null && typeDiaNavigationData != null)
            {
                methodGetNavigationData = typeDiaSession.GetMethod("GetNavigationData", new[] { typeof(string), typeof(string) });
                propertyFileName = typeDiaNavigationData.GetProperty("FileName");
                propertyMinLineNumber = typeDiaNavigationData.GetProperty("MinLineNumber");
            }
        }

        public DiaSession(string assemblyFileName)
        {
            this.assemblyFileName = assemblyFileName;

            if (typeDiaSession == null || Environment.GetEnvironmentVariable("XUNIT_SKIP_DIA") != null)
                sessionHasErrors = true;
        }

        public void Dispose()
        {
            if (wrappedSession != null)
                wrappedSession.Dispose();
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName)
        {
            if (!sessionHasErrors)
                try
                {
                    if (wrappedSession == null)
                        wrappedSession = (IDisposable)Activator.CreateInstance(typeDiaSession, assemblyFileName);

                    var data = methodGetNavigationData.Invoke(wrappedSession, new[] { typeName, methodName });
                    if (data == null)
                        return null;

                    var noIndex = new object[0];
                    return new DiaNavigationData
                    {
                        FileName = (string)propertyFileName.GetValue(data, noIndex),
                        LineNumber = (int)propertyMinLineNumber.GetValue(data, noIndex)
                    };
                }
                catch
                {
                    sessionHasErrors = true;
                }

            return null;
        }
    }
}

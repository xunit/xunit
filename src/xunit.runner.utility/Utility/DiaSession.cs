#if !NETSTANDARD1_1

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Xunit
{
    class DiaSession : IDisposable
    {
        static readonly MethodInfo methodGetNavigationData;
        static readonly PropertyInfo propertyFileName;
        static readonly PropertyInfo propertyMinLineNumber;
        static readonly Type typeDiaSession;
        static readonly Type typeDiaNavigationData;

        public readonly string AssemblyFileName;
        bool sessionHasErrors;
        readonly Dictionary<string, IDisposable> wrappedSessions;

        static DiaSession()
        {
            typeDiaSession = Type.GetType("Microsoft.VisualStudio.TestPlatform.ObjectModel.DiaSession, Microsoft.VisualStudio.TestPlatform.ObjectModel", false);
            typeDiaNavigationData = Type.GetType("Microsoft.VisualStudio.TestPlatform.ObjectModel.DiaNavigationData, Microsoft.VisualStudio.TestPlatform.ObjectModel", false);

            if (typeDiaSession != null && typeDiaNavigationData != null)
            {
                methodGetNavigationData = typeDiaSession.GetMethod("GetNavigationData", new[] { typeof(string), typeof(string) });
                propertyFileName = typeDiaNavigationData.GetProperty("FileName");
                propertyMinLineNumber = typeDiaNavigationData.GetProperty("MinLineNumber");
            }
        }

        public DiaSession(string assemblyFileName)
        {
            this.AssemblyFileName = assemblyFileName;
            sessionHasErrors |= (typeDiaSession == null || Environment.GetEnvironmentVariable("XUNIT_SKIP_DIA") != null);
            wrappedSessions = new Dictionary<string, IDisposable>();
        }

        public void Dispose()
        {
            foreach (var wrappedSession in wrappedSessions.Values)
                wrappedSession.Dispose();
        }

        public DiaNavigationData GetNavigationData(string typeName, string methodName, string owningAssemblyFilename)
        {
            if (!sessionHasErrors)
            {
                try
                {
                    // strip of any generic instantiation information
                    var idx = typeName.IndexOf('[');
                    if (idx >= 0)
                        typeName = typeName.Substring(0, idx);

                    if (!wrappedSessions.ContainsKey(owningAssemblyFilename))
                    {
#if WINDOWS_UAP
                        // Use overload with search path since pdb isn't next to the exe
                        wrappedSessions[owningAssemblyFilename] = (IDisposable)Activator.CreateInstance(typeDiaSession, owningAssemblyFilename,
                            Windows.ApplicationModel.Package.Current.InstalledLocation.Path);
#else
                        wrappedSessions[owningAssemblyFilename] = (IDisposable)Activator.CreateInstance(typeDiaSession, owningAssemblyFilename);
#endif
                    }

                    var data = methodGetNavigationData.Invoke(wrappedSessions[owningAssemblyFilename], new[] { typeName, methodName });
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

            }

            return null;
        }
    }
}

#endif

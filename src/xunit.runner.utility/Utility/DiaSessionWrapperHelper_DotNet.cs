#if NETSTANDARD1_5 || NETCOREAPP1_0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xunit
{
    class DiaSessionWrapperHelper : LongLivedMarshalByRefObject
    {
        readonly Assembly assembly;
        readonly Dictionary<string, Type> typeNameMap;

        public DiaSessionWrapperHelper(string assemblyFileName)
        {
            try
            {
                assembly = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) });
            }
            catch { }

            if (assembly != null)
            {
                Type[] types = null;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch { }  // Ignore anything other than ReflectionTypeLoadException

                if (types != null)
                    typeNameMap = types.Where(t => t != null && !string.IsNullOrEmpty(t.FullName))
                                       .ToDictionary(k => k.FullName);
                else
                    typeNameMap = new Dictionary<string, Type>();
            }
        }
      

        public void Normalize(ref string typeName, ref string methodName, ref string assemblyPath)
        {
            try
            {
                if (assembly == null)
                    return;

                Type type;
                if (typeNameMap.TryGetValue(typeName, out type) && type != null)
                {
                    MethodInfo method = type.GetMethod(methodName);
                    if (method != null)
                    {
                        // DiaSession only ever wants you to ask for the declaring type
                        typeName = method.DeclaringType.FullName;
                        assemblyPath = method.DeclaringType.GetAssembly().Location;

                        var stateMachineType = method.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
                        
                        if (stateMachineType != null)
                        {
                            typeName = stateMachineType.FullName;
                            methodName = "MoveNext";
                        }
                    }
                }
            }
            catch { }
        }
    }
}

#endif
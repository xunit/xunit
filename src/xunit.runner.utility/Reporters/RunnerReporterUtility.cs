#if NETFRAMEWORK || NETCOREAPP || NETSTANDARD1_5_OR_GREATER

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xunit
{
    /// <summary>
    /// A utility class for finding runner reporters.
    /// </summary>
    public static class RunnerReporterUtility
    {
        /// <summary>
        /// Gets a list of runner reporters from DLLs in the given folder. The only DLLs that are searched are those
        /// named "*reporters*.dll"
        /// </summary>
        /// <param name="folder">The folder to search for reporters in</param>
        /// <param name="messages">Messages that were generated during discovery</param>
        /// <returns>List of available reporters</returns>
        public static List<IRunnerReporter> GetAvailableRunnerReporters(string folder, out List<string> messages)
        {
            var result = new List<IRunnerReporter>();
            messages = new List<string>();
            string[] dllFiles;

            try
            {
                dllFiles = Directory.GetFiles(folder, "*reporters*.dll").Select(f => Path.Combine(folder, f)).ToArray();
            }
            catch (Exception ex)
            {
                messages.Add(string.Format(CultureInfo.CurrentCulture, "Exception thrown looking for reporters in folder '{0}':{1}{2}", folder, Environment.NewLine, ex));
                return result;
            }

            foreach (var dllFile in dllFiles)
            {
                Type[] types = new Type[0];

                try
                {
#if NETFRAMEWORK
                    var assembly = Assembly.LoadFile(dllFile);
#else
                    var assembly = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(dllFile)));
#endif
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
#if NETFRAMEWORK
                    if (type == null || type.IsAbstract || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
#else
                    if (type == null || type.GetTypeInfo().IsAbstract || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
#endif
                        continue;

                    try
                    {
                        var ctor = type.GetConstructor(new Type[0]);
                        if (ctor == null)
                        {
                            messages.Add(string.Format(CultureInfo.CurrentCulture, "Type '{0}' in assembly '{1}' appears to be a runner reporter, but does not have an empty constructor.", type.FullName ?? type.Name, dllFile));
                            continue;
                        }

                        result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
                    }
                    catch (Exception ex)
                    {
                        messages.Add(string.Format(CultureInfo.CurrentCulture, "Exception thrown while inspecting type '{0}' in assembly '{1}':{2}{3}", type.FullName ?? type.Name, dllFile, Environment.NewLine, ex));
                    }
                }
            }

            return result;
        }
    }
}

#endif

using System;

namespace Xunit.Runner.v3
{
	/// <summary />
	public class _TestAssemblyMessage : _MessageSinkMessage
	{
		string? assemblyName;

		/// <summary>
		/// Gets the assembly name. May return a fully qualified name for assemblies found via
		/// reflection (i.e., "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
		/// or may return just assembly name only for assemblies found via source code introspection
		/// (i.e., "mscorlib").
		/// </summary>
		public string AssemblyName
		{
			get => assemblyName ?? throw new InvalidOperationException($"Attempted to get {nameof(AssemblyName)} on an uninitialized '{GetType().FullName}' object");
			set => assemblyName = Guard.ArgumentNotNullOrEmpty(nameof(AssemblyName), value);
		}

		/// <summary>
		/// Gets the on-disk location of the assembly under test. If the assembly path is not
		/// known (for example, in AST-based runners), you must return <c>null</c>.
		/// </summary>
		public string? AssemblyPath { get; set; }

		/// <summary>
		/// Gets the full path of the configuration file name, if one is present.
		/// May be <c>null</c> if there is no configuration file.
		/// </summary>
		public string? ConfigFilePath { get; set; }
	}
}

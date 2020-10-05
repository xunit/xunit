using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that the discovery process is starting for
	/// the requested assembly.
	/// </summary>
	public class _DiscoveryStarting : _TestAssemblyMessage, _IAssemblyMetadata
	{
		string? assemblyName;

		/// <inheritdoc/>
		public string AssemblyName
		{
			get => assemblyName ?? throw new InvalidOperationException($"Attempted to get {nameof(AssemblyName)} on an uninitialized '{GetType().FullName}' object");
			set => assemblyName = Guard.ArgumentNotNullOrEmpty(nameof(AssemblyName), value);
		}

		/// <inheritdoc/>
		public string? AssemblyPath { get; set; }

		/// <inheritdoc/>
		public string? ConfigFilePath { get; set; }
	}
}

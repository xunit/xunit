using System.Collections;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents a project which contains zero or more test assemblies, as well as global
	/// (cross-assembly) configuration settings.
	/// </summary>
	public class XunitProject : IEnumerable<XunitProjectAssembly>
	{
		readonly List<XunitProjectAssembly> assemblies;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitProject"/> class.
		/// </summary>
		public XunitProject()
		{
			assemblies = new List<XunitProjectAssembly>();
		}

		/// <summary>
		/// Gets the assemblies that are in the project.
		/// </summary>
		public ICollection<XunitProjectAssembly> Assemblies => assemblies;

		/// <summary>
		/// Gets the configuration values for the test project.
		/// </summary>
		public TestProjectConfiguration Configuration { get; } = new();

		/// <summary>
		/// Adds an assembly to the project.
		/// </summary>
		/// <param name="assembly">The assembly to add to the project.</param>
		public void Add(XunitProjectAssembly assembly)
		{
			Guard.ArgumentNotNull("assembly", assembly);

			assemblies.Add(assembly);
		}

		/// <inheritdoc/>
		public IEnumerator<XunitProjectAssembly> GetEnumerator() =>
			assemblies.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();
	}
}

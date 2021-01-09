using System.IO;
using Xunit.Internal;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Represents an assembly in an <see cref="XunitProject"/>.
	/// </summary>
	public class XunitProjectAssembly
	{
		TestAssemblyConfiguration? configuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitProjectAssembly"/> class.
		/// </summary>
		/// <param name="project">The project this assembly belongs to.</param>
		public XunitProjectAssembly(XunitProject project)
		{
			Project = Guard.ArgumentNotNull(nameof(project), project);
		}

		/// <summary>
		/// Gets the assembly display name. Will return the value "&lt;dynamic&gt;" if the
		/// assembly does not have a file name.
		/// </summary>
		public string AssemblyDisplayName =>
			string.IsNullOrWhiteSpace(AssemblyFilename) ? "<dynamic>" : Path.GetFileNameWithoutExtension(AssemblyFilename);

		/// <summary>
		/// Gets or sets the assembly filename.
		/// </summary>
		public string? AssemblyFilename { get; set; }

		/// <summary>
		/// Gets or sets the config filename.
		/// </summary>
		public string? ConfigFilename { get; set; }

		/// <summary>
		/// Gets the configuration values read from the test assembly configuration file.
		/// </summary>
		public TestAssemblyConfiguration Configuration
		{
			get
			{
				if (configuration is null)
					configuration = ConfigReader.Load(AssemblyFilename ?? string.Empty, ConfigFilename);

				return configuration;
			}
		}

		/// <summary>
		/// Gets the project that this project assembly belongs to.
		/// </summary>
		public XunitProject Project { get; }
	}
}

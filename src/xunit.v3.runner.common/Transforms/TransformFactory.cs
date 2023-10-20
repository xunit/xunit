using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to retrieve a list of available
/// </summary>
public class TransformFactory
{
	static readonly TransformFactory instance = new();

	readonly List<Transform> availableTransforms;

	TransformFactory()
	{
		availableTransforms = new()
		{
			new Transform(
				"xml",
				"output results to xUnit.net v2+ XML file",
				Handler_DirectWrite
			),
			new Transform(
				"xmlV1",
				"output results to xUnit.net v1 XML file",
				(xml, outputFileName) => Handler_XslTransform("xUnit1.xslt", xml, outputFileName)
			),
			new Transform(
				"html",
				"output results to HTML file",
				(xml, outputFileName) => Handler_XslTransform("HTML.xslt", xml, outputFileName)
			),
			new Transform(
				"nUnit",
				"output results to NUnit v2.5 XML file",
				(xml, outputFileName) => Handler_XslTransform("NUnitXml.xslt", xml, outputFileName)
			),
			new Transform(
				"jUnit",
				"output results to JUnit XML file",
				(xml, outputFileName) => Handler_XslTransform("JUnitXml.xslt", xml, outputFileName)
			),
			new Transform(
				"trx",
				"output results to TRX XML file",
				(xml, outputFileName) => Handler_XslTransform("TRX.xslt", xml, outputFileName)
			),
		};
	}

	/// <summary>
	/// Gets the list of available transforms.
	/// </summary>
	public static IReadOnlyList<Transform> AvailableTransforms => instance.availableTransforms;

	/// <summary>
	/// Creates the root "assemblies" XML element.
	/// </summary>
	/// <returns></returns>
	public static XElement CreateAssembliesElement()
	{
		var result = new XElement(
			"assemblies",
			new XAttribute("schema-version", "2"),
			new XAttribute("id", Guid.NewGuid().ToString("d"))
		);

		var computer =
			// Windows
			Environment.GetEnvironmentVariable("COMPUTERNAME") ??
			// Linux
			Environment.GetEnvironmentVariable("HOSTNAME") ??
			Environment.GetEnvironmentVariable("NAME") ??
			// macOS
			Environment.GetEnvironmentVariable("HOST");

		if (computer is not null)
			result.Add(new XAttribute("computer", computer));

		var user =
			// Windows
			Environment.GetEnvironmentVariable("USERNAME") ??
			// Linux/macOS
			Environment.GetEnvironmentVariable("LOGNAME") ??
			Environment.GetEnvironmentVariable("USER");

		if (user is not null)
			result.Add(new XAttribute("user", user));

		return result;
	}

	/// <summary>
	/// Finishes the assemblies element by supplementing with summary attributes.
	/// </summary>
	/// <param name="assembliesElement"></param>
	public static void FinishAssembliesElement(XElement assembliesElement)
	{
		Guard.ArgumentNotNull(assembliesElement);

		var assemblyElements = assembliesElement.DescendantNodes().OfType<XElement>().ToList();

		var start = default(string);
		var finish = default(string);

		if (assemblyElements.Count > 0)
		{
			start = assemblyElements.Select(a => a.Attribute("start-rtf")?.Value).WhereNotNull().Min();
			finish = assemblyElements.Select(a => a.Attribute("finish-rtf")?.Value).WhereNotNull().Max();
		}

		start ??= DateTimeOffset.MinValue.ToString("o", CultureInfo.InvariantCulture);
		finish ??= DateTimeOffset.MinValue.ToString("o", CultureInfo.InvariantCulture);

		var finishTimestamp = DateTime.Parse(finish, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

		assembliesElement.Add(
			new XAttribute("start-rtf", start),
			new XAttribute("finish-rtf", finish),
			new XAttribute("timestamp", finishTimestamp)
		);
	}

	/// <summary>
	/// Gets the list of XML transformer functions for the given project.
	/// </summary>
	/// <param name="project">The project to get transforms for.</param>
	/// <returns>The list of transform functions.</returns>
	public static List<Action<XElement>> GetXmlTransformers(XunitProject project)
	{
		Guard.ArgumentNotNull(project);

		return
			project
				.Configuration
				.Output
				.Select(output => new Action<XElement>(xml => instance.availableTransforms.Single(t => t.ID.Equals(output.Key, StringComparison.OrdinalIgnoreCase)).OutputHandler(xml, output.Value)))
				.ToList();
	}

	static void Handler_DirectWrite(
		XElement xml,
		string outputFileName)
	{
		Guard.ArgumentNotNull(xml);
		Guard.ArgumentNotNull(outputFileName);

		using var stream = File.Create(outputFileName);
		xml.Save(stream);
	}

	static void Handler_XslTransform(
		string resourceName,
		XElement xml,
		string outputFileName)
	{
		Guard.ArgumentNotNull(xml);
		Guard.ArgumentNotNull(outputFileName);

		var xmlTransform = new XslCompiledTransform();
		var fqResourceName = string.Format(CultureInfo.InvariantCulture, "Xunit.Runner.Common.Transforms.templates.{0}", resourceName);

		using var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true });
		using var xsltStream = typeof(TransformFactory).Assembly.GetManifestResourceStream(fqResourceName);
		if (xsltStream is null)
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not load resource '{0}' from assembly '{1}'", fqResourceName, typeof(TransformFactory).Assembly.Location));

		using var xsltReader = XmlReader.Create(xsltStream);
		using var xmlReader = xml.CreateReader();
		xmlTransform.Load(xsltReader);
		xmlTransform.Transform(xmlReader, writer);
	}

	/// <summary>
	/// Runs the transformation for the given ID and XML, and writes it to the given output file.
	/// </summary>
	/// <param name="id">The transform ID</param>
	/// <param name="assembliesElement">The assembly XML to transform</param>
	/// <param name="outputFileName">The output file name</param>
	public static void Transform(
		string id,
		XElement assembliesElement,
		string outputFileName)
	{
		Guard.ArgumentNotNull(id);
		Guard.ArgumentNotNull(assembliesElement);
		Guard.ArgumentNotNull(outputFileName);

		var transform = AvailableTransforms.FirstOrDefault(t => string.Equals(t.ID, id, StringComparison.OrdinalIgnoreCase));

		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot find transform with ID '{0}'", id), transform);

		transform.OutputHandler(assembliesElement, outputFileName);
	}
}

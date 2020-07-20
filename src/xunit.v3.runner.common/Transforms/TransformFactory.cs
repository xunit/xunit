using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Used to retrieve a list of available
	/// </summary>
	public class TransformFactory
	{
		static readonly TransformFactory instance = new TransformFactory();

		readonly List<Transform> availableTransforms;

		TransformFactory()
		{
			availableTransforms = new List<Transform> {
				new Transform(
					"xml",
					"output results to xUnit.net v2+ XML file",
					Handler_DirectWrite
				),
				new Transform(
					"xmlv1",
					"output results to xUnit.net v1 XML file",
					(xml, outputFileName) => Handler_XslTransform("xUnit1.xslt", xml, outputFileName)
				),
				new Transform(
					"html",
					"output results to HTML file",
					(xml, outputFileName) => Handler_XslTransform("HTML.xslt", xml, outputFileName)
				),
				new Transform(
					"nunit",
					"output results to NUnit v2.5 XML file",
					(xml, outputFileName) => Handler_XslTransform("NUnitXml.xslt", xml, outputFileName)
				),
				new Transform(
					"junit",
					"output results to JUnit XML file",
					(xml, outputFileName) => Handler_XslTransform("JUnitXml.xslt", xml, outputFileName)
				),
			};
		}

		/// <summary>
		/// Gets the list of available transforms.
		/// </summary>
		public static IReadOnlyList<Transform> AvailableTransforms => instance.availableTransforms;

		/// <summary>
		/// Gets the list of XML transformer functions for the given project.
		/// </summary>
		/// <param name="project">The project to get transforms for.</param>
		/// <returns>The list of transform functions.</returns>
		public static List<Action<XElement>> GetXmlTransformers(XunitProject project)
		{
			Guard.ArgumentNotNull(nameof(project), project);

			return
				project
					.Output
					.Select(output => new Action<XElement>(xml => instance.availableTransforms.Single(t => t.ID == output.Key).OutputHandler(xml, output.Value)))
					.ToList();
		}

		static void Handler_DirectWrite(XElement xml, string outputFileName)
		{
			Guard.ArgumentNotNull(nameof(xml), xml);
			Guard.ArgumentNotNull(nameof(outputFileName), outputFileName);

			using var stream = File.Create(outputFileName);
			xml.Save(stream);
		}

		static void Handler_XslTransform(string resourceName, XElement xml, string outputFileName)
		{
			Guard.ArgumentNotNull(nameof(xml), xml);
			Guard.ArgumentNotNull(nameof(outputFileName), outputFileName);

			var xmlTransform = new XslCompiledTransform();

			using var writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true });
			using var xsltStream = typeof(TransformFactory).GetTypeInfo().Assembly.GetManifestResourceStream($"Xunit.Runner.Common.Transforms.templates.{resourceName}");
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
			Guard.ArgumentNotNull(nameof(id), id);
			Guard.ArgumentNotNull(nameof(assembliesElement), assembliesElement);
			Guard.ArgumentNotNull(nameof(outputFileName), outputFileName);

			var transform = AvailableTransforms.FirstOrDefault(t => string.Equals(t.ID, id, StringComparison.OrdinalIgnoreCase));

			Guard.NotNull($"Cannot find transform with ID '{id}'", transform);

			transform.OutputHandler(assembliesElement, outputFileName);
		}
	}
}

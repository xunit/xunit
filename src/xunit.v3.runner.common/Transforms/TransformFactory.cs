using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Used to retrieve a list of available
/// </summary>
public class TransformFactory
{
	static readonly TransformFactory instance = new();

	readonly List<Transform> availableTransforms;

	TransformFactory() =>
		availableTransforms =
		[
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
				"ctrf",
				"output results to CTRF file",
				Handler_CTRF
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
				Handler_TRX
			),
		];

	/// <summary>
	/// Gets the list of available transforms.
	/// </summary>
	public static IReadOnlyList<Transform> AvailableTransforms =>
		instance.availableTransforms;

	/// <summary>
	/// Creates the root "assemblies" XML element.
	/// </summary>
	/// <returns></returns>
	public static XElement CreateAssembliesElement()
	{
		var result = new XElement(
			"assemblies",
			new XAttribute("schema-version", "3"),
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

	static void Handler_CTRF(
		XElement xml,
		string outputFileName)
	{
		static void SerializeMessageAndTrace(
			JsonObjectSerializer obj,
			XElement? failure)
		{
			if (failure is null)
				return;

			if (failure.Element("message") is XElement messageXml)
				obj.Serialize("message", messageXml.Value);
			if (failure.Element("stack-trace") is XElement stackTraceXml)
				obj.Serialize("trace", stackTraceXml.Value);
		}

		var buffer = new StringBuilder();

		var totalRun = 0L;
		var totalPassed = 0L;
		var totalFailed = 0L;
		var totalSkipped = 0L;
		var totalNotRun = 0L;
		var totalOther = 0L;
		var totalSuites = 0L;

		using (var rootJson = new JsonObjectSerializer(buffer))
		{
			rootJson.Serialize("reportFormat", "CTRF");
			rootJson.Serialize("specVersion", "0.0.0");
			rootJson.Serialize("reportId", Guid.NewGuid().ToString());
			rootJson.Serialize("timestamp", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));

			using var resultsJson = rootJson.SerializeObject("results");
			using (var toolJson = resultsJson.SerializeObject("tool"))
			{
				toolJson.Serialize("name", "xUnit.net v3");
				toolJson.Serialize("version", ThisAssembly.AssemblyInformationalVersion);
			}

			using (var environmentJson = resultsJson.SerializeObject("environment"))
			{
				var osPlatform = "Unknown";
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
					osPlatform = "Windows";
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					osPlatform = "Linux";
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					osPlatform = "macOS";

				environmentJson.Serialize("osPlatform", osPlatform);
				environmentJson.Serialize("osRelease", RuntimeInformation.OSDescription.Trim());
			}

			using (var extraJson = resultsJson.SerializeObject("extra"))
			{
				if (xml.Attribute("computer") is XAttribute computerXml)
					extraJson.Serialize("computer", computerXml.Value);
				if (xml.Attribute("user") is XAttribute userXml)
					extraJson.Serialize("user", userXml.Value);

				using var suitesJson = extraJson.SerializeArray("suites");
				foreach (var assemblyXml in xml.Elements("assembly"))
				{
					using var suiteJson = suitesJson.SerializeObject();

					totalSuites++;

					if (assemblyXml.Attribute("id") is XAttribute idXml)
						suiteJson.Serialize("id", idXml.Value);
					if (assemblyXml.Attribute("name") is XAttribute nameXml)
						suiteJson.Serialize("filePath", nameXml.Value);
					if (assemblyXml.Attribute("config-file") is XAttribute configFileXml)
						suiteJson.Serialize("configPath", configFileXml.Value);
					if (assemblyXml.Attribute("environment") is XAttribute environmentXml)
						suiteJson.Serialize("environment", environmentXml.Value);
					if (assemblyXml.Attribute("test-framework") is XAttribute testFrameworkXml)
						suiteJson.Serialize("testFramework", testFrameworkXml.Value);
					if (assemblyXml.Attribute("target-framework") is XAttribute targetFrameworkXml)
						suiteJson.Serialize("targetFramework", targetFrameworkXml.Value);
					if (xml.Attribute("start-rtf") is XAttribute startRtfXml)
						suiteJson.Serialize("start", DateTimeOffset.Parse(startRtfXml.Value, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds());
					if (xml.Attribute("finish-rtf") is XAttribute finishRtfXml)
						suiteJson.Serialize("stop", DateTimeOffset.Parse(finishRtfXml.Value, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds());
					if (assemblyXml.Attribute("time") is XAttribute timeXml)
						suiteJson.Serialize("duration", (long)(decimal.Parse(timeXml.Value, CultureInfo.InvariantCulture) * 1000));

					if (assemblyXml.Attribute("errors") is XAttribute errorsXml)
						totalOther += long.Parse(errorsXml.Value, CultureInfo.InvariantCulture);
					if (assemblyXml.Attribute("failed") is XAttribute failedXml)
						totalFailed += long.Parse(failedXml.Value, CultureInfo.InvariantCulture);
					if (assemblyXml.Attribute("not-run") is XAttribute notRunXml)
						totalNotRun += long.Parse(notRunXml.Value, CultureInfo.InvariantCulture);
					if (assemblyXml.Attribute("passed") is XAttribute passedXml)
						totalPassed += long.Parse(passedXml.Value, CultureInfo.InvariantCulture);
					if (assemblyXml.Attribute("skipped") is XAttribute skippedXml)
						totalSkipped += long.Parse(skippedXml.Value, CultureInfo.InvariantCulture);
					if (assemblyXml.Attribute("total") is XAttribute totalXml)
						totalRun += long.Parse(totalXml.Value, CultureInfo.InvariantCulture);

					using (var collectionsJson = suiteJson.SerializeArray("collections"))
						foreach (var collectionXml in assemblyXml.Elements("collection"))
							using (var collection = collectionsJson.SerializeObject())
							{
								if (collectionXml.Attribute("id") is XAttribute collectionIdXml)
									collection.Serialize("id", collectionIdXml.Value);
								if (collectionXml.Attribute("name") is XAttribute collectionNameXml)
									collection.Serialize("name", collectionNameXml.Value);
							}

					using var errorsJson = suiteJson.SerializeArray("errors");
					if (assemblyXml.Element("errors") is XElement assemblyErrorsXml)
						foreach (var assemblyErrorXml in assemblyErrorsXml.Elements("error"))
							using (var error = errorsJson.SerializeObject())
							{
								if (assemblyErrorXml.Attribute("name") is XAttribute assemblyErrorNameXml)
									error.Serialize("name", assemblyErrorNameXml.Value);
								if (assemblyErrorXml.Attribute("type") is XAttribute assemblyErrorTypeXml)
									error.Serialize("type", assemblyErrorTypeXml.Value);

								if (assemblyErrorsXml.Element("failure") is XElement assemblyErrorFailureXml)
								{
									if (assemblyErrorFailureXml.Attribute("exception-type") is XAttribute failureExceptionXml)
										error.Serialize("exception", failureExceptionXml.Value);

									SerializeMessageAndTrace(error, assemblyErrorFailureXml);
								}
							}
				}
			}

			using (var summaryJson = resultsJson.SerializeObject("summary"))
			{
				var start = 0L;
				if (xml.Attribute("start-rtf") is XAttribute startRtfXml)
					start = DateTimeOffset.Parse(startRtfXml.Value, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();

				var stop = 0L;
				if (xml.Attribute("finish-rtf") is XAttribute finishRtfXml)
					stop = DateTimeOffset.Parse(finishRtfXml.Value, CultureInfo.InvariantCulture).ToUnixTimeMilliseconds();

				summaryJson.Serialize("tests", totalRun);
				summaryJson.Serialize("passed", totalPassed);
				summaryJson.Serialize("failed", totalFailed);
				summaryJson.Serialize("pending", totalNotRun);
				summaryJson.Serialize("skipped", totalSkipped);
				summaryJson.Serialize("other", totalOther);
				summaryJson.Serialize("suites", totalSuites);
				summaryJson.Serialize("start", start);
				summaryJson.Serialize("stop", stop);
			}

			using var testsJson = resultsJson.SerializeArray("tests");
			foreach (var assemblyXml in xml.Elements("assembly"))
			{
				var suiteID = assemblyXml.Attribute("id")?.Value;

				foreach (var collectionXml in assemblyXml.Elements("collection"))
				{
					var collectionID = collectionXml.Attribute("id")?.Value;

					foreach (var testXml in collectionXml.Elements("test"))
						using (var testJson = testsJson.SerializeObject())
						{
							var name = testXml.Attribute("name")?.Value ?? "<unnamed test>";
							var status = testXml.Attribute("result")?.Value.ToUpperInvariant() switch
							{
								"PASS" => "passed",
								"FAIL" => "failed",
								"SKIP" => "skipped",
								"NOTRUN" => "pending",
								_ => "other",
							};
							var duration = 0L;

							if (testXml.Attribute("time") is XAttribute timeXml)
								duration = (long)(decimal.Parse(timeXml.Value, CultureInfo.InvariantCulture) * 1000);

							testJson.Serialize("name", name);
							testJson.Serialize("status", status);
							testJson.Serialize("duration", duration);

							if (suiteID is not null)
								testJson.Serialize("suite", suiteID);
							if (testXml.Attribute("source-file") is XAttribute sourceFileXml)
								testJson.Serialize("filePath", sourceFileXml.Value);
							if (testXml.Attribute("source-line") is XAttribute sourceLineXml)
								testJson.Serialize("line", long.Parse(sourceLineXml.Value, CultureInfo.InvariantCulture));

							var failureXml = testXml.Element("failure");
							if (failureXml is not null)
								SerializeMessageAndTrace(testJson, failureXml);

							var traits = new Dictionary<string, HashSet<string>>();
							var traitsXml = testXml.Element("traits")?.Elements("trait");
							if (traitsXml is not null)
								foreach (var traitXml in traitsXml)
									if (traitXml.Attribute("name") is XAttribute nameXml && traitXml.Attribute("value") is XAttribute valueXml)
										traits.Add(nameXml.Value, valueXml.Value);

							if (traits.TryGetValue("Category", out var categories))
								using (var tags = testJson.SerializeArray("tags"))
									foreach (var categoryValue in categories)
										tags.Serialize(categoryValue);

							using var extraJson = testJson.SerializeObject("extra");
							var testID = testXml.Attribute("id")?.Value;

							if (testID is not null)
								extraJson.Serialize("id", testID);
							if (collectionID is not null)
								extraJson.Serialize("collection", collectionID);
							if (testXml.Attribute("type") is XAttribute typeXml)
								extraJson.Serialize("type", typeXml.Value);
							if (testXml.Attribute("method") is XAttribute methodXml)
								extraJson.Serialize("method", methodXml.Value);

							if (testXml.Element("reason") is XElement reasonXml)
								extraJson.Serialize("reason", reasonXml.Value);
							if (testXml.Element("output") is XElement outputXml)
								extraJson.Serialize("output", outputXml.Value);
							if (testXml.Element("warnings")?.Elements("warning") is IEnumerable<XElement> warningsXml)
								using (var warningsJson = extraJson.SerializeArray("warnings"))
									foreach (var warningXml in warningsXml)
										warningsJson.Serialize(warningXml.Value);

							if (traits.Count != 0)
								using (var traitsJson = extraJson.SerializeObject("traits"))
									foreach (var kvp in traits)
										using (var traitNameJson = traitsJson.SerializeArray(kvp.Key))
											foreach (var value in kvp.Value)
												traitNameJson.Serialize(value);

							var attachmentsXml = testXml.Element("attachments")?.Elements("attachment");
							if (attachmentsXml is not null)
								using (var attachmentsJson = extraJson.SerializeArray("attachments"))
								{
									var basePath = Path.Combine(Path.GetTempPath(), testID ?? Guid.NewGuid().ToString());
									Directory.CreateDirectory(basePath);

									foreach (var attachmentXml in attachmentsXml)
										if (attachmentXml.Attribute("name") is XAttribute nameXml)
										{
											string contentType;
											byte[] content;

											if (attachmentXml.Attribute("media-type") is XAttribute mediaTypeXml)
											{
												contentType = mediaTypeXml.Value;
												content = Convert.FromBase64String(attachmentXml.Value);
											}
											else
											{
												contentType = "text/plain";
												content = Encoding.UTF8.GetBytes(attachmentXml.Value);
											}

											var localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(nameXml.Value, contentType));
											File.WriteAllBytes(localFilePath, content);

											using var attachmentJson = attachmentsJson.SerializeObject();
											attachmentJson.Serialize("name", nameXml.Value);
											attachmentJson.Serialize("contentType", contentType);
											attachmentJson.Serialize("path", localFilePath);
										}
								}

							if (failureXml is not null && failureXml.Attribute("exception-type") is XAttribute exceptionXml)
								extraJson.Serialize("exception", exceptionXml.Value);
						}
				}
			}
		}

		File.WriteAllText(outputFileName, buffer.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	static void Handler_DirectWrite(
		XElement xml,
		string outputFileName)
	{
		Guard.ArgumentNotNull(xml);
		Guard.ArgumentNotNull(outputFileName);

		using var textWriter = new XmlTextWriter(outputFileName, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		xml.Save(textWriter);
	}

	static void Handler_TRX(
		XElement xml,
		string outputFileName)
	{
		var ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

		var reportId = xml.Attribute("id")!.Value;
		var user = xml.Attribute("user")!.Value;
		var computer = xml.Attribute("computer")!.Value;
		var startRtf = xml.Attribute("start-rtf")!.Value;

		var assemblies = xml.XPathSelectElements("assembly").CastOrToReadOnlyCollection();
		var tests = xml.XPathSelectElements("assembly/collection/test").CastOrToReadOnlyCollection();

		var totalRun = assemblies.Sum(assembly => int.Parse(assembly.Attribute("total")?.Value ?? "0", CultureInfo.InvariantCulture));
		var totalPassed = assemblies.Sum(assembly => int.Parse(assembly.Attribute("passed")?.Value ?? "0", CultureInfo.InvariantCulture));
		var totalFailed = assemblies.Sum(assembly => int.Parse(assembly.Attribute("failed")?.Value ?? "0", CultureInfo.InvariantCulture));
		var totalSkipped = assemblies.Sum(assembly => int.Parse(assembly.Attribute("skipped")?.Value ?? "0", CultureInfo.InvariantCulture));
		var totalNotRun = assemblies.Sum(assembly => int.Parse(assembly.Attribute("not-run")?.Value ?? "0", CultureInfo.InvariantCulture));
		var totalErrors = assemblies.Sum(assembly => int.Parse(assembly.Attribute("errors")?.Value ?? "0", CultureInfo.InvariantCulture));

		var result = new XDocument(new XDeclaration(version: "1.0", encoding: "utf-8", standalone: null),
			new XElement(ns + "TestRun",
				new XAttribute("id", reportId),
				new XAttribute("name", $"{user}@{computer} {startRtf}"),
				new XAttribute("runUser", user),
				new XElement(ns + "Times",
					new XAttribute("creation", startRtf),
					new XAttribute("queuing", startRtf),
					new XAttribute("start", startRtf),
					new XAttribute("finish", xml.Attribute("finish-rtf")!.Value)
				),
				new XElement(ns + "TestSettings",
					new XAttribute("name", "default"),
					new XAttribute("id", "6c4d5628-128d-4c3b-a1a4-ab366a4594ad")
				),
				new XElement(ns + "Results",
					tests.Select(test =>
					{
						var testId = test.Attribute("id")!.Value;
						var element =
							new XElement(ns + "UnitTestResult",
								new XAttribute("testName", test.Attribute("name")!.Value),
								new XAttribute("outcome", test.Attribute("result")!.Value switch
								{
									"Pass" => "Passed",
									"Fail" => "Failed",
									"Skip" => "NotExecuted",
									_ => "NotRunnable"
								}),
								new XAttribute("duration", test.Attribute("time-rtf")!.Value),
								new XAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b"),
								new XAttribute("testListId", "8c84fa94-04c1-424b-9868-57a2d4851a1d"),
								new XAttribute("testId", testId),
								new XAttribute("executionId", testId),
								new XAttribute("computerName", computer),
								new XAttribute("startTime", test.Attribute("start-rtf")!.Value),
								new XAttribute("endTime", test.Attribute("finish-rtf")!.Value)
							);

						if (test.XPathSelectElement("attachments") is XElement attachments)
						{
							var resultFiles = new XElement(ns + "ResultFiles");
							var basePath = Path.Combine(Path.GetTempPath(), testId);
							Directory.CreateDirectory(basePath);

							foreach (var attachment in attachments.XPathSelectElements("attachment"))
							{
								var fileName = attachment.Attribute("name")!.Value;
								string localFilePath;

								if (attachment.Attribute("media-type") is XAttribute mediaType)
								{
									localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(fileName, mediaType.Value));
									File.WriteAllBytes(localFilePath, Convert.FromBase64String(attachment.Value));
								}
								else
								{
									localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(fileName, "text/plain"));
									File.WriteAllText(localFilePath, attachment.Value);
								}

								resultFiles.Add(
									new XElement(ns + "ResultFile",
										new XAttribute("path", localFilePath)
									)
								);
							}

							element.Add(resultFiles);
						}

						var output = new XElement(ns + "Output");
						var textMessages = new XElement(ns + "TextMessages");

						if (test.XPathSelectElement("output") is XElement outputMessages)
							foreach (var line in outputMessages.Value.TrimEnd('\r', '\n').Split(["\r\n"], StringSplitOptions.None))
								textMessages.Add(new XElement(ns + "Message", new XText(line)));

						if (test.XPathSelectElement("warnings") is XElement warnings)
							foreach (var warning in warnings.XPathSelectElements("warning"))
								textMessages.Add(new XElement(ns + "Message", new XText("WARNING: " + warning.Value)));

						if (!textMessages.IsEmpty)
							output.Add(textMessages);

						if (test.XPathSelectElement("failure") is XElement failure)
						{
							var errorInfo = new XElement(ns + "ErrorInfo");

							if (failure.XPathSelectElement("message") is XElement message)
								errorInfo.Add(new XElement(ns + "Message", new XText(message.Value)));
							if (failure.XPathSelectElement("stack-trace") is XElement stackTrace)
								errorInfo.Add(new XElement(ns + "StackTrace", new XText(stackTrace.Value)));

							output.Add(errorInfo);
						}
						if (test.XPathSelectElement("reason") is XElement reason)
							output.Add(new XElement(ns + "StdOut", new XText(reason.Value)));

						if (!output.IsEmpty)
							element.Add(output);

						return element;
					})
				),
				new XElement(ns + "TestDefinitions",
					assemblies.Select(assembly =>
					{
						var assemblyPath = assembly.Attribute("name")!.Value;
						var results = new List<XElement>();

						foreach (var test in assembly.XPathSelectElements("collection/test"))
							results.Add(
								new XElement(ns + "UnitTest",
									new XAttribute("name", test.Attribute("name")!.Value),
									new XAttribute("storage", assemblyPath),
									new XAttribute("id", test.Attribute("id")!.Value),
									new XElement(ns + "Execution",
										new XAttribute("id", test.Attribute("id")!.Value)
									),
									new XElement(ns + "TestMethod",
										new XAttribute("codeBase", assemblyPath),
										new XAttribute("className", test.Attribute("type")!.Value),
										new XAttribute("name", test.Attribute("method")!.Value),
										new XAttribute("adapterTypeName", $"executor://{reportId}/{ThisAssembly.AssemblyFileVersion}")
									)
								)
							);

						return results;
					})
				),
				new XElement(ns + "TestEntries",
					tests.Select(test =>
						new XElement(ns + "TestEntry",
							new XAttribute("testListId", "8c84fa94-04c1-424b-9868-57a2d4851a1d"),
							new XAttribute("testId", test.Attribute("id")!.Value),
							new XAttribute("executionId", test.Attribute("id")!.Value)
						)
					)
				),
				new XElement(ns + "TestLists",
					new XElement(ns + "TestList",
						new XAttribute("name", "Results Not in a List"),
						new XAttribute("id", "8c84fa94-04c1-424b-9868-57a2d4851a1d")
					),
					new XElement(ns + "TestList",
						new XAttribute("name", "All Loaded Results"),
						new XAttribute("id", "19431567-8539-422a-85d7-44ee4e166bda")
					)
				),
				new XElement(ns + "ResultSummary",
					new XAttribute("outcome", totalErrors + totalFailed > 0 ? "Failed" : "Complete"),
					new XElement(ns + "Counters",
						new XAttribute("total", totalRun),
						new XAttribute("executed", totalRun - totalSkipped - totalNotRun),
						new XAttribute("passed", totalPassed),
						new XAttribute("failed", totalFailed),
						new XAttribute("error", totalErrors),
						new XAttribute("timeout", 0),
						new XAttribute("aborted", 0),
						new XAttribute("inconclusive", 0),
						new XAttribute("passedButRunAborted", 0),
						new XAttribute("notRunnable", totalNotRun),
						new XAttribute("notExecuted", totalSkipped),
						new XAttribute("disconnected", 0),
						new XAttribute("warning", 0),
						new XAttribute("completed", 0),
						new XAttribute("inProgress", 0),
						new XAttribute("pending", 0)
					)
				)
			)
		);

		File.WriteAllText(outputFileName, result.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
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

		using var textWriter = new XmlTextWriter(outputFileName, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		using var writer = XmlWriter.Create(textWriter, new XmlWriterSettings { Indent = true });
		using var xsltStream =
			typeof(TransformFactory).Assembly.GetManifestResourceStream(fqResourceName)
				?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Could not load resource '{0}' from assembly '{1}'", fqResourceName, typeof(TransformFactory).Assembly.Location));

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

using System.Xml;
using System.Xml.Linq;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="HtmlResultWriter"/>.
/// </summary>
public class HtmlResultWriterMessageHandler : MarkupResultWriterMessageHandlerBase
{
	bool disposed;
	readonly Lazy<XmlWriter> xmlWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="HtmlResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public HtmlResultWriterMessageHandler(string fileName) :
		this(new Lazy<XmlWriter>(() => XmlWriter.Create(fileName, XmlUtility.HtmlWriterSettings), isThreadSafe: false))
	{ }

	HtmlResultWriterMessageHandler(Lazy<XmlWriter> xmlWriter) =>
		this.xmlWriter = xmlWriter;

	/// <inheritdoc/>
	public override ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var totalsElements = new List<object>();
				var headerElements = new List<object>();
				var allElements = new List<object>();

				if (Totals.Errors != 0)
				{
					totalsElements.AddRange([
						"Errors: ",
						new XElement("a",
							new XAttribute("href", "#errors"),
							new XElement("b", Totals.Errors)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "errors")), "Errors"),
						Tests.Where(t => t.Status == TestResultStatus.Error).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (Totals.Failed != 0)
				{
					totalsElements.AddRange([
						"Failures: ",
						new XElement("a",
							new XAttribute("href", "#failed"),
							new XElement("b", Totals.Failed)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "failed")), "Failed tests"),
						Tests.Where(t => t.Status == TestResultStatus.Failed).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (Totals.Skipped != 0)
				{
					totalsElements.AddRange([
						"Skipped: ",
						new XElement("a",
							new XAttribute("href", "#skipped"),
							new XElement("b", Totals.Skipped)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "skipped")), "Skipped tests"),
						Tests.Where(t => t.Status == TestResultStatus.Skipped).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (Totals.NotRun != 0)
				{
					totalsElements.AddRange([
						"Not Run: ",
						new XElement("a",
							new XAttribute("href", "#notrun"),
							new XElement("b", Totals.NotRun)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "notrun")), "Not run tests"),
						Tests.Where(t => t.Status == TestResultStatus.NotRun).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				var currentID = 0;

				allElements.AddRange([
					new XElement("br"),
					new XElement("h2", new XElement("a", new XAttribute("id", "all")), "All tests"),
					new XElement("h5", "Click test class name to expand/collapse test details"),
					Tests.Where(t => t.Status != TestResultStatus.Error).GroupBy(t => t.ClassName).OrderBy(g => g.Key).Select(group =>
						new XElement("h3",
							new XElement("span",
								new XAttribute("class", "timing"),
								$"{group.Sum(g => g.Time):0.000}s"
							),
							new XElement("span",
								new XAttribute("class", "clickable"),
								new XAttribute("onclick", $"ToggleClass('class{++currentID}')"),
								new XAttribute("ondblclick", $"ToggleClass('class{currentID}')"),
								new XElement("span",
									new XAttribute("class", "status-icon"),
									group.Any(g => g.Status == TestResultStatus.Failed)
										? new XElement("span", new XAttribute("class", "failure"), "✗")
										: group.Any(g => g.Status == TestResultStatus.Skipped)
											? new XElement("span", new XAttribute("class", "skipped"), "?")
											: group.Any(g => g.Status == TestResultStatus.NotRun)
												? new XElement("span", new XAttribute("class", "notrun"), "🛇")
												: new XElement("span", new XAttribute("class", "success"), "✓")
								),
								$" {group.Key} ",
								new XElement("span",
									new XAttribute("class", "testcount"),
									$"[{group.Count()}]"
								)
							),
							new XElement("br", new XAttribute("clear", "all")),
							new XElement("div",
								new XAttribute("class", "indent"),
								group.Any(g => g.Status == TestResultStatus.Failed) ? "" : new XAttribute("style", "display: none;"),
								new XAttribute("id", $"class{currentID}"),
								group.OrderBy(g => g.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
							)
						)
					)
				]);

				var htmlElement =
					new XElement("html",
						new XElement("head",
							new XElement("title", "xUnit.net Test Results"),
							new XElement("style",
								new XAttribute("type", "text/css"),
								"""

								      body { font-family: Calibri, Verdana, Arial, sans-serif; background-color: White; color: Black; }
								      h2,h3,h4,h5 { margin: 0; padding: 0; }
								      h3 { font-weight: normal; }
								      h4 { margin: 0.5em 0; }
								      h5 { font-weight: normal; font-style: italic; margin-bottom: 0.75em; }
								      h6 { font-size: 0.9em; font-weight: bold; margin: 0.5em 0 0 2em; padding: 0; }
								      pre,table { font-family: Consolas; font-size: 0.8em; margin: 0 0 0 2.25em; padding: 0; }
								      li pre { margin: 0; }
								      table { padding-bottom: 0.25em; }
								      th { padding: 0 0.5em; border-right: 1px solid #bbb; text-align: left; }
								      td { padding-left: 0.5em; }
								      ul { margin: 0 0 0 1em; }
								      .divided { border-top: solid 1px #e0e2e8; padding-top: 0.5em; margin-top: 0.5em; }
								      .row, .altrow { padding: 0.1em 0.3em; }
								      .row { background-color: #f0f5fa; }
								      .altrow { background-color: #e1ebf4; }
								      .success, .failure, .skipped, .notrun { font-weight: bold; }
								      .success { color: #0c0; }
								      .failure { color: #c00; }
								      .skipped { color: #cc0; }
								      .notrun { color: #999; }
								      .timing { float: right; }
								      .indent { margin: 0.25em 0 0.5em 1.5em; }
								      .clickable { cursor: pointer; }
								      .status-icon { width: 1.5em; display: inline-block; text-align: center; }
								      .testcount { font-size: 85%; }
								""" + "\r\n    "
							),
							new XElement("script",
								new XAttribute("language", "javascript"),
								"""

								      function ToggleClass(id) {
								        var elem = document.getElementById(id);
								        if (elem.style.display == "none") {
								          elem.style.display = "block";
								        }
								        else {
								          elem.style.display = "none";
								        }
								      }
								""" + "\r\n    "
							)
						),
						new XElement("body",
							new XElement("h3",
								new XAttribute("class", "divided"),
								new XElement("b", "Assemblies Run")
							),
							Assemblies.OrderBy(a => a).Select(a => new XElement("div", a)),
							new XElement("h3",
								new XAttribute("class", "divided"),
								new XElement("b", "Summary")
							),
							new XElement("div",
								"Tests run: ",
								new XElement("a", new XAttribute("href", "#all"),
									new XElement("b", Totals.Total)
								),
								" — ",
								totalsElements,
								"Run time: ",
								new XElement("b",
									Totals.Time.ToString("0.000", CultureInfo.CurrentCulture),
									"s"
								),
								", Finished: ",
								new XElement("b", TimeFinish.ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
							),
							headerElements,
							allElements
						)
					);

				htmlElement.Save(xmlWriter.Value);
				xmlWriter.Value.SafeDispose();
			}
			finally
			{
				disposed = true;
			}

		return default;
	}

	static XElement RenderTestResult(
		TestResult testResult,
		string rowClass)
	{
		var resultElement = new XElement("div",
			new XAttribute("class", rowClass),
			new XElement("span",
				new XAttribute("class", "timing"),
				Timing(testResult)
			),
			new XElement("span",
				new XAttribute("class", $"{CssClass(testResult)} status-icon"),
				Symbol(testResult)
			),
			$" {testResult.DisplayName}",
			new XElement("br", new XAttribute("clear", "all"))
		);

		if (!string.IsNullOrWhiteSpace(testResult.Message))
			resultElement.Add(new XElement("pre", testResult.Message));

		if (!string.IsNullOrWhiteSpace(testResult.StackTrace))
			resultElement.Add(new XElement("pre", testResult.StackTrace));

		if (!string.IsNullOrWhiteSpace(testResult.Output))
			resultElement.Add(
				new XElement("h6", "Output:"),
				new XElement("pre", testResult.Output)
			);

		if (testResult.Warnings is not null && testResult.Warnings.Length != 0)
			resultElement.Add(
				new XElement("h6", "Warnings:"),
				new XElement("ul",
					testResult.Warnings.Select(warning => new XElement("li", new XElement("pre", warning)))
				)
			);

		if (testResult.Traits is not null && testResult.Traits.Count != 0)
			resultElement.Add(
				new XElement("h6", "Traits:"),
				new XElement("table",
					new XAttribute("cellspacing", 0),
					new XAttribute("cellpadding", 0),
					testResult.Traits.Select(kvp =>
						kvp.Value.Select(value =>
							new XElement("tr",
								new XElement("th", kvp.Key),
								new XElement("td", value)
							)
						)
					)
				)
			);

		return resultElement;
	}

	static string CssClass(TestResult testResult) =>
		testResult.Status switch
		{
			TestResultStatus.Error or TestResultStatus.Failed => "failure",
			TestResultStatus.NotRun => "notrun",
			TestResultStatus.Passed => "success",
			TestResultStatus.Skipped => "skipped",
			_ => throw new ArgumentException($"Unknown status {testResult.Status}"),
		};

	static string Symbol(TestResult testResult) =>
		testResult.Status switch
		{
			TestResultStatus.Error or TestResultStatus.Failed => "✗",
			TestResultStatus.NotRun => "🛇",
			TestResultStatus.Passed => "✓",
			TestResultStatus.Skipped => "?",
			_ => throw new ArgumentException($"Unknown status {testResult.Status}"),
		};

	static string Timing(TestResult testResult) =>
		testResult.Status switch
		{
			TestResultStatus.Error => "",
			TestResultStatus.NotRun => "Not Run",
			TestResultStatus.Skipped => "Skipped",
			_ => $"{testResult.Time:0.000}s",
		};
}

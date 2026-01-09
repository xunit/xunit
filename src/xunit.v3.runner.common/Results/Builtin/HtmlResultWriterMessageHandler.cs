using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="HtmlResultWriter"/>.
/// </summary>
public class HtmlResultWriterMessageHandler : ResultMetadataMessageHandlerBase<ResultMetadataBase>, IResultWriterMessageHandler
{
	readonly ConcurrentBag<string> assemblies = [];
	bool disposed;
	readonly ConcurrentBag<TestResult> tests = [];
	DateTimeOffset timeFinish = DateTimeOffset.MinValue;
	readonly ExecutionSummary totals = new();
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

	internal override ResultMetadataBase CreateMetadata() =>
		new();

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var totalsElements = new List<object>();
				var headerElements = new List<object>();
				var allElements = new List<object>();

				if (totals.Errors != 0)
				{
					totalsElements.AddRange([
						"Errors: ",
						new XElement("a",
							new XAttribute("href", "#errors"),
							new XElement("b", totals.Errors)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "errors")), "Errors"),
						tests.Where(t => t.Status == TestResultStatus.Error).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (totals.Failed != 0)
				{
					totalsElements.AddRange([
						"Failures: ",
						new XElement("a",
							new XAttribute("href", "#failed"),
							new XElement("b", totals.Failed)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "failed")), "Failed tests"),
						tests.Where(t => t.Status == TestResultStatus.Failed).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (totals.Skipped != 0)
				{
					totalsElements.AddRange([
						"Skipped: ",
						new XElement("a",
							new XAttribute("href", "#skipped"),
							new XElement("b", totals.Skipped)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "skipped")), "Skipped tests"),
						tests.Where(t => t.Status == TestResultStatus.Skipped).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				if (totals.NotRun != 0)
				{
					totalsElements.AddRange([
						"Not Run: ",
						new XElement("a",
							new XAttribute("href", "#notrun"),
							new XElement("b", totals.NotRun)
						),
						", "
					]);
					headerElements.AddRange([
						new XElement("br"),
						new XElement("h2", new XElement("a", new XAttribute("id", "notrun")), "Not run tests"),
						tests.Where(t => t.Status == TestResultStatus.NotRun).OrderBy(t => t.DisplayName).Select((t, idx) => RenderTestResult(t, idx % 2 == 0 ? "row" : "altrow"))
					]);
				}

				var currentID = 0;

				allElements.AddRange([
					new XElement("br"),
					new XElement("h2", new XElement("a", new XAttribute("id", "all")), "All tests"),
					new XElement("h5", "Click test class name to expand/collapse test details"),
					tests.Where(t => t.Status != TestResultStatus.Error).GroupBy(t => t.ClassName).OrderBy(g => g.Key).Select(group =>
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
							assemblies.OrderBy(a => a).Select(a => new XElement("div", a)),
							new XElement("h3",
								new XAttribute("class", "divided"),
								new XElement("b", "Summary")
							),
							new XElement("div",
								"Tests run: ",
								new XElement("a", new XAttribute("href", "#all"),
									new XElement("b", totals.Total)
								),
								" — ",
								totalsElements,
								"Run time: ",
								new XElement("b",
									totals.Time.ToString("0.000", CultureInfo.CurrentCulture),
									"s"
								),
								", Finished: ",
								new XElement("b", timeFinish.ToLocalTime().ToString("g", CultureInfo.CurrentCulture))
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

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		tests.Add(new()
		{
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Message = ExceptionUtility.CombineMessages(message),
			Output = message.Output,
			StackTrace = ExceptionUtility.CombineStackTraces(message),
			Status = TestResultStatus.Failed,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		tests.Add(new()
		{
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Output = message.Output,
			Status = TestResultStatus.NotRun,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		tests.Add(new()
		{
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Output = message.Output,
			Status = TestResultStatus.Passed,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		tests.Add(new()
		{
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Message = message.Reason,
			Output = message.Output,
			Status = TestResultStatus.Skipped,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<IErrorMessage>(a => OnError("Fatal Error", a.Message));
		message.DispatchWhen<ITestAssemblyCleanupFailure>(a => OnError("Test Assembly Cleanup Failure", a.Message));
		message.DispatchWhen<ITestCaseCleanupFailure>(a => OnError("Test Case Cleanup Failure", a.Message));
		message.DispatchWhen<ITestClassCleanupFailure>(a => OnError("Test Class Cleanup Failure", a.Message));
		message.DispatchWhen<ITestCleanupFailure>(a => OnError("Test Cleanup Failure", a.Message));
		message.DispatchWhen<ITestCollectionCleanupFailure>(a => OnError("Test Collection Cleanup Failure", a.Message));
		message.DispatchWhen<ITestMethodCleanupFailure>(a => OnError("Test Method Cleanup Failure", a.Message));

		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	void OnError(
		string type,
		IErrorMetadata error)
	{
		tests.Add(new()
		{
			DisplayName = type,
			Message = ExceptionUtility.CombineMessages(error),
			StackTrace = ExceptionUtility.CombineStackTraces(error),
			Status = TestResultStatus.Error,
		});

		lock (totals)
			++totals.Errors;
	}

	internal override void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		ResultMetadataBase resultMetadata) =>
			assemblies.Add(message.AssemblyPath);

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		ResultMetadataBase resultMetadata)
	{
		if (timeFinish < message.FinishTime)
			timeFinish = message.FinishTime;

		lock (totals)
		{
			totals.Failed += message.TestsFailed;
			totals.NotRun += message.TestsNotRun;
			totals.Skipped += message.TestsSkipped;
			totals.Time += message.ExecutionTime;
			totals.Total += message.TestsTotal;
		}
	}

	static XElement RenderTestResult(
		TestResult testResult,
		string rowClass)
	{
		var resultElement = new XElement("div",
			new XAttribute("class", rowClass),
			new XElement("span",
				new XAttribute("class", "timing"),
				testResult.Timing
			),
			new XElement("span",
				new XAttribute("class", $"{testResult.CssClass} status-icon"),
				testResult.Symbol
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

	sealed class TestResult
	{
		internal string? ClassName;
		internal required string DisplayName;
		internal string? Message;
		internal string? Output;
		internal string? StackTrace;
		internal required TestResultStatus Status;
		internal decimal Time = 0m;
		internal IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Traits;
		internal string[]? Warnings;

		internal string CssClass => Status switch
		{
			TestResultStatus.Error or TestResultStatus.Failed => "failure",
			TestResultStatus.NotRun => "notrun",
			TestResultStatus.Passed => "success",
			TestResultStatus.Skipped => "skipped",
			_ => throw new ArgumentException($"Unknown status {Status}"),
		};

		internal string Symbol => Status switch
		{
			TestResultStatus.Error or TestResultStatus.Failed => "✗",
			TestResultStatus.NotRun => "🛇",
			TestResultStatus.Passed => "✓",
			TestResultStatus.Skipped => "?",
			_ => throw new ArgumentException($"Unknown status {Status}"),
		};

		internal string Timing => Status switch
		{
			TestResultStatus.Error => "",
			TestResultStatus.NotRun => "Not Run",
			TestResultStatus.Skipped => "Skipped",
			_ => $"{Time:0.000}s",
		};
	}

	enum TestResultStatus { Passed, Failed, Skipped, NotRun, Error };
}

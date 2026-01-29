using System.Xml.Linq;
using Xunit;
using Xunit.Runner.Common;

public class TransformTests
{
	public class Trx : IDisposable
	{
		static readonly XNamespace ns = XNamespace.Get("http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
		readonly string outputFileName = Path.GetTempFileName();
		public void Dispose()
		{
			if (File.Exists(outputFileName))
				File.Delete(outputFileName);
		}

		[Fact]
		public void SetsBasicTestRunAttributes()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" computer="TEST-COMPUTER" user="someuser" start-rtf="2025-12-30T17:42:41.6882005+00:00" finish-rtf="2025-12-30T18:42:41.7935249+01:00" timestamp="12/30/2025 18:42:41">
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var testRunElement = result.Root!;
			Assert.Equal("TestRun", testRunElement.Name.LocalName);
			Assert.Equal("0e6bd160-a817-488e-984a-42d67b04e8ed", testRunElement.Attribute("id")!.Value);
			Assert.Equal("someuser@TEST-COMPUTER 2025-12-30T17:42:41.6882005+00:00", testRunElement.Attribute("name")!.Value);
			Assert.Equal("someuser", testRunElement.Attribute("runUser")!.Value);
		}

		[Fact]
		public void CreatesTimesElement()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" start-rtf="2025-12-30T17:42:41.6882005+00:00" finish-rtf="2025-12-30T18:42:41.7935249+01:00">
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var timesElement = result.Root!.Element(ns + "Times")!;
			Assert.NotNull(timesElement);
			Assert.Equal("2025-12-30T17:42:41.6882005+00:00", timesElement.Attribute("creation")!.Value);
			Assert.Equal("2025-12-30T17:42:41.6882005+00:00", timesElement.Attribute("queuing")!.Value);
			Assert.Equal("2025-12-30T17:42:41.6882005+00:00", timesElement.Attribute("start")!.Value);
			Assert.Equal("2025-12-30T18:42:41.7935249+01:00", timesElement.Attribute("finish")!.Value);
		}

		[Fact]
		public void CreatesTestSettingsElement()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var testSettingsElement = result.Root!.Element(ns + "TestSettings")!;
			Assert.NotNull(testSettingsElement);
			Assert.Equal("default", testSettingsElement.Attribute("name")!.Value);
			Assert.Equal("6c4d5628-128d-4c3b-a1a4-ab366a4594ad", testSettingsElement.Attribute("id")!.Value);
		}

		[Fact]
		public void CreatesResultsWithPassedTest()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" computer="TEST-COMPUTER">
			  <assembly name="test.dll" total="1" passed="1" failed="0" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="1" passed="1" failed="0" skipped="0" not-run="0">
			      <test id="test-1" name="Sample Test" result="Pass" time-rtf="00:00:00.0587022" start-rtf="2025-12-30T17:42:41.6882207+00:00" finish-rtf="2025-12-30T17:42:41.7539472+00:00" />
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultsElement = result.Root!.Element(ns + "Results")!;
			var testResult = resultsElement.Elements(ns + "UnitTestResult").Single();
			Assert.Equal("Sample Test", testResult.Attribute("testName")!.Value);
			Assert.Equal("Passed", testResult.Attribute("outcome")!.Value);
			Assert.Equal("test-1", testResult.Attribute("testId")!.Value);
			Assert.Equal("test-1", testResult.Attribute("executionId")!.Value);
			Assert.Equal("TEST-COMPUTER", testResult.Attribute("computerName")!.Value);
			Assert.Equal("00:00:00.0587022", testResult.Attribute("duration")!.Value);
			Assert.Equal("2025-12-30T17:42:41.6882207+00:00", testResult.Attribute("startTime")!.Value);
			Assert.Equal("2025-12-30T17:42:41.7539472+00:00", testResult.Attribute("endTime")!.Value);
		}

		[Fact]
		public void CreatesResultsWithFailedTest()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" computer="TEST-COMPUTER">
			  <assembly name="test.dll" total="1" passed="0" failed="1" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="1" passed="0" failed="1" skipped="0" not-run="0">
			      <test id="test-1" name="Failed Test" result="Fail">
			        <failure exception-type="System.Exception">
			          <message><![CDATA[System.Exception : Test failed]]></message>
			          <stack-trace><![CDATA[   at TestClass.TestMethod() in test.cs:line 42]]></stack-trace>
			        </failure>
			      </test>
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultsElement = result.Root!.Element(ns + "Results")!;
			var testResult = resultsElement.Elements(ns + "UnitTestResult").Single();
			Assert.Equal("Failed", testResult.Attribute("outcome")!.Value);
			var output = testResult.Element(ns + "Output")!;
			var errorInfo = output.Element(ns + "ErrorInfo")!;
			Assert.Equal("System.Exception : Test failed", errorInfo.Element(ns + "Message")!.Value);
			Assert.Equal("   at TestClass.TestMethod() in test.cs:line 42", errorInfo.Element(ns + "StackTrace")!.Value);
		}

		[Fact]
		public void CreatesResultsWithSkippedTest()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" computer="TEST-COMPUTER">
			  <assembly name="test.dll" total="1" passed="0" failed="0" skipped="1" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="1" passed="0" failed="0" skipped="1" not-run="0">
			      <test id="test-1" name="Skipped Test" result="Skip">
			        <reason><![CDATA[Test skipped due to condition]]></reason>
			      </test>
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultsElement = result.Root!.Element(ns + "Results")!;
			var testResult = resultsElement.Elements(ns + "UnitTestResult").Single();
			Assert.Equal("NotExecuted", testResult.Attribute("outcome")!.Value);
			var output = testResult.Element(ns + "Output")!;
			Assert.Equal("Test skipped due to condition", output.Element(ns + "StdOut")!.Value);
		}

		[Fact]
		public void CreatesResultsWithTestOutput()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed" computer="TEST-COMPUTER">
			  <assembly name="test.dll" total="1" passed="1" failed="0" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="1" passed="1" failed="0" skipped="0" not-run="0">
			      <test id="test-1" name="Test With Output" result="Pass">
			        <output>Line 1&#xD;&#xA;Line 2&#xD;&#xA;Line 3</output>
			      </test>
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultsElement = result.Root!.Element(ns + "Results")!;
			var testResult = resultsElement.Elements(ns + "UnitTestResult").Single();
			var output = testResult.Element(ns + "Output")!;
			var textMessages = output.Element(ns + "TextMessages")!;
			var messages = textMessages.Elements(ns + "Message").Select(m => m.Value).ToList();
			Assert.Equal(3, messages.Count);
			Assert.Equal("Line 1", messages[0]);
			Assert.Equal("Line 2", messages[1]);
			Assert.Equal("Line 3", messages[2]);
		}

		[Fact]
		public void CreatesTestDefinitions()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			  <assembly name="W:\test\test.dll" total="1" passed="1" failed="0" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="1" passed="1" failed="0" skipped="0" not-run="0">
			      <test id="test-1" name="Sample Test" result="Pass" type="TestNamespace.TestClass" method="TestMethod" />
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var testDefinitions = result.Root!.Element(ns + "TestDefinitions")!;
			var unitTest = testDefinitions.Elements(ns + "UnitTest").Single();
			Assert.Equal("Sample Test", unitTest.Attribute("name")!.Value);
			Assert.Equal("test-1", unitTest.Attribute("id")!.Value);
			Assert.Equal(@"W:\test\test.dll", unitTest.Attribute("storage")!.Value);

			var execution = unitTest.Element(ns + "Execution")!;
			Assert.Equal("test-1", execution.Attribute("id")!.Value);

			var testMethod = unitTest.Element(ns + "TestMethod")!;
			Assert.Equal(@"W:\test\test.dll", testMethod.Attribute("codeBase")!.Value);
			Assert.Equal("TestNamespace.TestClass", testMethod.Attribute("className")!.Value);
			Assert.Equal("TestMethod", testMethod.Attribute("name")!.Value);
			Assert.Equal($"executor://0e6bd160-a817-488e-984a-42d67b04e8ed/xunit.v3/{ThisAssembly.AssemblyFileVersion}", testMethod.Attribute("adapterTypeName")!.Value);
		}

		[Fact]
		public void CreatesTestEntries()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			  <assembly name="test.dll" total="2" passed="2" failed="0" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="2" passed="2" failed="0" skipped="0" not-run="0">
			      <test id="test-1" name="Test 1" result="Pass" />
			      <test id="test-2" name="Test 2" result="Pass" />
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var testEntries = result.Root!.Element(ns + "TestEntries")!;
			var entries = testEntries.Elements(ns + "TestEntry").ToList();
			Assert.Equal(2, entries.Count);
			Assert.Equal("test-1", entries[0].Attribute("testId")!.Value);
			Assert.Equal("test-1", entries[0].Attribute("executionId")!.Value);
			Assert.Equal("8c84fa94-04c1-424b-9868-57a2d4851a1d", entries[0].Attribute("testListId")!.Value);
			Assert.Equal("test-2", entries[1].Attribute("testId")!.Value);
		}

		[Fact]
		public void CreatesTestLists()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var testLists = result.Root!.Element(ns + "TestLists")!;
			var lists = testLists.Elements(ns + "TestList").ToList();
			Assert.Equal(2, lists.Count);
			Assert.Equal("Results Not in a List", lists[0].Attribute("name")!.Value);
			Assert.Equal("8c84fa94-04c1-424b-9868-57a2d4851a1d", lists[0].Attribute("id")!.Value);
			Assert.Equal("All Loaded Results", lists[1].Attribute("name")!.Value);
			Assert.Equal("19431567-8539-422a-85d7-44ee4e166bda", lists[1].Attribute("id")!.Value);
		}

		[Fact]
		public void CreatesResultSummary()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			  <assembly name="test.dll" total="10" passed="7" failed="1" skipped="1" not-run="1" errors="0">
			    <collection name="Test Collection" id="collection-1" total="10" passed="7" failed="1" skipped="1" not-run="1">
			      <test id="test-1" name="Test 1" result="Pass" />
			      <test id="test-2" name="Test 2" result="Pass" />
			      <test id="test-3" name="Test 3" result="Pass" />
			      <test id="test-4" name="Test 4" result="Pass" />
			      <test id="test-5" name="Test 5" result="Pass" />
			      <test id="test-6" name="Test 6" result="Pass" />
			      <test id="test-7" name="Test 7" result="Pass" />
			      <test id="test-8" name="Test 8" result="Fail" />
			      <test id="test-9" name="Test 9" result="Skip" />
			      <test id="test-10" name="Test 10" result="NotRun" />
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultSummary = result.Root!.Element(ns + "ResultSummary")!;
			Assert.Equal("Failed", resultSummary.Attribute("outcome")!.Value);

			var counters = resultSummary.Element(ns + "Counters")!;
			Assert.Equal("10", counters.Attribute("total")!.Value);
			Assert.Equal("8", counters.Attribute("executed")!.Value); // total - skipped - notRun
			Assert.Equal("7", counters.Attribute("passed")!.Value);
			Assert.Equal("1", counters.Attribute("failed")!.Value);
			Assert.Equal("0", counters.Attribute("error")!.Value);
			Assert.Equal("1", counters.Attribute("notRunnable")!.Value);
			Assert.Equal("1", counters.Attribute("notExecuted")!.Value);
		}

		[Fact]
		public void ResultSummaryOutcomeIsCompletedWhenNoFailures()
		{
			var xml = XDocument.Parse("""
			<?xml version="1.0" encoding="utf-8"?>
			<assemblies schema-version="3" id="0e6bd160-a817-488e-984a-42d67b04e8ed">
			  <assembly name="test.dll" total="2" passed="2" failed="0" skipped="0" not-run="0" errors="0">
			    <collection name="Test Collection" id="collection-1" total="2" passed="2" failed="0" skipped="0" not-run="0">
			      <test id="test-1" name="Test 1" result="Pass" />
			      <test id="test-2" name="Test 2" result="Pass" />
			    </collection>
			  </assembly>
			</assemblies>
			""");
			var assembliesElement = xml.Root!;
			TransformFactory.Transform("trx", assembliesElement, outputFileName);

			var result = XDocument.Load(outputFileName);
			var resultSummary = result.Root!.Element(ns + "ResultSummary")!;
			Assert.Equal("Completed", resultSummary.Attribute("outcome")!.Value);
		}
	}
}

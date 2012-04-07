using System;
using System.Collections.Generic;
using TestDriven.Framework;

public class StubTestListener : ITestListener
{
    public List<TestResult> TestFinished_Summaries = new List<TestResult>();
    public List<KeyValuePair<string, Category>> WriteLine__Lines = new List<KeyValuePair<string, Category>>();

    public void WriteLine(string text, Category category)
    {
        WriteLine__Lines.Add(new KeyValuePair<string, Category>(text, category));
    }

    public void TestFinished(TestResult summary)
    {
        TestFinished_Summaries.Add(summary);
    }

    public void TestResultsUrl(string resultsUrl)
    {
        throw new NotImplementedException();
    }
}
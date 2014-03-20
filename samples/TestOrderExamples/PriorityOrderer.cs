using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

public class PriorityOrderer : ITestCaseOrderer
{
    public IEnumerable<XunitTestCase> OrderTestCases(IEnumerable<XunitTestCase> testCases)
    {
        var sortedMethods = new SortedDictionary<int, List<XunitTestCase>>();

        foreach (XunitTestCase testCase in testCases)
        {
            int priority = 0;

            foreach (IAttributeInfo attr in testCase.Method.GetCustomAttributes((typeof (TestPriorityAttribute))))
                priority = attr.GetNamedArgument<int>("Priority");

            GetOrCreate(sortedMethods, priority).Add(testCase);
        }

        foreach (var list in sortedMethods.Keys.Select(priority => sortedMethods[priority]))
        {
            list.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Method.Name, y.Method.Name));
            foreach (XunitTestCase testCase in list)
                yield return testCase;
        }
    }

    static TValue GetOrCreate<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
    {
        TValue result;

        if (dictionary.TryGetValue(key, out result)) return result;
        
        result = new TValue();
        dictionary[key] = result;

        return result;
    }
}
using System;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.Common
{
	public partial class XunitFilters
	{
		/// <summary>
		/// Filters the given method using the defined filter values.
		/// </summary>
		/// <param name="testCase">The test case to filter.</param>
		/// <returns>Returns <c>true</c> if the test case passed the filter; returns <c>false</c> otherwise.</returns>
		public bool Filter(ITestCase testCase)
		{
			Guard.ArgumentNotNull(nameof(testCase), testCase);

			SplitMethodFilters();

			if (!FilterIncludedMethodsAndClasses(testCase))
				return false;
			if (!FilterExcludedMethodsAndClasses(testCase))
				return false;
			if (!FilterIncludedTraits(testCase))
				return false;
			if (!FilterExcludedTraits(testCase))
				return false;
			if (!FilterIncludedNamespaces(testCase))
				return false;
			if (!FilterExcludedNamespaces(testCase))
				return false;
			return true;
		}

		bool FilterExcludedNamespaces(ITestCase testCase)
		{
			// No assemblies in the filter == everything is okay
			if (ExcludedNamespaces.Count == 0)
				return true;

			if (ExcludedNamespaces.Count != 0 && ExcludedNamespaces.Any(a => testCase.TestMethod.TestClass.Class.Name.StartsWith($"{a}.", StringComparison.Ordinal)))
				return false;
			return true;
		}

		bool FilterIncludedNamespaces(ITestCase testCase)
		{
			// No assemblies in the filter == everything is okay
			if (IncludedNamespaces.Count == 0)
				return true;

			if (IncludedNamespaces.Count != 0 && IncludedNamespaces.Any(a => testCase.TestMethod.TestClass.Class.Name.StartsWith($"{a}.", StringComparison.Ordinal)))
				return true;

			return false;
		}

		bool FilterExcludedMethodsAndClasses(ITestCase testCase)
		{
			// No methods or classes in the filter == everything is okay
			if (excludeMethodStandardFilters?.Count == 0 && excludeMethodRegexFilters?.Count == 0 && ExcludedClasses.Count == 0)
				return true;

			if (ExcludedClasses.Count != 0 && ExcludedClasses.Contains(testCase.TestMethod.TestClass.Class.Name))
				return false;

			var methodName = $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}";

			if (excludeMethodStandardFilters?.Count != 0 && excludeMethodStandardFilters?.Contains(methodName) == true)
				return false;

			if (excludeMethodRegexFilters != null && excludeMethodRegexFilters.Count != 0)
				foreach (var regex in excludeMethodRegexFilters)
					if (regex.IsMatch(methodName))
						return false;

			return true;
		}

		bool FilterIncludedMethodsAndClasses(ITestCase testCase)
		{
			// No methods or classes in the filter == everything is okay
			if (includeMethodStandardFilters?.Count == 0 && includeMethodRegexFilters?.Count == 0 && IncludedClasses.Count == 0)
				return true;

			if (IncludedClasses.Count != 0 && IncludedClasses.Contains(testCase.TestMethod.TestClass.Class.Name))
				return true;

			var methodName = $"{testCase.TestMethod.TestClass.Class.Name}.{testCase.TestMethod.Method.Name}";

			if (includeMethodStandardFilters?.Count != 0 && includeMethodStandardFilters?.Contains(methodName) == true)
				return true;

			if (includeMethodRegexFilters != null && includeMethodRegexFilters.Count != 0)
				foreach (var regex in includeMethodRegexFilters)
					if (regex.IsMatch(methodName))
						return true;

			return false;
		}

		bool FilterExcludedTraits(ITestCase testCase)
		{
			// No traits in the filter == everything is okay
			if (ExcludedTraits.Count == 0)
				return true;

			// No traits in the method == it's never excluded
			if (testCase.Traits.Count == 0)
				return true;

			foreach (var key in ExcludedTraits.Keys)
				foreach (var value in ExcludedTraits[key])
					if (testCase.Traits.Contains(key, value, StringComparer.OrdinalIgnoreCase))
						return false;

			return true;
		}

		bool FilterIncludedTraits(ITestCase testCase)
		{
			// No traits in the filter == everything is okay
			if (IncludedTraits.Count == 0)
				return true;

			// No traits in the method == it'll never match anything, don't try
			if (testCase.Traits.Count == 0)
				return false;

			foreach (var key in IncludedTraits.Keys)
				foreach (var value in IncludedTraits[key])
					if (testCase.Traits.Contains(key, value, StringComparer.OrdinalIgnoreCase))
						return true;

			return false;
		}
	}
}

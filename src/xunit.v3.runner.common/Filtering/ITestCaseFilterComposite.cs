using System.Collections.Generic;

namespace Xunit.Runner.Common;

internal interface ITestCaseFilterComposite : ITestCaseFilter
{
	List<ITestCaseFilter> Filters { get; }
}

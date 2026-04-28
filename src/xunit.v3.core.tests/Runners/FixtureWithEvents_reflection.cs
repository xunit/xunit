namespace Xunit.v3;

internal class FixtureWithEvents :
	INotifyTestAssemblyLifecycle, INotifyTestAssemblyLifecycleAsync,
	INotifyTestCollectionLifecycle, INotifyTestCollectionLifecycleAsync,
	INotifyTestClassLifecycle, INotifyTestClassLifecycleAsync,
	INotifyTestMethodLifecycle, INotifyTestMethodLifecycleAsync,
	INotifyTestCaseLifecycle, INotifyTestCaseLifecycleAsync,
	INotifyTestLifecycle, INotifyTestLifecycleAsync
{
	public static List<string> Events = [];

	public void OnTestAssemblyFinished(IXunitTestAssembly testAssembly) => Events.Add($"OnTestAssemblyFinished");
	public async ValueTask OnTestAssemblyFinishedAsync(IXunitTestAssembly testAssembly) => Events.Add($"OnTestAssemblyFinishedAsync");
	public void OnTestAssemblyStarting(IXunitTestAssembly testAssembly) => Events.Add($"OnTestAssemblyStarting");
	public async ValueTask OnTestAssemblyStartingAsync(IXunitTestAssembly testAssembly) => Events.Add($"OnTestAssemblyStartingAsync");

	public void OnTestCollectionFinished(IXunitTestCollection testCollection) => Events.Add($"OnTestCollectionFinished");
	public async ValueTask OnTestCollectionFinishedAsync(IXunitTestCollection testCollection) => Events.Add($"OnTestCollectionFinishedAsync");
	public void OnTestCollectionStarting(IXunitTestCollection testCollection) => Events.Add($"OnTestCollectionStarting");
	public async ValueTask OnTestCollectionStartingAsync(IXunitTestCollection testCollection) => Events.Add($"OnTestCollectionStartingAsync");

	public void OnTestClassFinished(IXunitTestClass testClass) => Events.Add($"OnTestClassFinished");
	public async ValueTask OnTestClassFinishedAsync(IXunitTestClass testClass) => Events.Add($"OnTestClassFinishedAsync");
	public void OnTestClassStarting(IXunitTestClass testClass) => Events.Add($"OnTestClassStarting");
	public async ValueTask OnTestClassStartingAsync(IXunitTestClass testClass) => Events.Add($"OnTestClassStartingAsync");

	public void OnTestMethodFinished(IXunitTestMethod testMethod) => Events.Add($"OnTestMethodFinished");
	public async ValueTask OnTestMethodFinishedAsync(IXunitTestMethod testMethod) => Events.Add($"OnTestMethodFinishedAsync");
	public void OnTestMethodStarting(IXunitTestMethod testMethod) => Events.Add($"OnTestMethodStarting");
	public async ValueTask OnTestMethodStartingAsync(IXunitTestMethod testMethod) => Events.Add($"OnTestMethodStartingAsync");

	public void OnTestCaseFinished(IXunitTestCase testCase) => Events.Add($"OnTestCaseFinished");
	public async ValueTask OnTestCaseFinishedAsync(IXunitTestCase testCase) => Events.Add($"OnTestCaseFinishedAsync");
	public void OnTestCaseStarting(IXunitTestCase testCase) => Events.Add($"OnTestCaseStarting");
	public async ValueTask OnTestCaseStartingAsync(IXunitTestCase testCase) => Events.Add($"OnTestCaseStartingAsync");

	public void OnTestFinished(IXunitTest test) => Events.Add($"OnTestFinished");
	public async ValueTask OnTestFinishedAsync(IXunitTest test) => Events.Add($"OnTestFinishedAsync");
	public void OnTestStarting(IXunitTest test) => Events.Add($"OnTestStarting");
	public async ValueTask OnTestStartingAsync(IXunitTest test) => Events.Add($"OnTestStartingAsync");
}

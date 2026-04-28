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

	public void OnTestAssemblyFinished(ICodeGenTestAssembly testAssembly) => Events.Add($"OnTestAssemblyFinished");
	public async ValueTask OnTestAssemblyFinishedAsync(ICodeGenTestAssembly testAssembly) => Events.Add($"OnTestAssemblyFinishedAsync");
	public void OnTestAssemblyStarting(ICodeGenTestAssembly testAssembly) => Events.Add($"OnTestAssemblyStarting");
	public async ValueTask OnTestAssemblyStartingAsync(ICodeGenTestAssembly testAssembly) => Events.Add($"OnTestAssemblyStartingAsync");

	public void OnTestCollectionFinished(ICodeGenTestCollection testCollection) => Events.Add($"OnTestCollectionFinished");
	public async ValueTask OnTestCollectionFinishedAsync(ICodeGenTestCollection testCollection) => Events.Add($"OnTestCollectionFinishedAsync");
	public void OnTestCollectionStarting(ICodeGenTestCollection testCollection) => Events.Add($"OnTestCollectionStarting");
	public async ValueTask OnTestCollectionStartingAsync(ICodeGenTestCollection testCollection) => Events.Add($"OnTestCollectionStartingAsync");

	public void OnTestClassFinished(ICodeGenTestClass testClass) => Events.Add($"OnTestClassFinished");
	public async ValueTask OnTestClassFinishedAsync(ICodeGenTestClass testClass) => Events.Add($"OnTestClassFinishedAsync");
	public void OnTestClassStarting(ICodeGenTestClass testClass) => Events.Add($"OnTestClassStarting");
	public async ValueTask OnTestClassStartingAsync(ICodeGenTestClass testClass) => Events.Add($"OnTestClassStartingAsync");

	public void OnTestMethodFinished(ICodeGenTestMethod testMethod) => Events.Add($"OnTestMethodFinished");
	public async ValueTask OnTestMethodFinishedAsync(ICodeGenTestMethod testMethod) => Events.Add($"OnTestMethodFinishedAsync");
	public void OnTestMethodStarting(ICodeGenTestMethod testMethod) => Events.Add($"OnTestMethodStarting");
	public async ValueTask OnTestMethodStartingAsync(ICodeGenTestMethod testMethod) => Events.Add($"OnTestMethodStartingAsync");

	public void OnTestCaseFinished(ICodeGenTestCase testCase) => Events.Add($"OnTestCaseFinished");
	public async ValueTask OnTestCaseFinishedAsync(ICodeGenTestCase testCase) => Events.Add($"OnTestCaseFinishedAsync");
	public void OnTestCaseStarting(ICodeGenTestCase testCase) => Events.Add($"OnTestCaseStarting");
	public async ValueTask OnTestCaseStartingAsync(ICodeGenTestCase testCase) => Events.Add($"OnTestCaseStartingAsync");

	public void OnTestFinished(ICodeGenTest test) => Events.Add($"OnTestFinished");
	public async ValueTask OnTestFinishedAsync(ICodeGenTest test) => Events.Add($"OnTestFinishedAsync");
	public void OnTestStarting(ICodeGenTest test) => Events.Add($"OnTestStarting");
	public async ValueTask OnTestStartingAsync(ICodeGenTest test) => Events.Add($"OnTestStartingAsync");
}

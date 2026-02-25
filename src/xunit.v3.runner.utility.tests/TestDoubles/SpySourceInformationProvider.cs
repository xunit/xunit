using Xunit.Runner.Common;

internal class SpySourceInformationProvider : ISourceInformationProvider
{
	public Func<ValueTask> OnDisposeAsync { get; set; } =
		() => default;

	public Func<string?, string?, SourceInformation> OnGetSourceInformation { get; set; } =
		(testClassName, testMethodName) => SourceInformation.Null;

	public ValueTask DisposeAsync() =>
		OnDisposeAsync();

	public SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName) =>
			OnGetSourceInformation(testClassName, testMethodName);
}

public enum BuildVerbosity
{
	quiet,
	minimal,
	normal,
	detailed,
	diagnostic,

	// Shortcut names to match with msbuild/dotnet build
	q = quiet,
	m = minimal,
	n = normal,
	d = detailed,
	diag = diagnostic
}

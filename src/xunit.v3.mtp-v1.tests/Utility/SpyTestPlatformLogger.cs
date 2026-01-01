using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Logging;

public class SpyTestPlatformLogger : ILogger
{
	public List<string> Messages { get; } = [];

	public bool IsEnabled(LogLevel logLevel) => true;

	public void Log<TState>(
		LogLevel logLevel,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter) =>
			Messages.Add($"[{logLevel}] {formatter(state, exception)}");


	public Task LogAsync<TState>(
		LogLevel logLevel,
		TState state,
		Exception? exception,
		Func<TState, Exception?, string> formatter)
	{
		Messages.Add($"[{logLevel}] {formatter(state, exception)}");

		return Task.CompletedTask;
	}
}

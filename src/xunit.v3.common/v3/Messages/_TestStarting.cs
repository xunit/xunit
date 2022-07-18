using System;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test is about to start executing.
/// </summary>
public class _TestStarting : _TestMessage, _ITestMetadata
{
	string? testDisplayName;
	IReadOnlyDictionary<string, IReadOnlyList<string>> traits = new Dictionary<string, IReadOnlyList<string>>();

	/// <inheritdoc/>
	public bool Explicit { get; set; }

	/// <inheritdoc/>
	public string TestDisplayName
	{
		get => testDisplayName ?? throw new InvalidOperationException($"Attempted to get {nameof(TestDisplayName)} on an uninitialized '{GetType().FullName}' object");
		set => testDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestDisplayName));
	}

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits
	{
		get => traits;
		set => traits = value ?? new Dictionary<string, IReadOnlyList<string>>();
	}

	/// <inheritdoc/>
	public override string ToString() =>
		$"{base.ToString()} name={testDisplayName.Quoted()}";
}

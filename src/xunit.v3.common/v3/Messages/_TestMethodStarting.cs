using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test method is about to begin executing.
	/// </summary>
	public class _TestMethodStarting : _TestMethodMessage, _ITestMethodMetadata
	{
		string? testMethod;

		/// <inheritdoc/>
		public string TestMethod
		{
			get => testMethod ?? throw new InvalidOperationException($"Attempted to get {nameof(TestMethod)} on an uninitialized '{GetType().FullName}' object");
			set => testMethod = Guard.ArgumentNotNullOrEmpty(value, nameof(TestMethod));
		}

		/// <inheritdoc/>
		public override string ToString() =>
			$"{base.ToString()} method={testMethod.Quoted()}";
	}
}

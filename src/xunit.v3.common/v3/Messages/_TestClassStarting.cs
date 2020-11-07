using System;
using Xunit.Internal;

namespace Xunit.v3
{
	/// <summary>
	/// This message indicates that a test class is about to begin executing.
	/// </summary>
	public class _TestClassStarting : _TestClassMessage, _ITestClassMetadata
	{
		string? testClass;

		/// <inheritdoc/>
		public string TestClass
		{
			get => testClass ?? throw new InvalidOperationException($"Attempted to get {nameof(TestClass)} on an uninitialized '{GetType().FullName}' object");
			set => testClass = Guard.ArgumentNotNullOrEmpty(nameof(TestClass), value);
		}
	}
}

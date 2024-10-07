using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Generates unique IDs from multiple string inputs. Used to compute the unique
/// IDs that are used inside the test framework.
/// </summary>
public sealed class UniqueIDGenerator : IDisposable
{
	bool disposed;
	readonly HashAlgorithm hasher;
	readonly Stream stream;
	readonly object streamLock = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="UniqueIDGenerator"/> class.
	/// </summary>
	public UniqueIDGenerator()
	{
		hasher = SHA256.Create();
		stream = new MemoryStream();
	}

	/// <summary>
	/// Add a string value into the unique ID computation.
	/// </summary>
	/// <param name="value">The value to be added to the unique ID computation</param>
	public void Add(string value)
	{
		Guard.ArgumentNotNull(value);

		lock (streamLock)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(UniqueIDGenerator), "Cannot use UniqueIDGenerator after you have called Compute or Dispose");

			var bytes = Encoding.UTF8.GetBytes(value);
			stream.Write(bytes, 0, bytes.Length);
			stream.WriteByte(0);
		}
	}

	/// <summary>
	/// Compute the unique ID for the given input values. Note that once the unique
	/// ID has been computed, no further <see cref="Add"/> operations will be allowed.
	/// </summary>
	/// <returns>The computed unique ID</returns>
	public string Compute()
	{
		lock (streamLock)
		{
			if (disposed)
				throw new ObjectDisposedException(nameof(UniqueIDGenerator), "Cannot use UniqueIDGenerator after you have called Compute or Dispose");

			stream.Seek(0, SeekOrigin.Begin);

			var hashBytes = hasher.ComputeHash(stream);

			Dispose();

			return ToHexString(hashBytes);
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		lock (streamLock)
		{
			if (!disposed)
			{
				disposed = true;
				stream?.Dispose();
				hasher?.Dispose();
			}
		}
	}

	/// <summary>
	/// Computes a unique ID for a test assembly.
	/// </summary>
	/// <param name="assemblyPath">The assembly path</param>
	/// <param name="configFilePath">The optional configuration file path</param>
	/// <returns>The computed unique ID for the assembly</returns>
	public static string ForAssembly(
		string assemblyPath,
		string? configFilePath)
	{
		Guard.ArgumentNotNull(assemblyPath);

		using var generator = new UniqueIDGenerator();
		generator.Add(assemblyPath);
		generator.Add(configFilePath ?? string.Empty);
		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a test.
	/// </summary>
	/// <param name="testCaseUniqueID">The unique ID of the test case that this test belongs to.</param>
	/// <param name="testIndex">The index of this test in the test case, typically starting with 0
	/// (though a negative number may be used to prevent collisions with legitimate test indices).</param>
	public static string ForTest(
		string testCaseUniqueID,
		int testIndex)
	{
		Guard.ArgumentNotNull(testCaseUniqueID);

		using var generator = new UniqueIDGenerator();
		generator.Add(testCaseUniqueID);
		generator.Add(testIndex.ToString(CultureInfo.InvariantCulture));
		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a test case.
	/// </summary>
	/// <param name="parentUniqueID">The unique ID of the parent in the hierarchy; typically the test method
	/// unique ID, but may also be the test class or test collection unique ID, when test method (and
	/// possibly test class) don't exist.</param>
	/// <param name="testMethodGenericTypes">The test method's generic types</param>
	/// <param name="testMethodArguments">The test method's arguments</param>
	/// <returns>The computed unique ID for the test case</returns>
	public static string ForTestCase(
		string parentUniqueID,
		Type[]? testMethodGenericTypes,
		object?[]? testMethodArguments)
	{
		Guard.ArgumentNotNull(parentUniqueID);

		using var generator = new UniqueIDGenerator();

		generator.Add(parentUniqueID);

		if (testMethodArguments is not null)
			generator.Add(SerializationHelper.Instance.Serialize(testMethodArguments));

		if (testMethodGenericTypes is not null)
			for (var idx = 0; idx < testMethodGenericTypes.Length; idx++)
				generator.Add(testMethodGenericTypes[idx].SafeName());

		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a test class.
	/// </summary>
	/// <param name="testCollectionUniqueID">The unique ID of the parent test collection for the test class</param>
	/// <param name="className">The optional fully qualified type name of the test class</param>
	/// <returns>The computed unique ID for the test class (may return <c>null</c> if <paramref name="className"/>
	/// is null)</returns>
	[return: NotNullIfNotNull(nameof(className))]
	public static string? ForTestClass(
		string testCollectionUniqueID,
		string? className)
	{
		Guard.ArgumentNotNull(testCollectionUniqueID);

		if (className is null)
			return null;

		using var generator = new UniqueIDGenerator();
		generator.Add(testCollectionUniqueID);
		generator.Add(className);
		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a test collection.
	/// </summary>
	/// <param name="assemblyUniqueID">The unique ID of the assembly the test collection lives in</param>
	/// <param name="collectionDisplayName">The display name of the test collection</param>
	/// <param name="collectionDefinitionClassName">The optional class name that contains the test collection definition</param>
	/// <returns>The computed unique ID for the test collection</returns>
	public static string ForTestCollection(
		string assemblyUniqueID,
		string collectionDisplayName,
		string? collectionDefinitionClassName)
	{
		Guard.ArgumentNotNull(assemblyUniqueID);
		Guard.ArgumentNotNull(collectionDisplayName);

		using var generator = new UniqueIDGenerator();
		generator.Add(assemblyUniqueID);
		generator.Add(collectionDisplayName);
		generator.Add(collectionDefinitionClassName ?? string.Empty);
		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a test method.
	/// </summary>
	/// <param name="testClassUniqueID">The unique ID of the parent test class for the test method</param>
	/// <param name="methodName">The optional test method name</param>
	/// <returns>The computed unique ID for the test method (may return <c>null</c> if either the class
	/// unique ID or the method name is null)</returns>
	[return: NotNullIfNotNull(nameof(methodName))]
	public static string? ForTestMethod(
		string? testClassUniqueID,
		string? methodName)
	{
		if (testClassUniqueID is null || methodName is null)
			return null;

		using var generator = new UniqueIDGenerator();
		generator.Add(testClassUniqueID);
		generator.Add(methodName);
		return generator.Compute();
	}

	/// <summary>
	/// Computes a unique ID for a <see cref="Type"/>.
	/// </summary>
	/// <param name="type">The type to generate an ID for</param>
	public static string ForType(Type type)
	{
		Guard.ArgumentNotNull(type);
		Guard.ArgumentNotNull(type.Assembly.FullName);

		using var generator = new UniqueIDGenerator();

		// Assembly name may include some parts that are less stable, so for now, split on comma
		var assemblyParts = type.Assembly.FullName.Split(',');
		generator.Add(assemblyParts[0]);
		generator.Add(type.SafeName());
		return generator.Compute();
	}

	static char ToHexChar(int b) =>
		(char)(b < 10 ? b + '0' : b - 10 + 'a');

	static string ToHexString(byte[] bytes)
	{
		var chars = new char[bytes.Length * 2];
		var idx = 0;

		foreach (var @byte in bytes)
		{
			chars[idx++] = ToHexChar(@byte >> 4);
			chars[idx++] = ToHexChar(@byte & 0xF);
		}

		return new string(chars);
	}
}

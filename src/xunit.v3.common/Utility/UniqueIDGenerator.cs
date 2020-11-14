using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Generates unique IDs from multiple string inputs. Used to compute the unique
	/// IDs that are used inside the test framework.
	/// </summary>
	public class UniqueIDGenerator : IDisposable
	{
		bool disposed;
		HashAlgorithm hasher;
		Stream stream;

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
			Guard.ArgumentNotNull(nameof(value), value);

			lock (stream)
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
			lock (stream)
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
			lock (stream)
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
		/// Computes a unique ID for an assembly, to be placed into <see cref="_TestAssemblyMessage.AssemblyUniqueID"/>
		/// </summary>
		/// <param name="assemblyName">The assembly name</param>
		/// <param name="assemblyPath">The optional assembly path</param>
		/// <param name="configFilePath">The optional configuration file path</param>
		/// <returns>The computed unique ID for the assembly</returns>
		public static string ForAssembly(
			string assemblyName,
			string? assemblyPath,
			string? configFilePath)
		{
			Guard.ArgumentNotNull(nameof(assemblyName), assemblyName);

			using var generator = new UniqueIDGenerator();
			generator.Add(assemblyName);
			generator.Add(assemblyPath ?? string.Empty);
			generator.Add(configFilePath ?? string.Empty);
			return generator.Compute();
		}

		/// <summary>
		/// Computes a unique ID for a test method, to be placed into <see cref="_TestMethodMessage.TestMethodUniqueID"/>
		/// </summary>
		/// <param name="testClassUniqueID">The unique ID of the parent test class for the test method</param>
		/// <param name="methodName">The optional test method name</param>
		/// <returns>The computed unique ID for the test method (may return <c>null</c> if either the class
		/// unique ID or the method name is null)</returns>
		public static string? ForTestMethod(
			string? testClassUniqueID,
			string? methodName)
		{
			if (testClassUniqueID == null || methodName == null)
				return null;

			using var generator = new UniqueIDGenerator();
			generator.Add(testClassUniqueID);
			generator.Add(methodName);
			return generator.Compute();
		}

		/// <summary>
		/// Computes a unique ID for a test class, to be placed into <see cref="_TestClassMessage.TestClassUniqueID"/>
		/// </summary>
		/// <param name="testCollectionUniqueID">The unique ID of the parent test collection for the test class</param>
		/// <param name="className">The optional fully qualified type name of the test class</param>
		/// <returns>The computed unique ID for the test class (may return <c>null</c> if the class
		/// name is null)</returns>
		public static string? ForTestClass(
			string testCollectionUniqueID,
			string? className)
		{
			Guard.ArgumentNotNull(nameof(testCollectionUniqueID), testCollectionUniqueID);

			if (className == null)
				return null;

			using var generator = new UniqueIDGenerator();
			generator.Add(testCollectionUniqueID);
			generator.Add(className);
			return generator.Compute();
		}

		/// <summary>
		/// Computes a unique ID for a test collection, to be placed into <see cref="_TestCollectionMessage.TestCollectionUniqueID"/>
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
			Guard.ArgumentNotNull(nameof(assemblyUniqueID), assemblyUniqueID);
			Guard.ArgumentNotNull(nameof(collectionDisplayName), collectionDisplayName);

			using var generator = new UniqueIDGenerator();
			generator.Add(assemblyUniqueID);
			generator.Add(collectionDisplayName);
			generator.Add(collectionDefinitionClassName ?? string.Empty);
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
}

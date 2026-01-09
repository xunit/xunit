using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class FileSystem : IFileSystem
{
	FileSystem()
	{ }

	/// <summary/>
	public static FileSystem Instance { get; } = new();

	/// <summary/>
	public void CreateDirectory(string path) =>
		Directory.CreateDirectory(path);

	/// <summary/>
	public bool Exists([NotNullWhen(true)] string? path) =>
		File.Exists(path);

	/// <summary/>
	public byte[] ReadAllBytes(string path) =>
		File.ReadAllBytes(path);

	/// <summary/>
	public string ReadAllText(string path) =>
		File.ReadAllText(path);

	/// <summary/>
	public void WriteAllBytes(string path, byte[] bytes) =>
		File.WriteAllBytes(path, bytes);

	/// <summary/>
	public void WriteAllText(string path, string text) =>
		File.WriteAllText(path, text);
}

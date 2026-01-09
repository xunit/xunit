using System.Diagnostics.CodeAnalysis;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL INTERFACE. DO NOT USE.
/// </summary>
public interface IFileSystem
{
	/// <summary/>
	void CreateDirectory(string path);

	/// <summary/>
	bool Exists([NotNullWhen(true)] string? path);

	/// <summary/>
	byte[] ReadAllBytes(string path);

	/// <summary/>
	string ReadAllText(string path);

	/// <summary/>
	void WriteAllBytes(string path, byte[] bytes);

	/// <summary/>
	void WriteAllText(string path, string text);
}

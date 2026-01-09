using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace Xunit.Internal;

public class SpyFileSystem : IFileSystem
{
	readonly ConcurrentDictionary<string, byte[]> files = new(StringComparer.InvariantCultureIgnoreCase);

	public void CreateDirectory(string path)
	{ }

	public bool Exists(string? path) =>
		path is not null && files.ContainsKey(path);

	public byte[] ReadAllBytes(string path)
	{
		if (!files.TryGetValue(path, out var bytes))
			throw new FileNotFoundException();

		return bytes;
	}

	public string ReadAllText(string path)
	{
		if (!files.TryGetValue(path, out var bytes))
			throw new FileNotFoundException();

		return Encoding.UTF8.GetString(bytes);
	}

	public void WriteAllBytes(string path, byte[] bytes) =>
		files.TryAdd(path, bytes);

	public void WriteAllText(string path, string text) =>
		files.TryAdd(path, Encoding.UTF8.GetBytes(text));
}

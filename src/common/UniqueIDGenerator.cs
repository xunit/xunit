using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class UniqueIDGenerator : IDisposable
{
	bool disposed;
	HashAlgorithm hasher;
	Stream stream;

	public UniqueIDGenerator()
	{
		hasher = SHA256.Create();
		stream = new MemoryStream();
	}

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

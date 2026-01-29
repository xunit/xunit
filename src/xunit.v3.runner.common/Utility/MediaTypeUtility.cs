using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace Xunit;

/// <summary>
/// This class provides mappings from media types (stored in attachments) to the intended file system extension
/// when storing the attachment on disk.
/// </summary>
public static class MediaTypeUtility
{
	const string DefaultExtension = ".bin";
	static readonly HashSet<char> InvalidFileNameChars = [.. Path.GetInvalidFileNameChars()];

	static readonly Dictionary<string, string> mediaTypeMappings = new(StringComparer.OrdinalIgnoreCase)
	{
		["audio/aac"] = ".aac",
		["application/x-abiword"] = ".abw",
		["image/apng"] = ".apng",
		["application/x-freearc"] = ".arc",
		["image/avif"] = ".avif",
		["video/x-msvideo"] = ".avi",
		["application/vnd.amazon.ebook"] = ".azw",
		["application/octet-stream"] = ".bin",
		["image/bmp"] = ".bmp",
		["application/x-bzip"] = ".bz",
		["application/x-bzip2"] = ".bz2",
		["application/x-cdf"] = ".cda",
		["application/x-csh"] = ".csh",
		["text/css"] = ".css",
		["text/csv"] = ".csv",
		["application/msword"] = ".doc",
		["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = ".docx",
		["application/vnd.ms-fontobject"] = ".eot",
		["application/epub+zip"] = ".epub",
		["application/gzip"] = ".gz",
		["application/x-gzip"] = ".gz",
		["image/gif"] = ".gif",
		["text/html"] = ".html",
		["image/vnd.microsoft.icon"] = ".ico",
		["text/calendar"] = ".ics",
		["application/java-archive"] = ".jar",
		["image/jpeg"] = ".jpg",
		["text/javascript"] = ".js",
		["application/json"] = ".json",
		["application/ld+json"] = ".jsonld",
		["audio/midi"] = ".mid",
		["audio/x-midi"] = ".mid",
		["audio/mpeg"] = ".mp3",
		["video/mp4"] = ".mp4",
		["video/mpeg"] = ".mpeg",
		["application/vnd.apple.installer+xml"] = ".mpkg",
		["application/vnd.oasis.opendocument.presentation"] = ".odp",
		["application/vnd.oasis.opendocument.spreadsheet"] = ".ods",
		["application/vnd.oasis.opendocument.text"] = ".odt",
		["audio/ogg"] = ".oga",
		["video/ogg"] = ".ogv",
		["application/ogg"] = ".ogx",
		["font/otf"] = ".otf",
		["image/png"] = ".png",
		["application/pdf"] = ".pdf",
		["application/x-httpd-php"] = ".php",
		["application/vnd.ms-powerpoint"] = ".ppt",
		["application/vnd.openxmlformats-officedocument.presentationml.presentation"] = ".pptx",
		["application/vnd.rar"] = ".rar",
		["application/rtf"] = ".rtf",
		["application/x-sh"] = ".sh",
		["image/svg+xml"] = ".svg",
		["application/x-tar"] = ".tar",
		["image/tiff"] = ".tif",
		["video/mp2t"] = ".ts",
		["font/ttf"] = ".ttf",
		["text/plain"] = ".txt",
		["application/vnd.visio"] = ".vsd",
		["audio/wav"] = ".wav",
		["audio/webm"] = ".weba",
		["video/webm"] = ".webv",
		["font/woff"] = ".woff",
		["font/woff2"] = ".woff2",
		["application/xhtml+xml"] = ".xhtml",
		["application/vnd.ms-excel"] = ".xls",
		["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = ".xlsx",
		["application/xml"] = ".xml",
		["text/xml"] = ".xml",
		["application/vnd.mozilla.xul+xml"] = ".xul",
		["application/zip"] = ".zip",
		["application/x-zip-compressed"] = ".zip",
		["audio/3gpp"] = ".3gp",
		["video/3gpp"] = ".3gp",
		["audio/3gpp2"] = ".3g2",
		["video/3gpp2"] = ".3g2",
		["application/x-7z-compressed"] = ".7z",
	};

	/// <summary>
	/// Gets the file extension for the given media type. If the media type is unknown, it will return <c>.bin</c>.
	/// </summary>
	/// <param name="mediaType">The media type to get the file extension from</param>
	/// <remarks>
	/// The media type list is pre-populated with media types from <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/MIME_types/Common_types"/>.
	/// On Windows, an unknown media type will attempt to find the file extension in the system registry, under
	/// <c>HKEY_CLASSES_ROOT\MIME\Database\Content Type</c>; on non-Windows OSes, there is no fallback.
	/// The pre-populated cache was last populated on 2025 February 5.
	/// </remarks>
	public static string GetFileExtension(string mediaType)
	{
		if (!mediaTypeMappings.TryGetValue(mediaType, out var extension))
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				extension = Registry.ClassesRoot.OpenSubKey($"MIME\\Database\\Content Type\\{mediaType}")?.GetValue("Extension") as string;

			extension ??= DefaultExtension;
			mediaTypeMappings[mediaType] = extension;
		}

		return extension;
	}

	/// <summary>
	/// This takes a filename that may contain illegal characters (per <see cref="Path.GetInvalidFileNameChars"/>),
	/// replaces them with underscores, and ensures that the file extension matches the provided MIME media type.
	/// If the media type does not have a known extension, <c>.bin</c> will be used.
	/// </summary>
	/// <param name="fileName">The filename</param>
	/// <param name="mediaType">The media type</param>
	/// <returns>The sanitized filename with the correct file extension applied</returns>
	/// <remarks>
	/// The media type list is pre-populated with media types from <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/MIME_types/Common_types"/>.
	/// On Windows, an unknown media type will attempt to find the file extension in the system registry, under
	/// <c>HKEY_CLASSES_ROOT\MIME\Database\Content Type</c>; on non-Windows OSes, there is no fallback.
	/// The pre-populated cache was last populated on 2025 February 5.
	/// </remarks>
	public static string GetSanitizedFileNameWithExtension(
		string fileName,
		string mediaType)
	{
		Guard.ArgumentNotNull(fileName);

		var builder = new StringBuilder(fileName.Length);

		foreach (var c in fileName)
			if (InvalidFileNameChars.Contains(c))
				builder.Append('_');
			else
				builder.Append(c);

		var result = builder.ToString();
		var fileExtension = GetFileExtension(mediaType);

		if (Path.GetExtension(result) != fileExtension)
			result += fileExtension;

		return result;
	}
}

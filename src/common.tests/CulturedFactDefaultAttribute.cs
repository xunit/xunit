using System.Runtime.CompilerServices;

namespace Xunit;

public class CulturedFactDefaultAttribute(
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		CulturedFactAttribute(["en-US", "fr-FR"], sourceFilePath, sourceLineNumber)
{ }

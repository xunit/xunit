using System.Runtime.CompilerServices;

namespace Xunit;

public class CulturedTheoryDefaultAttribute(
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		CulturedTheoryAttribute(["en-US", "fr-FR"], sourceFilePath, sourceLineNumber)
{ }

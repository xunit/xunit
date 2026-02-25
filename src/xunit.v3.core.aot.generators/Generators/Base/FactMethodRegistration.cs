namespace Xunit.Generators;

public record FactMethodRegistration(
	string MethodName,
	CodeGenTestMethodRegistration TestMethod,
	string TestCaseFactory
);

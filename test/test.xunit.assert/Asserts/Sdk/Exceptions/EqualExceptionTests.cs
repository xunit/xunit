using System;
using System.Linq;
using Xunit;

public class EqualExceptionTests
{
    [Fact]
    public void OneStringAddsValueToEndOfTheOtherString()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                    ↓ (pos 10)" + Environment.NewLine +
            "Expected: first test 1" + Environment.NewLine +
            "Actual:   first test" + Environment.NewLine +
            "                    ↑ (pos 10)";

        var ex = Record.Exception(() => Assert.Equal("first test 1", "first test"));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void OneStringOneNullDoesNotShowDifferencePoint()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "Expected: first test 1" + Environment.NewLine +
            "Actual:   (null)";

        var ex = Record.Exception(() => Assert.Equal("first test 1", null));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void StringsDifferInTheMiddle()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                ↓ (pos 6)" + Environment.NewLine +
            "Expected: first failure" + Environment.NewLine +
            "Actual:   first test" + Environment.NewLine +
            "                ↑ (pos 6)";

        var ex = Record.Exception(() => Assert.Equal("first failure", "first test"));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableOneEmptyCollectionShowsDifferencePoint()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "           ↓ (pos 0)" + Environment.NewLine +
            "Expected: [a, b]" + Environment.NewLine +
            "Actual:   []" + Environment.NewLine +
            "           ↑ (pos 0)";

        var ex = Record.Exception(() =>
            Assert.Equal(new[] { "a", "b" }, Enumerable.Empty<string>()));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableDifferInTheBeginning()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                 ↓ (pos 2)" + Environment.NewLine +
            "Expected: [a, b, c, d, e, f, ...]" + Environment.NewLine +
            "Actual:   [a, b, cXX, d, e, f, ...]" + Environment.NewLine +
            "                 ↑ (pos 2)";

        var ex = Record.Exception(() => 
            Assert.Equal(new[]{ "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k" }, 
                          new []{ "a", "b", "cXX", "d", "e", "f", "g", "h", "i", "j", "k" }));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableDifferInTheMiddle()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                      ↓ (pos 5)" + Environment.NewLine +
            "Expected: [..., d, e, f, g, h, i, ...]" + Environment.NewLine +
            "Actual:   [..., d, e, fXX, g, h, i, ...]" + Environment.NewLine +
            "                      ↑ (pos 5)";

        var ex = Record.Exception(() =>
            Assert.Equal(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k" },
                new[] { "a", "b", "c", "d", "e", "fXX", "g", "h", "i", "j", "k" }));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableDifferInTheEnd()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                      ↓ (pos 9)" + Environment.NewLine +
            "Expected: [..., h, i, j, k]" + Environment.NewLine +
            "Actual:   [..., h, i, jXX, k]" + Environment.NewLine +
            "                      ↑ (pos 9)";

        var ex = Record.Exception(() =>
            Assert.Equal(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k" },
                new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "jXX", "k" }));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableDifferInLength()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                      ↓ (pos 9)" + Environment.NewLine +
            "Expected: [..., h, i, j, k]" + Environment.NewLine +
            "Actual:   [..., h, i]" + Environment.NewLine +
            "                      ↑ (pos 9)";

        var ex = Record.Exception(() =>
            Assert.Equal(new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k" },
                new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" }));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableWithComplexDataTypesDifferInTheMiddle()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                ↓ (pos 1)" + Environment.NewLine +
            "Expected: [..., { Name = John, IsFailure = False }, { Name = John, IsFailure = False }]" + Environment.NewLine +
            "Actual:   [..., { Name = John, IsFailure = True }, { Name = John, IsFailure = False }]" + Environment.NewLine +
            "                ↑ (pos 1)";

        var enumerableX = new[]
        {
            new {Name = "John", IsFailure = false},
            new {Name = "John", IsFailure = false},
            new {Name = "John", IsFailure = false}
        };

        var enumerableY = new[]
        {
            new {Name = "John", IsFailure = false},
            new {Name = "John", IsFailure = true},
            new {Name = "John", IsFailure = false}
        };

        var ex = Record.Exception(() =>
            Assert.Equal(enumerableX, enumerableY));

        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public void EnumerableWithObjectExceeding100Characters()
    {
        string expectedMessage =
            "Assert.Equal() Failure" + Environment.NewLine +
            "                ↓ (pos 1)" + Environment.NewLine +
            "Expected: [..., { Name = John, LastName = Doe, Company = ACME, Address = 123 Main Street, Unit 21, IsFailure...]" + Environment.NewLine +
            "Actual:   [..., { Name = John, LastName = Doe, Company = ACME, Address = 123 Main Street, Unit 21, IsFailure...]" + Environment.NewLine +
            "                ↑ (pos 1)";

        var enumerableX = new[]
        {
            new {Name = "John", LastName = "Doe", Company = "ACME", Address = "123 Main Street, Unit 21", IsFailure = false},
            new {Name = "John", LastName = "Doe", Company = "ACME", Address = "123 Main Street, Unit 21", IsFailure = false}
        };

        var enumerableY = new[]
        {
            new {Name = "John", LastName = "Doe", Company = "ACME", Address = "123 Main Street, Unit 21", IsFailure = false},
            new {Name = "John", LastName = "Doe", Company = "ACME", Address = "123 Main Street, Unit 21", IsFailure = true}
        };

        var ex = Record.Exception(() =>
            Assert.Equal(enumerableX, enumerableY));

        Assert.Equal(expectedMessage, ex.Message);
    }
}

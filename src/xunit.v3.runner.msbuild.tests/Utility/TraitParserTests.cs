using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.MSBuild;

public class TraitParserTests
{
    public class Parse
    {
        [Fact]
        public void ReturnsEmptyWhenNull()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse(null, traits);

            Assert.Empty(traits);
        }

        [Fact]
        public void ReturnsEmptyWhenEmpty()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse(string.Empty, traits);

            Assert.Empty(traits);
        }

        [Fact]
        public void ReturnsTraits()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("One=1;Two=2", traits);

            Assert.Collection(traits.Keys,
                key =>
                {
                    Assert.Equal("One", key);
                    Assert.Equal("1", Assert.Single(traits[key]));
                },
                key =>
                {
                    Assert.Equal("Two", key);
                    Assert.Equal("2", Assert.Single(traits[key]));
                });
        }

        [Fact]
        public void IgnoresExtraTraitSeperatorsAndWhitespace()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("; One = 1 ;;", traits);

            Assert.Collection(traits.Keys,
                key =>
                {
                    Assert.Equal("One", key);
                    Assert.Equal("1", Assert.Single(traits[key]));
                });
        }

        [Fact]
        public void IncludesExtraKeyValueSeperatorsInValue()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("One=1=2=3", traits);

            Assert.Collection(traits.Keys,
                key =>
                {
                    Assert.Equal("One", key);
                    Assert.Equal("1=2=3", Assert.Single(traits[key]));
                });
        }

        [Fact]
        public void IgnoresMissingKeyValueSeperator()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("One1", traits);

            Assert.Empty(traits);
        }

        [Fact]
        public void IgnoresMissingKey()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("=1", traits);

            Assert.Empty(traits);
        }

        [Fact]
        public void IgnoresMissingValue()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("1=", traits);

            Assert.Empty(traits);
        }

        [Fact]
        public void ContinuesOnError()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            new TraitParser().Parse("One;Two=2", traits);

            Assert.Collection(traits.Keys,
                key =>
                {
                    Assert.Equal("Two", key);
                    Assert.Equal("2", Assert.Single(traits[key]));
                });
        }

        [Fact]
        public void RaisesWarningOnError()
        {
            var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var messages = new List<string>();
            var parser = new TraitParser(messages.Add);

            parser.Parse("One1", traits);

            Assert.Collection(messages,
                msg => Assert.Equal("Invalid trait 'One1'. The format should be 'name=value'. This trait will be ignored.", msg));
        }
    }
}

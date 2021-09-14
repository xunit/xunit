using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Xunit1
{
    public class MultiValueDictionaryTests
    {
        [Fact]
        public void EmptyDictionary()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            Assert.Equal(0, dictionary.Keys.Count());
        }

        [Fact]
        public void RetrievingUnknownKeyThrows()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            Assert.Throws<KeyNotFoundException>(() => dictionary["foo"]);
        }

        [Fact]
        public void AddSingleValue()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            dictionary.AddValue("Key", "Value");

            Assert.Contains("Key", dictionary.Keys);
            Assert.Contains("Value", dictionary["Key"]);
        }

        [Fact]
        public void AddTwoValuesForSameKey()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            dictionary.AddValue("Key", "Value1");
            dictionary.AddValue("Key", "Value2");

            Assert.Contains("Key", dictionary.Keys);
            IEnumerable<string> values = dictionary["Key"];
            Assert.Contains("Value1", values);
            Assert.Contains("Value2", values);
        }

        [Fact]
        public void AddSameValueForSameKeyDoesNotDuplicateValue()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            dictionary.AddValue("Key", "Value1");
            dictionary.AddValue("Key", "Value1");

            Assert.Contains("Key", dictionary.Keys);
            IEnumerable<string> values = dictionary["Key"];
            Assert.Single(values);
            Assert.Contains("Value1", values);
        }

        [Fact]
        public void RemoveKey()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();
            dictionary.AddValue("Key", "Value");

            dictionary.Remove("Key");

            Assert.DoesNotContain("Key", dictionary.Keys);
        }

        [Fact]
        public void RemoveKeyForUnknownKeyDoesNotThrow()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            Assert.DoesNotThrow(() => dictionary.Remove("Key"));
        }

        [Fact]
        public void RemoveValue()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();
            dictionary.AddValue("Key", "Value1");
            dictionary.AddValue("Key", "Value2");

            dictionary.RemoveValue("Key", "Value1");

            Assert.Contains("Key", dictionary.Keys);
            IEnumerable<string> values = dictionary["Key"];
            Assert.DoesNotContain("Value1", values);
            Assert.Contains("Value2", values);
        }

        [Fact]
        public void RemoveValueForUnknownKeyDoesNotThrow()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();

            Assert.DoesNotThrow(() => dictionary.RemoveValue("Key", "Value1"));
        }

        [Fact]
        public void RemoveValueForUnknownValueDoesNotThrow()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();
            dictionary.AddValue("Key", "Value1");

            Assert.DoesNotThrow(() => dictionary.RemoveValue("Key", "Value2"));
        }

        [Fact]
        public void RemovingLastValueFromKeyRemovesKey()
        {
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();
            dictionary.AddValue("Key", "Value1");

            dictionary.RemoveValue("Key", "Value1");

            Assert.DoesNotContain("Key", dictionary.Keys);
        }

        [Fact]
        public void CanEnumerateKeysAndValuesWithDelegate()
        {
            string result = "";
            var dictionary = new Xunit.Sdk.MultiValueDictionary<string, string>();
            dictionary.AddValue("Key1", "Value1");
            dictionary.AddValue("Key2", "Value2");
            dictionary.AddValue("Key2", "Value1");
            dictionary.AddValue("Key3", "Value7");

            dictionary.ForEach((key, value) => result += key + ": " + value + "\r\n");

            Assert.Equal("Key1: Value1\r\n" +
                         "Key2: Value2\r\n" +
                         "Key2: Value1\r\n" +
                         "Key3: Value7\r\n", result);
        }
    }
}

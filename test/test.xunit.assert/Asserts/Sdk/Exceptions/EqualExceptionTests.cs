using System;
using System.Collections.Generic;
using Xunit;

public class EqualExceptionTests
{
    public class StringTests
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
    }


    public class IEnumerableTests
    {
        //NOTE: for tests for the ienumerables containing strings, the ↓ and ↑ won't look accurate in the expected messages as escaping the quotes in the collection
        //causes the actual and expected values to look longer than they are when run

        [Fact]
        public void LongArrayErrorIndicatesIndexStart()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"           ↓ (pos 0)" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3"", ""4"", ""5"", ...]" + Environment.NewLine +
                @"Actual:   [""Rubbish"", ""2"", ""3"", ""4"", ""5"", ...]" + Environment.NewLine +
                @"           ↑ (pos 0)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                new string[] { "Rubbish", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void LongArrayErrorIndicatesIndexMiddle()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"                            ↓ (pos 15)" + Environment.NewLine +
                @"Expected: [..., ""14"", ""15"", ""16"", ""17"", ""18"", ...]" + Environment.NewLine +
                @"Actual:   [..., ""14"", ""15"", ""RUBBISH"", ""17"", ""18"", ...]" + Environment.NewLine +
                @"                            ↑ (pos 15)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "RUBBISH", "17", "18", "19", "20" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void LongArrayErrorIndicatesIndexEnd()
        {//NOTE the string shortener is shortening the output here for some reason, needs more investigation
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"                                 ↓ (pos 19)" + Environment.NewLine +
                @"Expected: ···, ""17"", ""18"", ""19"", ""20""]" + Environment.NewLine +
                @"Actual:   ···, ""17"", ""18"", ""19"", ""Rubbish""]" + Environment.NewLine +
                @"                                 ↑ (pos 19)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" },
                new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "Rubbish" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ShortArrayErrorIndicatesIndexStart()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"           ↓ (pos 0)" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3"", ""4""]" + Environment.NewLine +
                @"Actual:   [""Rubbish"", ""2"", ""3"", ""4""]" + Environment.NewLine +
                @"           ↑ (pos 0)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4" },
                new string[] { "Rubbish", "2", "3", "4" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ShortArrayErrorIndicatesIndexMiddle()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"                     ↓ (pos 2)" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3"", ""4""]" + Environment.NewLine +
                @"Actual:   [""1"", ""2"", ""Rubbish"", ""4""]" + Environment.NewLine +
                @"                     ↑ (pos 2)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4" },
                new string[] { "1", "2", "Rubbish", "4" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ShortArrayErrorIndicatesIndexEnd()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"                          ↓ (pos 3)" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3"", ""4""]" + Environment.NewLine +
                @"Actual:   [""1"", ""2"", ""3"", ""Rubbish""]" + Environment.NewLine +
                @"                          ↑ (pos 3)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "4" },
                new string[] { "1", "2", "3", "Rubbish" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExpectedShorter()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3""]" + Environment.NewLine +
                @"Actual:   [""1"", ""2"", ""3"", ""Rubbish""]" + Environment.NewLine +
                @"                          ↑ (pos 3)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3" },
                new string[] { "1", "2", "3", "Rubbish" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExpectedLonger()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"                          ↓ (pos 3)" + Environment.NewLine +
                @"Expected: [""1"", ""2"", ""3"", ""Rubbish""]" + Environment.NewLine +
                @"Actual:   [""1"", ""2"", ""3""]";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3", "Rubbish" },
                new string[] { "1", "2", "3" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExpectedNull()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"Expected: (null)" + Environment.NewLine +
                @"Actual:   String[] [""1"", ""2"", ""3""]";
            var ex = Record.Exception(() => Assert.Equal(
                null,
                new string[] { "1", "2", "3" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ActualNull()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"Expected: String[] [""1"", ""2"", ""3""]" + Environment.NewLine +
                @"Actual:   (null)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1", "2", "3" },
                null));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void JustOneValue()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"           ↓ (pos 0)" + Environment.NewLine +
                @"Expected: [""1""]" + Environment.NewLine +
                @"Actual:   [""Rubbish""]" + Environment.NewLine +
                @"           ↑ (pos 0)";
            var ex = Record.Exception(() => Assert.Equal(
                new string[] { "1" },
                new string[] { "Rubbish" }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExpectedShorterWithIntArray()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"Expected: [1, 2, 3]" + Environment.NewLine +
                @"Actual:   [1, 2, 3, 4]" + Environment.NewLine +
                @"                    ↑ (pos 3)";
            var ex = Record.Exception(() => Assert.Equal(
                new int[] { 1, 2, 3 },
                new int[] { 1, 2, 3, 4 }));
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void ExpectedShorterWithIntList()
        {
            string expectedMessage =
                @"Assert.Equal() Failure" + Environment.NewLine +
                @"Expected: [1, 2, 3]" + Environment.NewLine +
                @"Actual:   [1, 2, 3, 4]" + Environment.NewLine +
                @"                    ↑ (pos 3)";
            var ex = Record.Exception(() => Assert.Equal(
                new List<int>() { 1, 2, 3 },
                new List<int>() { 1, 2, 3, 4 }));
            Assert.Equal(expectedMessage, ex.Message);
        }
    }
}

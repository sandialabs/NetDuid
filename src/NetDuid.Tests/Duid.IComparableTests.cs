using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region Compare(Duid)

        public static IEnumerable<object[]> Compare_Duid_Test_TestCases()
        {
            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });
            yield return TestCase(0, referenceDuid, referenceDuid); // by reference
            yield return TestCase(-1, referenceDuid, null); // null case

            object[] TestCase(int expectedSign, Duid thisDuid, Duid otherDuid)
            {
                return new object[] { expectedSign, thisDuid, otherDuid };
            }

            var deepTestCases = new List<IEnumerable<object[]>>
            {
                // equality
                TestCases(0, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 }),
                TestCases(0, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 }),
                // by length
                TestCases(-1, new byte[3], new byte[4]),
                TestCases(-1, new byte[3], new byte[130]),
                // by length, then big endian value
                TestCases(-1, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
                TestCases(-1, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 }),
                TestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 }),
                TestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 }),
            }.SelectMany(tc => tc);

            foreach (var testCase in deepTestCases)
            {
                yield return testCase;
            }

            IEnumerable<object[]> TestCases(int expectedSign, byte[] thisBytes, byte[] otherBytes)
            {
                yield return TestCase(expectedSign, new Duid(thisBytes), new Duid(otherBytes));

                if (expectedSign != 0)
                {
                    // create transitive test case
                    yield return TestCase(expectedSign * -1, new Duid(otherBytes), new Duid(thisBytes));
                }
            }
        }

        [Theory]
        [MemberData(nameof(Compare_Duid_Test_TestCases))]
        public void Compare_Duid_Test(int expectedSign, Duid thisDuid, Duid otherDuid)
        {
            // Arrange
            // Act
            var result = thisDuid.CompareTo(otherDuid);

            // Assert
            if (expectedSign == 0)
            {
                // expected equivalent
                Assert.Equal(0, result);
            }
            else if (expectedSign > 0)
            {
                // expected this after other
                Assert.True(result > 0);
            }
            else if (expectedSign < 0)
            {
                // expected this before other
                Assert.True(result < 0);
            }
        }

        [Theory]
        [MemberData(nameof(Compare_Duid_Test_TestCases))]
        public void Compare_Object_Test(int expectedSign, Duid thisDuid, object other)
        {
            // Arrange
            // Act
            var result = thisDuid.CompareTo(other);

            // Assert

            if (expectedSign == 0)
            {
                // expected equivalent
                Assert.Equal(0, result);
            }
            else if (expectedSign > 0)
            {
                // expected this after other
                Assert.True(result > 0);
            }
            else if (expectedSign < 0)
            {
                // expected this before other
                Assert.True(result < 0);
            }
        }

        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        public void Compare_Object_NotDuid_Throws_ArgumentException_Test(object other)
        {
            // Arrange
            var duid = new Duid(new byte[] { 0x00, 0x00, 0xff });

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => duid.CompareTo(other));
        }

        #endregion
    }
}

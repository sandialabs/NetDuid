using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region Equal(Duid)

        public static IEnumerable<object[]> Equal_Duid_Test_TestCases()
        {
            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });
            yield return TestCase(true, referenceDuid, referenceDuid); // by reference
            yield return TestCase(false, referenceDuid, null); // null case

            object[] TestCase(bool expectedEqual, Duid thisDuid, Duid otherDuid)
            {
                return new object[] { expectedEqual, thisDuid, otherDuid };
            }

            var deepTestCases = new List<IEnumerable<object[]>>
            {
                // equality
                TestCases(true, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 }),
                TestCases(true, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 }),
                TestCases(false, new byte[3], new byte[4]),
                TestCases(false, new byte[3], new byte[130]),
                TestCases(false, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
                TestCases(false, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 }),
                TestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 }),
                TestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 }),
            }.SelectMany(tc => tc);

            foreach (var testCase in deepTestCases)
            {
                yield return testCase;
            }

            IEnumerable<object[]> TestCases(bool expectedEqual, byte[] thisBytes, byte[] otherBytes)
            {
                yield return TestCase(expectedEqual, new Duid(thisBytes), new Duid(otherBytes));

                if (!expectedEqual)
                {
                    // create transitive test case
                    yield return TestCase(expectedEqual, new Duid(otherBytes), new Duid(thisBytes));
                }
            }
        }

        [Theory]
        [MemberData(nameof(Equal_Duid_Test_TestCases))]
        public void Equal_Duid_Test(bool expectedEqual, Duid thisDuid, Duid otherDuid)
        {
            // Arrange
            // Act
            var result = thisDuid.Equals(otherDuid);

            // Assert
            Assert.Equal(expectedEqual, result);

            if (expectedEqual)
            {
                Assert.Equal(thisDuid.GetHashCode(), otherDuid.GetHashCode());
            }
        }

        [Theory]
        [MemberData(nameof(Equal_Duid_Test_TestCases))]
        public void Equal_Object_Test(bool expectedEqual, Duid thisDuid, object otherDuid)
        {
            // Arrange
            // Act
            var result = thisDuid.Equals(otherDuid);

            // Assert
            Assert.Equal(expectedEqual, result);
        }

        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(null)]
        public void Equal_Object_NotDuid_Throws_ArgumentException_Test(object other)
        {
            // Arrange
            var duid = new Duid(new byte[] { 0x00, 0x00, 0xff });

            // Act
            var result = duid.Equals(other);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetHashCode
        public static IEnumerable<object[]> HashCode_Test_TestCases()
        {
            for (var byteCount = 3; byteCount <= 130; byteCount += 5)
            {
                yield return new object[] { StaticTestData.GenerateBytes(byteCount, byteCount) };
            }
        }

        [Theory]
        [MemberData(nameof(HashCode_Test_TestCases))]
        public void HashCode_EquivalentDuid_HashEqual_Test(byte[] duidBytes)
        {
            // Arrange
            var duidA = new Duid(duidBytes);
            var duidB = new Duid(duidBytes);

            // Act
            // Assert
            Assert.Equal(duidA.GetHashCode(), duidB.GetHashCode());
        }
        #endregion
    }
}

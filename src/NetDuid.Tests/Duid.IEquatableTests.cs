using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region Equal(Duid)

        public static TheoryData<bool, Duid, Duid> Equal_Duid_Test_TestCases()
        {
            var theoryData = new TheoryData<bool, Duid, Duid>();

            // reference equality
            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });
            AddTestCase(true, referenceDuid, referenceDuid); // by reference
            AddTestCase(false, referenceDuid, null); // null case

            // equality
            AddCommutativeTestCases(true, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 });
            AddCommutativeTestCases(true, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 });

            // inequality
            AddCommutativeTestCases(false, new byte[3], new byte[4]);
            AddCommutativeTestCases(false, new byte[3], new byte[130]);
            AddCommutativeTestCases(false, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            AddCommutativeTestCases(false, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 });
            AddCommutativeTestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 });
            AddCommutativeTestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 });

            return theoryData;

            void AddTestCase(bool expectedEqual, Duid thisDuid, Duid otherDuid)
            {
                theoryData.Add(expectedEqual, thisDuid, otherDuid);
            }

            void AddCommutativeTestCases(bool expectedEqual, byte[] thisBytes, byte[] otherBytes)
            {
                AddTestCase(expectedEqual, new Duid(thisBytes), new Duid(otherBytes));
                AddTestCase(expectedEqual, new Duid(otherBytes), new Duid(thisBytes)); // Commutative test case form
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
        public void Equal_Object_Test(bool expectedEqual, Duid thisDuid, Duid otherDuid)
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
        public static TheoryData<byte[]> HashCode_Test_TestCases()
        {
            var theoryData = new TheoryData<byte[]>();

            for (var byteCount = 3; byteCount <= 130; byteCount += 5)
            {
                theoryData.Add(StaticTestData.GenerateBytes(byteCount, byteCount));
            }

            return theoryData;
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

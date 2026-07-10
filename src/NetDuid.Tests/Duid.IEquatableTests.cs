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
            var referenceDuid = new Duid([0x00, 0x00, 0xff]);
            AddTestCase(true, referenceDuid, referenceDuid); // by reference
            AddTestCase(false, referenceDuid, null); // null case

            // equality
            AddCommutativeTestCases(true, [0x00, 0x00, 0x00], [0x00, 0x00, 0x00]);
            AddCommutativeTestCases(true, [0xFF, 0x00, 0x80], [0xFF, 0x00, 0x80]);

            // inequality
            AddCommutativeTestCases(false, new byte[3], new byte[4]);
            AddCommutativeTestCases(false, new byte[3], new byte[130]);
            AddCommutativeTestCases(false, [0xFF, 0x00, 0x00], [0xFF, 0xFF, 0xFF, 0xFF]);
            AddCommutativeTestCases(false, [0xFF, 0xFF, 0xFF], [0xFF, 0x00, 0x00, 0x00]);
            AddCommutativeTestCases(false, [0x00, 0x00, 0x00], [0x00, 0x00, 0x01]);
            AddCommutativeTestCases(false, [0x00, 0x00, 0x00], [0x80, 0x00, 0x00]);

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
        public void Equal_Object_NotDuid_Returns_False_Test(object other)
        {
            // Arrange
            var duid = new Duid([0x00, 0x00, 0xff]);

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

        [Fact]
        public void HashCode_SameInstance_ReturnsStableHash_Test()
        {
            // Arrange
            var duid = new Duid([0x00, 0x01, 0x02, 0x03]);

            // Act
            var hash1 = duid.GetHashCode();
            var hash2 = duid.GetHashCode();
            var hash3 = duid.GetHashCode();

            // Assert
            Assert.Equal(hash1, hash2);
            Assert.Equal(hash2, hash3);
        }

        [Theory]
        [InlineData(new byte[] { 0x00, 0x01, 0x02 }, new byte[] { 0x00, 0x01, 0x03 })]
        [InlineData(new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF })]
        [InlineData(new byte[] { 0x00, 0x01, 0x02 }, new byte[] { 0x00, 0x01, 0x02, 0x03 })]
        public void HashCode_DifferentDuids_DifferentHashes_Test(byte[] bytesA, byte[] bytesB)
        {
            // Arrange
            var duidA = new Duid(bytesA);
            var duidB = new Duid(bytesB);

            // Act
            var hashA = duidA.GetHashCode();
            var hashB = duidB.GetHashCode();

            // Assert
            Assert.NotEqual(hashA, hashB);
        }
        #endregion
    }
}

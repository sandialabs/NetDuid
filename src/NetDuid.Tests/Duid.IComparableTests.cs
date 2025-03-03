using System;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region Compare(Duid)

        public static TheoryData<int, Duid, Duid> Compare_Duid_Test_TestCases()
        {
            var theoryData = new TheoryData<int, Duid, Duid>();

            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });

            AddTestCase(0, referenceDuid, referenceDuid); // by reference
            AddTestCase(-1, referenceDuid, null); // null case

            // equality
            AddCommutativeTestCases(0, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 });
            AddCommutativeTestCases(0, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 });

            // by length
            AddCommutativeTestCases(-1, new byte[3], new byte[4]);
            AddCommutativeTestCases(-1, new byte[3], new byte[130]);

            // by length, then big endian value
            AddCommutativeTestCases(-1, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
            AddCommutativeTestCases(-1, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 });
            AddCommutativeTestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 });
            AddCommutativeTestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 });

            return theoryData;

            void AddTestCase(int expectedSign, Duid thisDuid, Duid otherDuid)
            {
                theoryData.Add(expectedSign, thisDuid, otherDuid);
            }

            void AddCommutativeTestCases(int expectedSign, byte[] thisBytes, byte[] otherBytes)
            {
                AddTestCase(expectedSign, new Duid(thisBytes), new Duid(otherBytes));
                AddTestCase(expectedSign * -1, new Duid(otherBytes), new Duid(thisBytes)); // Commutative test case form
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

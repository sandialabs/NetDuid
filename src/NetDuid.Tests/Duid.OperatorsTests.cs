using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region Operators

        #region Equality Operator

        [Theory]
        [MemberData(nameof(Equal_Duid_Test_TestCases))]
        public void Equality_Operator_Duid_Test(bool expectedEqual, Duid thisDuid, Duid otherDuid)
        {
            // Arrange
            // Act
            var result = thisDuid == otherDuid;

            // Assert
            Assert.Equal(expectedEqual, result);
        }

        #endregion

        #region Inequality Operator

        [Theory]
        [MemberData(nameof(Equal_Duid_Test_TestCases))]
        public void Inequality_Operator_Duid_Test(bool expectedEqual, Duid thisDuid, Duid otherDuid)
        {
            // Arrange
            // Act
            var result = thisDuid != otherDuid;

            // Assert
            Assert.Equal(!expectedEqual, result);
        }

        #endregion

        #region Null Equality

        [Fact]
        public void Equality_Operator_BothNull_ReturnsTrue_Test()
        {
            // Arrange
            Duid lhs = null;
            Duid rhs = null;

            // Act
            var result = lhs == rhs;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Inequality_Operator_BothNull_ReturnsFalse_Test()
        {
            // Arrange
            Duid lhs = null;
            Duid rhs = null;

            // Act
            var result = lhs != rhs;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Comparison Operators

        public static TheoryData<int, Duid, Duid> Operators_Duid_Test_TestCases()
        {
            var theoryData = new TheoryData<int, Duid, Duid>();

            var zeroArray = new byte[] { 0x00, 0x00, 0x00 };

            var nonNullDuid = new Duid(zeroArray);

            TestCase(0, nonNullDuid, nonNullDuid); // reference equal
            TestCase(0, nonNullDuid, new Duid(zeroArray)); // non-reference equal
            TestCase(0, null, null); // both null equal

            // single null operand (null less than non-null)
            TestCase(1, nonNullDuid, null);
            TestCase(-1, null, nonNullDuid);

            // non-null operands (manually computed: length-first, then byte-by-byte)
            var otherDuidBytes = new byte[] { 0xFF, 0x00, 0x80 };
            var otherDuid = new Duid(otherDuidBytes);

            // both same length (3), first byte: 0x00 < 0xFF → nonNullDuid < otherDuid
            TestCase(-1, nonNullDuid, otherDuid);
            TestCase(1, otherDuid, nonNullDuid);
            TestCase(0, otherDuid, otherDuid);

            return theoryData;

            void TestCase(int magnitude, Duid lhs, Duid rhs)
            {
                theoryData.Add(magnitude, lhs, rhs);
            }
        }

        [Theory]
        [MemberData(nameof(Operators_Duid_Test_TestCases))]
        public void Operator_LessThan_Test(int magnitude, Duid lhs, Duid rhs)
        {
            // Arrange
            // Act
            var result = lhs < rhs;

            // Assert
            if (magnitude == 0) // lhs equal rhs
            {
                Assert.False(result);
            }
            else if (magnitude > 0) // lhs > rhs
            {
                Assert.False(result);
            }
            else // lhs < rhs
            {
                Assert.True(result);
            }
        }

        [Theory]
        [MemberData(nameof(Operators_Duid_Test_TestCases))]
        public void Operator_LessThanOrEqual_Test(int magnitude, Duid lhs, Duid rhs)
        {
            // Act
            var result = lhs <= rhs;

            // Assert
            if (magnitude == 0) // lhs equal rhs
            {
                Assert.True(result);
            }
            else if (magnitude > 0) // lhs > rhs
            {
                Assert.False(result);
            }
            else // lhs < rhs
            {
                Assert.True(result);
            }
        }

        [Theory]
        [MemberData(nameof(Operators_Duid_Test_TestCases))]
        public void Operator_GreaterThan_Test(int magnitude, Duid lhs, Duid rhs)
        {
            // Act
            var result = lhs > rhs;

            // Assert
            if (magnitude == 0) // lhs equal rhs
            {
                Assert.False(result);
            }
            else if (magnitude > 0) // lhs > rhs
            {
                Assert.True(result);
            }
            else // lhs < rhs
            {
                Assert.False(result);
            }
        }

        [Theory]
        [MemberData(nameof(Operators_Duid_Test_TestCases))]
        public void Operator_GreaterThanOrEqual_Test(int magnitude, Duid lhs, Duid rhs)
        {
            // Act
            var result = lhs >= rhs;

            // Assert
            if (magnitude == 0) // lhs equal rhs
            {
                Assert.True(result);
            }
            else if (magnitude > 0) // lhs > rhs
            {
                Assert.True(result);
            }
            else // lhs < rhs
            {
                Assert.False(result);
            }
        }

        #endregion

        #endregion
    }
}

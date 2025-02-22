using System.Collections.Generic;
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

        #region Comparison Operators

        public static IEnumerable<object[]> Operators_Duid_Test_TestCases()
        {
            var zeroArray = new byte[] { 0x00, 0x00, 0x00 };

            var nonNullDuid = new Duid(zeroArray);

            yield return TestCase(0, nonNullDuid, nonNullDuid); // reference equal
            yield return TestCase(0, nonNullDuid, new Duid(zeroArray)); // non-reference equal
            yield return TestCase(0, null, null); // both null equal

            // single null operand (null less than non-null)
            yield return TestCase(1, nonNullDuid, null);
            yield return TestCase(-1, null, nonNullDuid);

            // non-null operands (in theory based on CompareTo)
            var otherDuid = new Duid(new byte[] { 0xFF, 0x00, 0x80 });

            yield return TestCase(nonNullDuid.CompareTo(otherDuid), nonNullDuid, otherDuid);
            yield return TestCase(otherDuid.CompareTo(nonNullDuid), otherDuid, nonNullDuid);
            yield return TestCase(otherDuid.CompareTo(otherDuid), otherDuid, otherDuid);

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(int magnitude, Duid lhs, Duid rhs)
            {
                return new object[] { magnitude, lhs, rhs };
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

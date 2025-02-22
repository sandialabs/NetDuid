using System.Collections.Generic;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region ToString(string, IFormatProvider)

        public static IEnumerable<object[]> ToString_String_IFormatProvider_Test_TestCases()
        {
            var defaultFormat = new string[] { null, string.Empty, "U:", "u:" };
            var lowerColonFormat = new string[] { "L:", "l:" };

            var upperDashFormat = new string[] { "U-", "u-", "-" };
            var lowerDashFormat = new string[] { "L-", "l-" };

            var upperNoDelimiter = new string[] { "U", "u" };
            var lowerNoDelimiter = new string[] { "L", "l" };

            for (var byteCount = 3; byteCount <= 130; byteCount += 5)
            {
                var bytes = StaticTestData.GenerateBytes(byteCount, byteCount);
                var duid = new Duid(bytes);

                // default format
                foreach (var format in defaultFormat)
                {
                    yield return TestCase(bytes.BytesAsString(":").ToUpper(), format, duid);
                }

                // lower colon format
                foreach (var format in lowerColonFormat)
                {
                    yield return TestCase(bytes.BytesAsString(":").ToLower(), format, duid);
                }

                // upper dash format
                foreach (var format in upperDashFormat)
                {
                    yield return TestCase(bytes.BytesAsString("-").ToUpper(), format, duid);
                }

                // lower dash format
                foreach (var format in lowerDashFormat)
                {
                    yield return TestCase(bytes.BytesAsString("-").ToLower(), format, duid);
                }

                // upper no delimiter format
                foreach (var format in upperNoDelimiter)
                {
                    yield return TestCase(bytes.BytesAsString().ToUpper(), format, duid);
                }

                // lower no delimiter format
                foreach (var format in lowerNoDelimiter)
                {
                    yield return TestCase(bytes.BytesAsString().ToLower(), format, duid);
                }
            }

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(string expected, string format, Duid duid)
            {
                return new object[] { expected, format, duid };
            }
        }

        [Theory]
        [MemberData(nameof(ToString_String_IFormatProvider_Test_TestCases))]
        public void ToString_String_IFormatProvider_Test(string expectedString, string format, Duid duid)
        {
            // Arrange
            // Act
            var result = duid.ToString(format, null);

            // Assert
            Assert.Equal(expectedString, result);
        }

        #endregion
    }
}

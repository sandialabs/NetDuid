using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region ToString(string, IFormatProvider)

        public static TheoryData<string, string, Duid> ToString_String_IFormatProvider_Test_TestCases()
        {
            var theoryData = new TheoryData<string, string, Duid>();

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
                    AddTestCase(bytes.BytesAsString(":").ToUpper(), format, duid);
                }

                // lower colon format
                foreach (var format in lowerColonFormat)
                {
                    AddTestCase(bytes.BytesAsString(":").ToLower(), format, duid);
                }

                // upper dash format
                foreach (var format in upperDashFormat)
                {
                    AddTestCase(bytes.BytesAsString("-").ToUpper(), format, duid);
                }

                // lower dash format
                foreach (var format in lowerDashFormat)
                {
                    AddTestCase(bytes.BytesAsString("-").ToLower(), format, duid);
                }

                // upper no delimiter format
                foreach (var format in upperNoDelimiter)
                {
                    AddTestCase(bytes.BytesAsString().ToUpper(), format, duid);
                }

                // lower no delimiter format
                foreach (var format in lowerNoDelimiter)
                {
                    AddTestCase(bytes.BytesAsString().ToLower(), format, duid);
                }
            }

            return theoryData;

            void AddTestCase(string expected, string format, Duid duid)
            {
                theoryData.Add(expected, format, duid);
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

using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region IParsable<Duid>

        public static TheoryData<byte[], string> TryParse_String_IFormatProvider_Success_Test_TestCases()
        {
            var theoryData = new TheoryData<byte[], string>();

            // legal delimiters
            var delimiters = new string[] { ":", "-", " ", string.Empty };

            // DUIDs should be between 3 and 130 bytes inclusively
            for (var byteCount = 3; byteCount <= 130; byteCount++)
            {
                var duidBytes = StaticTestData.GenerateBytes(byteCount, byteCount); // seed random with the number of bytes so byte arrays aren't similar

                foreach (var delimiter in delimiters)
                {
                    // upper case version
                    var upperCase = duidBytes.BytesAsString(delimiter);
                    AddTestCase(duidBytes, upperCase);

                    // lower case version
                    var lowerCase = upperCase.ToLowerInvariant();
                    AddTestCase(duidBytes, lowerCase);

                    if (delimiter != string.Empty)
                    {
                        // delimited strings do not require a leading 0
                        var delimitedUpperCase = duidBytes.BytesAsStringNoLeadingZero(delimiter);

                        AddTestCase(duidBytes, delimitedUpperCase);
                        AddTestCase(duidBytes, delimitedUpperCase.ToLowerInvariant());
                    }
                }
            }

            return theoryData;

            void AddTestCase(byte[] expectedBytes, string inputString)
            {
                theoryData.Add(expectedBytes, inputString);
            }
        }

        [Theory]
        [MemberData(nameof(TryParse_String_IFormatProvider_Success_Test_TestCases))]
        public void TryParse_String_IFormatProvider_Success_Test(byte[] expectedBytes, string inputString)
        {
            // Arrange
            // Act
            var success = Duid.TryParse(inputString, null, out var duid);

            // Assert
            Assert.True(success, "parse failed");
            Assert.NotNull(duid);

            Assert.IsType<Duid>(duid);
            Assert.Equal(expectedBytes, duid.GetBytes());
        }

        public static TheoryData<string> Try_Parse_String_IFormatProvider_Invalid_Test_TestCases()
        {
            var theoryData = new TheoryData<string>();

            AddTestCase(null);
            AddTestCase(string.Empty);
            AddTestCase("potato");

            // invalid lengths
            foreach (var byteLength in new[] { 1, 2, 131, 200 })
            {
                AddTestCase(StaticTestData.GenerateBytes(byteLength).BytesAsString());
            }

            return theoryData;

            void AddTestCase(string invalidDuidString)
            {
                theoryData.Add(invalidDuidString);
            }
        }

        [Theory]
        [MemberData(nameof(Try_Parse_String_IFormatProvider_Invalid_Test_TestCases))]
        public void Try_Parse_String_IFormatProvider_Invalid_Test(string inputString)
        {
            // Arrange
            // Act
            var success = Duid.TryParse(inputString, null, out var duid);

            // Assert
            Assert.False(success, "unexpectedly succeeded");
            Assert.Null(duid);
        }

        #endregion
    }
}

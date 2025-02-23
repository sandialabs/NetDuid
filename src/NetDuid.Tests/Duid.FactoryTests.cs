using System;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region TryParse(string, Duid)

        public static TheoryData<byte[], string> Parse_String_Success_Test_TestCases()
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
        [MemberData(nameof(Parse_String_Success_Test_TestCases))]
        public void TryParse_String_Success_Test(byte[] expectedBytes, string inputString)
        {
            // Arrange
            // Act
            var success = Duid.TryParse(inputString, out var duid);

            // Assert
            Assert.True(success, "parse failed");
            Assert.NotNull(duid);

            Assert.IsType<Duid>(duid);
            Assert.Equal(expectedBytes, duid.GetBytes());
        }

        public static TheoryData<string> Parse_String_Invalid_Test_TestCases()
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
        [MemberData(nameof(Parse_String_Invalid_Test_TestCases))]
        public void Try_Parse_String_Invalid_Test(string inputString)
        {
            // Arrange
            // Act
            var success = Duid.TryParse(inputString, out var duid);

            // Assert
            Assert.False(success, "unexpectedly succeeded");
            Assert.Null(duid);
        }

        #endregion

        #region Parse(string)
        [Theory]
        [MemberData(nameof(Parse_String_Success_Test_TestCases))]
        public void Parse_Success_String_Test(byte[] expectedBytes, string inputString)
        {
            // Arrange
            // Act
            var duid = Duid.Parse(inputString);

            // Assert
            Assert.NotNull(duid);

            Assert.IsType<Duid>(duid);
            Assert.Equal(expectedBytes, duid.GetBytes());
        }

        [Theory]
        [MemberData(nameof(Parse_String_Invalid_Test_TestCases))]
        public void Parse_String_Invalid_Throws_ArgumentException_Test(string inputString)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => Duid.Parse(inputString));
        }

        [Theory]
        [MemberData(nameof(Parse_String_Success_Test_TestCases))]
        public void Parse_Success_IFormatProvider_String_Test(byte[] expectedBytes, string inputString)
        {
            // Arrange
            // Act
            var duid = Duid.Parse(inputString, null);

            // Assert
            Assert.NotNull(duid);

            Assert.IsType<Duid>(duid);
            Assert.Equal(expectedBytes, duid.GetBytes());
        }

        [Theory]
        [MemberData(nameof(Parse_String_Invalid_Test_TestCases))]
        public void Parse_String_IFormatProvider_Invalid_Throws_ArgumentException_Test(string inputString)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => Duid.Parse(inputString, null));
        }

        #endregion
    }
}

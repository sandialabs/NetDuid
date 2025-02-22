using System;
using System.Collections.Generic;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region TryParse(string, Duid)

        public static IEnumerable<object[]> Parse_String_Success_Test_TestCases()
        {
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
                    yield return TestCase(duidBytes, upperCase);

                    // lower case version
                    var lowerCase = upperCase.ToLowerInvariant();
                    yield return TestCase(duidBytes, lowerCase);

                    if (delimiter != string.Empty)
                    {
                        // delimited strings do not require a leading 0
                        var delimitedUpperCase = duidBytes.BytesAsStringNoLeadingZero(delimiter);

                        yield return TestCase(duidBytes, delimitedUpperCase);
                        yield return TestCase(duidBytes, delimitedUpperCase.ToLowerInvariant());
                    }
                }
            }

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(byte[] expectedBytes, string inputString)
            {
                return new object[] { expectedBytes, inputString };
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

        public static IEnumerable<object[]> Parse_String_Invalid_Test_TestCases()
        {
            yield return TestCase(null);
            yield return TestCase(string.Empty);
            yield return TestCase("potato");

            // invalid lengths
            foreach (var byteLength in new[] { 1, 2, 131, 200 })
            {
                yield return TestCase(StaticTestData.GenerateBytes(byteLength).BytesAsString());
            }

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(string invalidDuidString)
            {
                return new object[] { invalidDuidString };
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

#if NET7_0_OR_GREATER
        [Theory]
        [MemberData(nameof(Parse_String_Success_Test_TestCases))]
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

        [Theory]
        [MemberData(nameof(Parse_String_Invalid_Test_TestCases))]
        public void Try_Parse_String_IFormatProvider_Invalid_Test(string inputString)
        {
            // Arrange
            // Act
            var success = Duid.TryParse(inputString, null, out var duid);

            // Assert
            Assert.False(success, "unexpectedly succeeded");
            Assert.Null(duid);
        }
#endif

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

#if NET7_0_OR_GREATER
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
#endif
        #endregion
    }
}

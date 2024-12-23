using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit;
#if NET48
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
#endif

namespace NetDuid.Tests
{
    public class DuidTests
    {
        #region Type

        [Fact]
        public void IsConcrete_Test()
        {
            // Arrange
            var type = typeof(Duid);

            // Act
            var isConcrete = type.IsClass && !type.IsAbstract;

            // Assert
            Assert.True(isConcrete);
        }

        [Fact]
        public void IsSealed_Test()
        {
            // Arrange
            var type = typeof(Duid);

            // Act
            var isSealed = type.IsSealed;

            // Assert
            Assert.True(isSealed);
        }

        [Fact]
        public void IsDecoratedWith_SerializableAttribute_Test()
        {
            // Arrange
            var type = typeof(Duid);

            // Act
            var isSerializable = type.IsDefined(typeof(SerializableAttribute), false);

            // Assert
            Assert.True(isSerializable);
        }

        [Theory]
        [InlineData(typeof(IEquatable<Duid>))]
        [InlineData(typeof(IComparable))]
        [InlineData(typeof(ISerializable))]
        [InlineData(typeof(IFormattable))]
#if NET7_0_OR_GREATER
        [InlineData(typeof(IParsable<Duid>))]
#endif
        public void Assignability_Test(Type assignableFromType)
        {
            // Arrange
            var type = typeof(Duid);

            // Act
            var isAssignableFrom = assignableFromType.IsAssignableFrom(type);

            // Assert
            Assert.True(isAssignableFrom);
        }

        #endregion

        #region Constrcutor IEnumerable<byte>

        public static IEnumerable<object[]> Constructor_Bytes_BadLength_Throws_ArgumentException_Test_TestCases()
        {
            // invalid byte lengths
            foreach (var byteLength in new[] { 0, 1, 2, 131, 200 })
            {
                yield return new object[] { StaticTestData.GenerateBytes(byteLength) };
            }
        }

        [Theory]
        [MemberData(nameof(Constructor_Bytes_BadLength_Throws_ArgumentException_Test_TestCases))]
        public void Constructor_Bytes_BadLength_Throws_ArgumentException_Test(byte[] bytes)
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => new Duid(bytes));
        }

        [Fact]
        public void Constructor_Bytes_NullInput_Throws_ArgumentNullException_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => new Duid(null));
        }

        #endregion

        #region DuidType

        public static IEnumerable<object[]> Type_Test_TestCases()
        {
            var bytes = StaticTestData.GenerateBytes(32);

            // undefined subset, seemed to make sense not to iterate though all 2^16 possibilities of 2 octet prefixes
            yield return TestCase(DuidType.Undefined, bytes.PrependBytes(0x00, 0x00));
            yield return TestCase(DuidType.Undefined, bytes.PrependBytes(0x00, 0x05));
            yield return TestCase(DuidType.Undefined, bytes.PrependBytes(0x00, 0xFF));
            yield return TestCase(DuidType.Undefined, bytes.PrependBytes(0x0F, 0xFF));

            // Linked Layer Plus Time
            yield return TestCase(DuidType.LinkLayerPlusTime, bytes.PrependBytes(0x00, 0x01));

            // Vendor Assigned
            yield return TestCase(DuidType.VendorAssigned, bytes.PrependBytes(0x00, 0x02));

            // Link Layer
            yield return TestCase(DuidType.LinkLayer, bytes.PrependBytes(0x00, 0x03));

            // UUID LinkLayer
            yield return TestCase(DuidType.Uuid, bytes.PrependBytes(0x00, 0x04));

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(DuidType expectedType, byte[] duidBytes)
            {
                return new object[] { expectedType, duidBytes };
            }
        }

        [Theory]
        [MemberData(nameof(Type_Test_TestCases))]
        public void Type_Test(DuidType expectType, byte[] duidBytes)
        {
            // Arrange
            var duid = new Duid(duidBytes);

            // Act
            var type = duid.Type;

            // Assert
            Assert.Equal(expectType, type);
        }

        #endregion

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

        #region Compare(Duid)

        public static IEnumerable<object[]> Compare_Duid_Test_TestCases()
        {
            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });
            yield return TestCase(0, referenceDuid, referenceDuid); // by reference
            yield return TestCase(-1, referenceDuid, null); // null case

            object[] TestCase(int expectedSign, Duid thisDuid, Duid otherDuid)
            {
                return new object[] { expectedSign, thisDuid, otherDuid };
            }

            var deepTestCases = new List<IEnumerable<object[]>>
            {
                // equality
                TestCases(0, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 }),
                TestCases(0, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 }),
                // by length
                TestCases(-1, new byte[3], new byte[4]),
                TestCases(-1, new byte[3], new byte[130]),
                // by length, then big endian value
                TestCases(-1, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
                TestCases(-1, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 }),
                TestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 }),
                TestCases(-1, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 })
            }.SelectMany(tc => tc);

            foreach (var testCase in deepTestCases)
            {
                yield return testCase;
            }

            IEnumerable<object[]> TestCases(int expectedSign, byte[] thisBytes, byte[] otherBytes)
            {
                yield return TestCase(expectedSign, new Duid(thisBytes), new Duid(otherBytes));

                if (expectedSign != 0)
                {
                    // create transitive test case
                    yield return TestCase(expectedSign * -1, new Duid(otherBytes), new Duid(thisBytes));
                }
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

        #region Equal(Duid)

        public static IEnumerable<object[]> Equal_Duid_Test_TestCases()
        {
            var referenceDuid = new Duid(new byte[] { 0x00, 0x00, 0xff });
            yield return TestCase(true, referenceDuid, referenceDuid); // by reference
            yield return TestCase(false, referenceDuid, null); // null case

            object[] TestCase(bool expectedEqual, Duid thisDuid, Duid otherDuid)
            {
                return new object[] { expectedEqual, thisDuid, otherDuid };
            }

            var deepTestCases = new List<IEnumerable<object[]>>
            {
                // equality
                TestCases(true, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x00 }),
                TestCases(true, new byte[] { 0xFF, 0x00, 0x80 }, new byte[] { 0xFF, 0x00, 0x80 }),
                TestCases(false, new byte[3], new byte[4]),
                TestCases(false, new byte[3], new byte[130]),
                TestCases(false, new byte[] { 0xFF, 0x00, 0x00 }, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }),
                TestCases(false, new byte[] { 0xFF, 0xFF, 0xFF }, new byte[] { 0xFF, 0x00, 0x00, 0x00 }),
                TestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x00, 0x00, 0x01 }),
                TestCases(false, new byte[] { 0x00, 0x00, 0x00 }, new byte[] { 0x80, 0x00, 0x00 })
            }.SelectMany(tc => tc);

            foreach (var testCase in deepTestCases)
            {
                yield return testCase;
            }

            IEnumerable<object[]> TestCases(bool expectedEqual, byte[] thisBytes, byte[] otherBytes)
            {
                yield return TestCase(expectedEqual, new Duid(thisBytes), new Duid(otherBytes));

                if (!expectedEqual)
                {
                    // create transitive test case
                    yield return TestCase(expectedEqual, new Duid(otherBytes), new Duid(thisBytes));
                }
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
        public void Equal_Object_Test(bool expectedEqual, Duid thisDuid, object otherDuid)
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

        #region GetHashCode
        public static IEnumerable<object[]> HashCode_Test_TestCases()
        {
            for (var byteCount = 3; byteCount <= 130; byteCount += 5)
            {
                yield return new object[] { StaticTestData.GenerateBytes(byteCount, byteCount) };
            }
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

        #region ToString

        public static IEnumerable<object[]> ToString_Test_TestCases()
        {
            for (var byteCount = 3; byteCount <= 130; byteCount += 5)
            {
                var bytes = StaticTestData.GenerateBytes(byteCount, byteCount);
                yield return TestCase(bytes.BytesAsString(":"), new Duid(bytes));
            }

#if NET6_0_OR_GREATER
            static
#endif
            object[] TestCase(string expected, Duid duid)
            {
                return new object[] { expected, duid };
            }
        }

        [Theory] // TODO this test generation should be automated
        [MemberData(nameof(ToString_Test_TestCases))]
        public void ToString_Test(string expectedString, Duid duid)
        {
            // Arrange
            // Act
            var result = duid.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        #endregion

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

                // deafault format
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

                // upper no delimter format
                foreach (var format in upperNoDelimiter)
                {
                    yield return TestCase(bytes.BytesAsString().ToUpper(), format, duid);
                }

                // lower no delimter format
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

        #region ISerializable
#if NET48   // TODO find Serialize for testing outside of NET 4.8

        public static IEnumerable<object[]> Serializable_Test_TestCases()
        {
            for (var byteCount = 3; byteCount <= 130; byteCount++)
            {
                var bytes = StaticTestData.GenerateBytes(byteCount, byteCount);
                yield return new object[] { new Duid(bytes) };
            }
        }

        [Theory]
        [MemberData(nameof(Serializable_Test_TestCases))]
        public void Serializable_Test(Duid duid)
        {
            // Arrange
            var stream = new MemoryStream();
            var formatter = new SoapFormatter(); // Soap isn't important, only the ability to find and use a reasonable formatter

            // Act
            formatter.Serialize(stream, duid); // serialize
            stream.Seek(0, SeekOrigin.Begin);
            var result = formatter.Deserialize(stream) as Duid; // deserialize

            // Assert
            Assert.NotNull(result);
            Assert.Equal(duid, result);
        }

#endif

        #endregion
    }
}

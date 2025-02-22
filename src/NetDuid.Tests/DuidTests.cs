using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
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
    }
}

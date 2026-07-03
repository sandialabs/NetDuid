#if !NET9_0_OR_GREATER
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region ISerializable

        public static TheoryData<Duid> Serializable_Test_TestCases()
        {
            var theoryData = new TheoryData<Duid>();

            for (var byteCount = 3; byteCount <= 130; byteCount++)
            {
                var bytes = StaticTestData.GenerateBytes(byteCount, byteCount);
                var duid = new Duid(bytes);

                theoryData.Add(duid);
            }

            return theoryData;
        }

        [Theory]
        [MemberData(nameof(Serializable_Test_TestCases))]
        public void Serializable_Test(Duid duid)
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
#if !NET48
#pragma warning disable SYSLIB0011
#endif
                var formatter = new BinaryFormatter();

                // Act
                formatter.Serialize(stream, duid); // serialize
                stream.Seek(0, SeekOrigin.Begin);
                var result = formatter.Deserialize(stream) as Duid; // deserialize
#if !NET48
#pragma warning restore SYSLIB0011
#endif

                // Assert
                Assert.NotNull(result);
                Assert.Equal(duid, result);
            }
        }

        [Theory]
        [MemberData(nameof(Serializable_Test_TestCases))]
        public void Serializable_GetHashCode_Test(Duid duid)
        {
            // Arrange
            using (var stream = new MemoryStream())
            {
#if !NET48
#pragma warning disable SYSLIB0011
#endif
                var formatter = new BinaryFormatter();

                // Act
                formatter.Serialize(stream, duid);
                stream.Seek(0, SeekOrigin.Begin);
                var result = formatter.Deserialize(stream) as Duid;
#if !NET48
#pragma warning restore SYSLIB0011
#endif

                // Assert
                Assert.NotNull(result);
                Assert.Equal(duid.GetHashCode(), result.GetHashCode());
            }
        }

        #endregion

        #region Deserialization exception paths

        [Fact]
        public void Deserialize_UnrecognizedVersion_ThrowsSerializationException_Test()
        {
            // Arrange
#if !NET48
#pragma warning disable SYSLIB0050
#endif
            var info = new SerializationInfo(typeof(Duid), new FormatterConverter());
            info.AddValue("SerializableVersion", 999);
            info.AddValue("_duidBytes", new byte[] { 0xAB, 0xCD, 0xEF }, typeof(byte[]));
            var context = new StreamingContext(StreamingContextStates.All);
#if !NET48
#pragma warning restore SYSLIB0050
#endif
            var ctor = typeof(Duid).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(SerializationInfo), typeof(StreamingContext) },
                null
            );

            // Act
            var exception = Assert.Throws<TargetInvocationException>(() => ctor.Invoke(new object[] { info, context }));

            // Assert
            Assert.IsType<SerializationException>(exception.InnerException);
            Assert.Contains(
                "Could not deserialize unrecognized format",
                exception.InnerException.Message,
                StringComparison.Ordinal
            );
        }

        [Fact]
        public void Deserialize_NullByteArray_ThrowsSerializationException_Test()
        {
            // Arrange
#if !NET48
#pragma warning disable SYSLIB0050
#endif
            var info = new SerializationInfo(typeof(Duid), new FormatterConverter());
            info.AddValue("SerializableVersion", 0);
            info.AddValue("_duidBytes", null, typeof(byte[]));
            var context = new StreamingContext(StreamingContextStates.All);
#if !NET48
#pragma warning restore SYSLIB0050
#endif
            var ctor = typeof(Duid).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(SerializationInfo), typeof(StreamingContext) },
                null
            );

            // Act
            var exception = Assert.Throws<TargetInvocationException>(() => ctor.Invoke(new object[] { info, context }));

            // Assert
            Assert.IsType<SerializationException>(exception.InnerException);
            Assert.Contains("unrecognized input byte array", exception.InnerException.Message, StringComparison.Ordinal);
        }

        #endregion
    }
}
#endif

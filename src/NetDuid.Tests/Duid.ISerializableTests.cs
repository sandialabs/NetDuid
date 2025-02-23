#if !NET9_0_OR_GREATER
using System.IO;
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

        #endregion
    }
}
#endif

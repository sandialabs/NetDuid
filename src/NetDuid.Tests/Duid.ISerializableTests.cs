#if NET48
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using Xunit;

namespace NetDuid.Tests
{
    public partial class DuidTests
    {
        #region ISerializable

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

        #endregion
    }
}
#endif

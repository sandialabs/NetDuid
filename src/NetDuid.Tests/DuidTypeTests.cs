using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetDuid.Tests
{
    public class DuidTypeTests
    {
        #region other members

        public static IEnumerable<object[]> DuidType_Values()
        {
            return Enum.GetValues(typeof(DuidType)).Cast<DuidType>().Select(e => new object[] { e });
        }

        #endregion

        [Fact]
        public void DuidType_GetEnumUnderlyingType_Test()
        {
            // Arrange
            var type = typeof(DuidType);
            var enumUnderlyingType = type.GetEnumUnderlyingType();

            // Act
            var isAssignable = typeof(int).IsAssignableFrom(enumUnderlyingType);

            // Assert
            Assert.True(isAssignable);
        }

        [Fact]
        public void DuidType_IsEnum_Test()
        {
            // Arrange
            var type = typeof(DuidType);

            // Act
            // Assert
            Assert.True(type.IsEnum);
        }

        [Fact]
        public void DuidType_IsNotFlags_Test()
        {
            // Arrange
            var type = typeof(DuidType);
            var flagAttributes = type.GetCustomAttributes(typeof(FlagsAttribute), false);

            // Act
            // Assert
            Assert.Empty(flagAttributes);
        }

        [Fact]
        public void DuidType_UniqueValues_Test()
        {
            // Arrange
            var values = Enum.GetValues(typeof(DuidType)).OfType<DuidType>();

            // Act
            var groupedValues = values.GroupBy(flag => (int)flag, flag => flag);

            // Assert
            Assert.All(
                groupedValues,
                valueGroup => Assert.True(valueGroup.Count() == 1, string.Join(",", valueGroup.Select(v => v.ToString())))
            );
        }

        [Fact]
        public void DuidType_Length_Test()
        {
            // Arrange
            // Act
            var count = Enum.GetValues(typeof(DuidType)).OfType<DuidType>().Count();

            // Assert
            Assert.Equal(5, count);
        }

        [Fact]
        public void DuidType_DefaultValue_Test()
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(default, DuidType.Undefined);
        }

        [Theory]
        [InlineData(0, DuidType.Undefined)]
        [InlineData(1, DuidType.LinkLayerPlusTime)]
        [InlineData(2, DuidType.VendorAssigned)]
        [InlineData(3, DuidType.LinkLayer)]
        [InlineData(4, DuidType.Uuid)]
        public void DuidType_Index_Test(int expected, DuidType enumValue)
        {
            // Arrange
            // Act
            // Assert
            Assert.Equal(expected, (int)enumValue);
        }

        [Theory]
        [InlineData(DuidType.Undefined, "Undefined")]
        [InlineData(DuidType.LinkLayerPlusTime, "LinkLayerPlusTime")]
        [InlineData(DuidType.VendorAssigned, "VendorAssigned")]
        [InlineData(DuidType.LinkLayer, "LinkLayer")]
        [InlineData(DuidType.Uuid, "Uuid")]
        public void DuidType_Parse_Test(DuidType expected, string parseString)
        {
            // Arrange
            // Act
            var result = Enum.Parse(typeof(DuidType), parseString, false);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}

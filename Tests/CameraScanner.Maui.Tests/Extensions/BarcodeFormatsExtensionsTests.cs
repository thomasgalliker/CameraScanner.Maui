using CameraScanner.Maui.Extensions;
using FluentAssertions;
using Xunit;

namespace CameraScanner.Maui.Tests.Extensions
{
    public class BarcodeFormatsExtensionsTests
    {
        [Fact]
        public void ShouldConvertFlagsToArray_None()
        {
            // Arrange
            var barcodeFormats = BarcodeFormats.None;

            // Act
            var barcodeFormatsArray = barcodeFormats.ToArray();

            // Assert
            barcodeFormatsArray.Should().BeEmpty();
        }

        [Fact]
        public void ShouldConvertFlagsToArray_All1D()
        {
            // Arrange
            var barcodeFormats = BarcodeFormats.All1D;

            // Act
            var barcodeFormatsArray = barcodeFormats.ToArray();

            // Assert
            barcodeFormatsArray.Should().HaveCount(11);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.None);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All1D);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All2D);
        }
        

        [Fact]
        public void ShouldConvertFlagsToArray_All2D()
        {
            // Arrange
            var barcodeFormats = BarcodeFormats.All2D;

            // Act
            var barcodeFormatsArray = barcodeFormats.ToArray();

            // Assert
            barcodeFormatsArray.Should().HaveCount(6);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.None);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All1D);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All2D);
        }

        [Fact]
        public void ShouldConvertFlagsToArray_All()
        {
            // Arrange
            var barcodeFormats = BarcodeFormats.All;

            // Act
            var barcodeFormatsArray = barcodeFormats.ToArray();

            // Assert
            barcodeFormatsArray.Should().HaveCount(17);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.None);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All1D);
            barcodeFormatsArray.Should().NotContain(BarcodeFormats.All2D);
        }

        [Fact]
        public void ShouldConvertFlagsToArray_SomeValues()
        {
            // Arrange
            var barcodeFormats = BarcodeFormats.Aztec | BarcodeFormats.QR | BarcodeFormats.Ean13;

            // Act
            var barcodeFormatsArray = barcodeFormats.ToArray();

            // Assert
            barcodeFormatsArray.Should().HaveCount(3);
        }

        [Fact]
        public void ShouldConvertArrayToFlags_SomeValues()
        {
            // Arrange
            var barcodeFormatsArray = new[]
            {
                BarcodeFormats.Aztec,
                BarcodeFormats.QR,
                BarcodeFormats.Ean13
            };

            // Act
            var barcodeFormats = barcodeFormatsArray.ToEnum();

            // Assert
            barcodeFormats.Should().Be(BarcodeFormats.Aztec | BarcodeFormats.QR | BarcodeFormats.Ean13);
        }

        [Fact]
        public void ShouldConvertArrayToFlags_None()
        {
            // Arrange
            var barcodeFormatsArray = Array.Empty<BarcodeFormats>();

            // Act
            var barcodeFormats = barcodeFormatsArray.ToEnum();

            // Assert
            barcodeFormats.Should().Be(BarcodeFormats.None);
        }

        [Fact]
        public void ShouldConvertArrayToFlags_All()
        {
            // Arrange
            var barcodeFormatsArray = BarcodeFormats.All.ToArray();

            // Act
            var barcodeFormats = barcodeFormatsArray.ToEnum();

            // Assert
            barcodeFormats.Should().Be(BarcodeFormats.All);
        }
    }
}
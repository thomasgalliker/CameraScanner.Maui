using FluentAssertions;
using Xunit;

namespace CameraScanner.Maui.Tests
{
    public class BarcodeResultTests
    {
        [Theory]
        [ClassData(typeof(BarcodeResultTestData))]
        public void ShouldBeEqual(BarcodeResult barcodeResult1, BarcodeResult barcodeResult2, bool expectedEqual)
        {
            // Act
            var isEqual = barcodeResult1 == barcodeResult2;

            // Assert
            isEqual.Should().Be(expectedEqual);
        }

        public class BarcodeResultTestData : TheoryData<BarcodeResult, BarcodeResult, bool>
        {
            public BarcodeResultTestData()
            {
                this.Add(
                    new BarcodeResult(null),
                    new BarcodeResult(null),
                    true);
                this.Add(
                    new BarcodeResult("test"),
                    new BarcodeResult("test"),
                    true);
                this.Add(
                    new BarcodeResult("test"),
                    new BarcodeResult("Test"),
                    false);
            }
        }
    }
}
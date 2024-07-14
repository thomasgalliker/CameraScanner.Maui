using Moq;
using Moq.AutoMock;
using Xunit;

namespace CameraScanner.Maui.Tests
{
    public class CameraViewTests
    {
        private readonly AutoMocker autoMocker;

        public CameraViewTests()
        {
            this.autoMocker = new AutoMocker();
        }

        [Fact]
        public void DetectionFinished_ShouldReturn_IfBarcodeResultsIsNull()
        {
            // Arrange
            var vibrationMock = new Mock<IVibration>();
            var cameraView = this.autoMocker.CreateInstance<CameraView>(enablePrivate: true);

            // Act
            cameraView.DetectionFinished(null);

            // Assert
            vibrationMock.VerifyNoOtherCalls();
        }

        // TODO: Add more unit tests here!
    }
}

namespace CameraScanner.Maui
{
    public class OnDetectionFinishedEventArg : EventArgs
    {
        public BarcodeResult[] BarcodeResults { get; set; } = [];
    }
}
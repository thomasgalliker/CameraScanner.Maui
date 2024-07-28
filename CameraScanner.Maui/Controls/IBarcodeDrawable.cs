namespace CameraScanner.Maui.Controls
{
    public interface IBarcodeDrawable : IDrawable
    {
        void Update(BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor);
        
        void Reset();
    }
}
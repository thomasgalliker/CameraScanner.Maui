namespace CameraScanner.Maui.Controls
{
    public interface IBarcodeDrawable : IDrawable
    {
        void OnSizeAllocated(double width, double height);

        void Update(BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor);

        RectF ConvertRectF(RectF source);

        void Reset();
    }
}
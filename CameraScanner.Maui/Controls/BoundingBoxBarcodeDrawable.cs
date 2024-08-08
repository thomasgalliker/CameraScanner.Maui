namespace CameraScanner.Maui.Controls
{
    public class BoundingBoxBarcodeDrawable : BarcodeDrawable
    {
        protected override void DrawResult(ICanvas canvas, BarcodeResult barcodeResult)
        {
            // Draw rectangle around the barcode result
            canvas.DrawRectangle(barcodeResult.PreviewBoundingBox);
        }

        protected override PointF GetTextPosition(BarcodeResult barcodeResult)
        {
            return new PointF(barcodeResult.PreviewBoundingBox.Left, barcodeResult.PreviewBoundingBox.Bottom + 8F);
        }
    }
}
namespace CameraScanner.Maui.Controls
{
    public class CornerPointBarcodeDrawable : BarcodeDrawable
    {
        protected override bool IsValid(BarcodeResult barcodeResult)
        {
            return barcodeResult.CornerPoints is Point[] cornerPoints && cornerPoints.Length == 4;
        }

        protected override void DrawResult(ICanvas canvas, BarcodeResult barcodeResult)
        {
            // Draw path around corner points
            var cornerToCornerPath = new PathF();
            cornerToCornerPath.MoveTo(barcodeResult.CornerPoints.Last());

            foreach (var cornerPoint in barcodeResult.CornerPoints)
            {
                cornerToCornerPath.LineTo(cornerPoint);
            }

            canvas.DrawPath(cornerToCornerPath);
        }

        protected override PointF GetTextPosition(BarcodeResult barcodeResult)
        {
            var pointWithHighestY = barcodeResult.CornerPoints
                .Skip(2)
                .First();

            return new PointF((float)pointWithHighestY.X, (float)pointWithHighestY.Y + 8F);
        }
    }
}
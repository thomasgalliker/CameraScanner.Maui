namespace CameraScanner.Maui.Controls
{
    public class CornerPointBarcodeDrawable : BarcodeDrawable
    {
        protected override void DrawResult(ICanvas canvas, BarcodeResult barcodeResult)
        {
            // Draw path around corner points
            if (barcodeResult.CornerPoints is Point[] cornerPoints && cornerPoints.Length != 0)
            {
                var cornerToCornerPath = new PathF();
                cornerToCornerPath.MoveTo(cornerPoints.Last());

                foreach (var cornerPoint in cornerPoints)
                {
                    cornerToCornerPath.LineTo(cornerPoint);
                }

                canvas.DrawPath(cornerToCornerPath);
            }
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
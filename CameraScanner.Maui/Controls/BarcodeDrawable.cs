namespace CameraScanner.Maui.Controls
{
    public class BarcodeDrawable : IDrawable
    {
        private BarcodeResult[] barcodeResults;

        public BarcodeResult[] BarcodeResults
        {
            private get;
            set;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (this.BarcodeResults is { Length: > 0 } barcodeResults)
            {
                canvas.StrokeSize = 8;
                canvas.StrokeColor = Colors.Red;
                var scale = 1 / canvas.DisplayScale;
                canvas.Scale(scale, scale);

                foreach (var barcodeResult in barcodeResults)
                {
                    canvas.DrawRectangle(barcodeResult.PreviewBoundingBox);
                }
            }
        }
    }
}
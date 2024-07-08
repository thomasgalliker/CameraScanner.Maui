namespace CameraScanner.Maui.Controls
{
    public class BarcodeDrawable : IDrawable
    {
        private BarcodeResult[] barcodeResults;
        private float strokeSize;
        private Color strokeColor;

        public void Update(BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor)
        {
            this.barcodeResults = barcodeResults;
            this.strokeSize = strokeSize;
            this.strokeColor = strokeColor;
        }

        public void Reset()
        {
            this.barcodeResults = null;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (this.barcodeResults is { Length: > 0 } results)
            {
                canvas.StrokeSize = this.strokeSize;
                canvas.StrokeColor = this.strokeColor;
                var scale = 1 / canvas.DisplayScale;
                canvas.Scale(scale, scale);

                foreach (var barcodeResult in results)
                {
                    canvas.DrawRectangle(barcodeResult.PreviewBoundingBox);
                }
            }
        }
    }
}
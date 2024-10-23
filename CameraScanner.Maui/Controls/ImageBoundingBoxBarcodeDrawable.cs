namespace CameraScanner.Maui.Controls
{
    public class ImageBoundingBoxBarcodeDrawable : BarcodeDrawable
    {
        protected override bool IsValid(BarcodeResult barcodeResult)
        {
            return barcodeResult.ImageBoundingBox != RectF.Zero;
        }

        protected override void DrawResult(ICanvas canvas, BarcodeResult barcodeResult)
        {
            // Draw rectangle around the barcode result
            // using ImageBoundingBox
            var rect = this.ConvertRectF(barcodeResult.ImageBoundingBox);
            canvas.DrawRectangle(rect);
        }

        public override RectF ConvertRectF(RectF source)
        {
            var width = source.Width * (float)this.Width;
            var height = source.Height * (float)this.Height;
            var left = source.Left * (float)this.Width;
            var top = source.Top * (float)this.Height;
            var rect = new RectF(left, top, width, height);
            return rect;
        }

        protected override PointF GetTextPosition(BarcodeResult barcodeResult)
        {
            var x = barcodeResult.ImageBoundingBox.Left * (float)this.Width;
            var y = barcodeResult.ImageBoundingBox.Bottom * (float)this.Height + (float)(this.StrokeSize * 2d);
            return new PointF(x, y);
        }
    }
}
using Font = Microsoft.Maui.Graphics.Font;

namespace CameraScanner.Maui.Controls
{
    internal class BarcodeDrawable : IDrawable
    {
        private BarcodeResult[] barcodeResults;
        private float strokeSize;
        private Color strokeColor;
        private Color textColor;

        public void Update(BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor)
        {
            this.barcodeResults = barcodeResults;
            this.strokeSize = strokeSize;
            this.strokeColor = strokeColor;
            this.textColor = textColor;
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
                    // Rectangle around the barcode result
                    canvas.DrawRectangle(barcodeResult.PreviewBoundingBox);

                    // Display preview text underneath the barcode result
                    var displayValue = GetPreviewText(barcodeResult);

                    var position = new PointF(
                        x: barcodeResult.PreviewBoundingBox.Left,
                        y: barcodeResult.PreviewBoundingBox.Bottom + this.strokeSize);

                    DrawText(canvas, $"{barcodeResult.BarcodeFormat}", displayValue, position, this.strokeColor, this.textColor);
                }
            }
        }

        private static string GetPreviewText(BarcodeResult barcodeResult)
        {
            var displayValueTopNLines = barcodeResult.DisplayValue.SplitToLines()
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(3)
                .ToArray();

            if (displayValueTopNLines.Length == 3)
            {
                displayValueTopNLines[2] = "(...)";
            }

            var displayValue = string.Join(Environment.NewLine, displayValueTopNLines);
            return displayValue;
        }

        private static void DrawText(ICanvas canvas, string title, string text, PointF position, Color strokeColor, Color textColor)
        {
            const float fontSize = 16f;

            var stringSizeTitle = canvas.GetStringSize(title, Font.Default, fontSize);
            var stringSizeText = canvas.GetStringSize(text, Font.Default, fontSize);

            var stringBoundsTitle = new RectF(
                position.X + 8f,
                position.Y + 8f,
                stringSizeTitle.Width + 8f,
                stringSizeTitle.Height + 2f);

            var stringBoundsText = new RectF(
                position.X + 8f,
                stringBoundsTitle.Y + stringBoundsTitle.Height + 2f,
                stringSizeText.Width + 8f,
                stringSizeText.Height + 8f);

            var unionBounds = new RectF(
                position.X,
                position.Y,
                Math.Max(stringBoundsTitle.Width, stringBoundsText.Width) + 16f,
                stringBoundsTitle.Height + stringBoundsText.Height + 14f);

            canvas.StrokeColor = strokeColor;
            canvas.FontColor = textColor;
            canvas.FillColor = strokeColor;
            canvas.FillRoundedRectangle(unionBounds, 8f);

            canvas.FontSize = fontSize;
            canvas.Font = Font.Default;

            canvas.DrawString(title, stringBoundsTitle, HorizontalAlignment.Left, VerticalAlignment.Top);
            canvas.DrawString(text, stringBoundsText, HorizontalAlignment.Left, VerticalAlignment.Top);
        }
    }
}
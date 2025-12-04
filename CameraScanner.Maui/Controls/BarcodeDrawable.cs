using Font = Microsoft.Maui.Graphics.Font;

namespace CameraScanner.Maui.Controls
{
    public abstract class BarcodeDrawable : IBarcodeDrawable
    {
        private BarcodeResult[] barcodeResults = Array.Empty<BarcodeResult>();
        private float strokeSize;
        private Color? strokeColor;
        private Color? textColor;
        private double width;
        private double height;

        public virtual void OnSizeAllocated(double width, double height)
        {
            this.width = width;
            this.height = height;
        }

        public double Width => this.width;

        public double Height => this.height;

        public double StrokeSize => this.strokeSize;

        public void Update(BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor)
        {
            this.barcodeResults = barcodeResults;
            this.strokeSize = strokeSize;
            this.strokeColor = strokeColor;
            this.textColor = textColor;
        }

        public virtual RectF ConvertRectF(RectF source)
        {
            return source;
        }

        public void Reset()
        {
            this.barcodeResults = Array.Empty<BarcodeResult>();
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
                    if (this.IsValid(barcodeResult))
                    {
                        this.DrawResult(canvas, barcodeResult);

                        // Display preview text underneath the barcode result
                        var displayValue = GetPreviewText(barcodeResult);

                        var textPosition = this.GetTextPosition(barcodeResult);
                        if (displayValue != null &&
                            this.strokeColor is Color strokeColor &&
                            this.textColor is Color textColor)
                        {
                            DrawText(canvas, $"{barcodeResult.BarcodeFormat}", displayValue, textPosition, strokeColor, textColor);
                        }
                    }
                    else
                    {
                        // TODO: There are cases where we don't get valid barcode position information
                    }
                }
            }
        }

        protected abstract bool IsValid(BarcodeResult barcodeResult);

        protected abstract void DrawResult(ICanvas canvas, BarcodeResult barcodeResult);

        protected abstract PointF GetTextPosition(BarcodeResult barcodeResult);

        private static string? GetPreviewText(BarcodeResult barcodeResult)
        {
            if (barcodeResult.DisplayValue is not string displayValue)
            {
                return null;
            }

            var displayValueTopNLines = displayValue.SplitToLines()
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Take(3)
                .ToArray();

            if (displayValueTopNLines.Length == 3)
            {
                displayValueTopNLines[2] = "(...)";
            }

            displayValue = string.Join(Environment.NewLine, displayValueTopNLines);
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
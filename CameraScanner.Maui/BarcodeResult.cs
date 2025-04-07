using System.Diagnostics;

namespace CameraScanner.Maui
{
    [DebuggerDisplay("{this.BarcodeFormat}")]
    public class BarcodeResult : IEquatable<BarcodeResult>
    {
        public BarcodeResult(string displayValue)
        {
            this.DisplayValue = displayValue;
        }

        public BarcodeResult(string displayValue, BarcodeTypes barcodeType)
        {
            this.DisplayValue = displayValue;
            this.BarcodeType = barcodeType;
        }

        public BarcodeResult(string displayValue, BarcodeTypes barcodeType, BarcodeFormats barcodeFormat)
        {
            this.DisplayValue = displayValue;
            this.BarcodeType = barcodeType;
            this.BarcodeFormat = barcodeFormat;
        }

        public BarcodeResult(
            string displayValue,
            BarcodeTypes barcodeType,
            BarcodeFormats barcodeFormat,
            string rawValue,
            byte[] rawBytes,
            RectF previewBoundingBox,
            RectF imageBoundingBox,
            Point[] cornerPoints)
        {
            this.DisplayValue = displayValue;
            this.BarcodeType = barcodeType;
            this.BarcodeFormat = barcodeFormat;
            this.RawValue = rawValue;
            this.RawBytes = rawBytes;
            this.PreviewBoundingBox = previewBoundingBox;
            this.ImageBoundingBox = imageBoundingBox;
            this.CornerPoints = cornerPoints;
        }

        /// <summary>
        /// Gets the barcode type of the scan result, e.g. WiFi, URL, etc...
        /// </summary>
        public BarcodeTypes BarcodeType { get; }

        /// <summary>
        /// Gets the barcode format, e.g. QRCode, EAN13, etc...
        /// </summary>
        public BarcodeFormats BarcodeFormat { get; }

        public string DisplayValue { get; }

        public string RawValue { get; }

        public byte[] RawBytes { get; }

        public RectF PreviewBoundingBox { get; internal set; }

        public RectF ImageBoundingBox { get; internal set; }

        public Point[] CornerPoints { get; }

        public bool Equals(BarcodeResult other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.DisplayValue == other.DisplayValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((BarcodeResult)obj);
        }

        public override int GetHashCode()
        {
            return this.DisplayValue != null ? this.DisplayValue?.GetHashCode() ?? 0 : 0;
        }

        public static bool operator ==(BarcodeResult left, BarcodeResult right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BarcodeResult left, BarcodeResult right)
        {
            return !Equals(left, right);
        }
    }
}
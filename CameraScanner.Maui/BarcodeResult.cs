namespace CameraScanner.Maui
{
    public class BarcodeResult : IEquatable<BarcodeResult>
    {
        public BarcodeResult(string displayValue)
        {
            this.DisplayValue = displayValue;
        }

        /// <summary>
        /// Gets the barcode type of the scan result, e.g. WiFi, URL, etc...
        /// </summary>
        public BarcodeTypes BarcodeType { get; set; }

        /// <summary>
        /// Gets the barcode format, e.g. QRCode, EAN13, etc...
        /// </summary>
        public BarcodeFormats BarcodeFormat { get; set; }

        public string DisplayValue { get; }

        public string RawValue { get; set; }

        public byte[] RawBytes { get; set; }

        public RectF PreviewBoundingBox { get; set; }

        public RectF ImageBoundingBox { get; set; }

        public Point[] CornerPoints { get; set; }

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
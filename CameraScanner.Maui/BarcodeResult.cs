namespace CameraScanner.Maui
{
    public class BarcodeResult : IEquatable<BarcodeResult>
    {
        /// <summary>
        /// Gets the barcode type of the scan result, e.g. WiFi, URL, etc...
        /// </summary>
        public BarcodeTypes BarcodeType { get; set; }

        /// <summary>
        /// Gets the barcode format, e.g. QRCode, EAN13, etc...
        /// </summary>
        public BarcodeFormats BarcodeFormat { get; set; }

        public string DisplayValue { get; set; }

        public string RawValue { get; set; }

        public byte[] RawBytes { get; set; }

        public RectF PreviewBoundingBox { get; set; }

        public RectF ImageBoundingBox { get; set; }

        public bool Equals(BarcodeResult other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.RawValue == other.RawValue && this.ImageBoundingBox.IntersectsWith(other.ImageBoundingBox))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            else if (obj is not BarcodeResult barcode)
            {
                return false;
            }
            else
            {
                return base.Equals(barcode);
            }
        }
        public override int GetHashCode()
        {
            return this.RawValue?.GetHashCode() ?? 0;
        }
    }
}
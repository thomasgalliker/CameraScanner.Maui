namespace CameraScanner.Maui.Extensions
{
    public static class BarcodeFormatsExtensions
    {
        public static BarcodeFormats[] ToArray(this BarcodeFormats barcodeFormats)
        {
            return Enum.GetValues(typeof(BarcodeFormats))
                .Cast<BarcodeFormats>()
                .Where(role => barcodeFormats.HasFlag(role) &&
                               role != BarcodeFormats.None &&
                               role != BarcodeFormats.All &&
                               role != BarcodeFormats.All1D &&
                               role != BarcodeFormats.All2D)
                .Select(role => role)
                .ToArray();
        }

        public static BarcodeFormats ToEnum(this IEnumerable<BarcodeFormats> barcodeFormats)
        {
            if (barcodeFormats == null || !barcodeFormats.Any())
            {
                return BarcodeFormats.None;
            }

            return barcodeFormats
                .Aggregate((x, y) => x | y);
        }
    }
}
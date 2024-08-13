
namespace CameraScanner.Maui
{
    public interface IBarcodeScanner
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IBarcodeScanner"/>.
        /// </summary>
        public static IBarcodeScanner Current { get; set; } = BarcodeScanner.Current;

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(byte[] imageArray);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(FileResult file);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(string url);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(Stream stream);
    }
}
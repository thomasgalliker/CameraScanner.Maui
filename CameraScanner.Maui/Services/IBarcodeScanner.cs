
namespace CameraScanner.Maui
{
    public interface IBarcodeScanner
    {
        Task<HashSet<BarcodeResult>> ScanFromImageAsync(byte[] imageArray);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(FileResult file);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(string url);

        Task<HashSet<BarcodeResult>> ScanFromImageAsync(Stream stream);
    }
}
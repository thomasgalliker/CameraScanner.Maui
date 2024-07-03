namespace CameraScanner.Maui
{
    public static class BarcodeScanner
    {
        private static Lazy<IBarcodeScanner> Implementation = new Lazy<IBarcodeScanner>(CreateBarcodeScanner, LazyThreadSafetyMode.PublicationOnly);

        public static IBarcodeScanner Current
        {
            get => Implementation.Value;
        }

        private static IBarcodeScanner CreateBarcodeScanner()
        {
#if (ANDROID || IOS)
            return new Platforms.Services.BarcodeScanner();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }
    }
}

namespace CameraScanner.Maui
{
    [Flags]
    public enum BarcodeFormats
    {
        None        = 0,

        // Common formats on both platforms
        Aztec       = 1 << 0,  // 1
        Code128     = 1 << 1,  // 2
        Code39      = 1 << 2,  // 4
        Code93      = 1 << 3,  // 8
        DataMatrix  = 1 << 4,  // 16
        Ean13       = 1 << 5,  // 32
        Ean8        = 1 << 6,  // 64
        ITF         = 1 << 7,  // 128
        Pdf417      = 1 << 8,  // 256
        QR          = 1 << 9,  // 512
        UPC_A       = 1 << 10, // 1024
        UPC_E       = 1 << 11, // 2048
        Codabar     = 1 << 12, // 4096

        // Apple Vision only
        GS1DataBar  = 1 << 13, // 8192
        I2OF5       = 1 << 14, // 16384
        MicroQR     = 1 << 15, // 32768
        MicroPdf417 = 1 << 16, // 65536

        All1D = Code128 | Code39 | Code93 | Ean13 | Ean8 | ITF | UPC_A | UPC_E | Codabar | GS1DataBar | I2OF5,
        
        All2D = Aztec | DataMatrix | QR | MicroQR | Pdf417 | MicroPdf417,

        All = All1D | All2D,
    }
}
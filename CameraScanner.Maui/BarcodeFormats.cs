using System.Runtime.Versioning;

namespace CameraScanner.Maui
{
    [Flags]
    public enum BarcodeFormats
    {
        None               = 0,

        // Common formats on both platforms
        Aztec              = 1 << 0, // 1
        Code128            = 1 << 1, // 2
        Code39             = 1 << 2, // 4
        Code93             = 1 << 3, // 8
        DataMatrix         = 1 << 4, // 16
        Ean13              = 1 << 5, // 32
        Ean8               = 1 << 6, // 64
        ITF                = 1 << 7, // 128
        Pdf417             = 1 << 8, // 256
        QR                 = 1 << 9, // 512
        UPC_A              = 1 << 10, // 1024
        UPC_E              = 1 << 11, // 2048
        Codabar            = 1 << 12, // 4096

        // Apple Vision only
        GS1DataBar         = 1 << 13, // 8192
        GS1DataBarExpanded = 1 << 14, // 16384
        GS1DataBarLimited  = 1 << 15, // 32768
        I2OF5              = 1 << 16, // 65536
        MicroQR            = 1 << 17, // 131072
        MicroPdf417        = 1 << 18, // 262144

        // Android only
        Code39Mod43        = 1 << 19, // 524288
        MSI                = 1 << 20, // 1048576
        Plessey            = 1 << 21, // 2097152
        RSS14              = 1 << 22, // 4194304
        RSSExpanded        = 1 << 23, // 8388608
        UPCEAN             = 1 << 24, // 16777216

        All = Aztec | Code128 | Code39 | Code93 | DataMatrix | Ean13 | Ean8 | ITF | Pdf417 | QR |
              UPC_A | UPC_E | Codabar | GS1DataBar | GS1DataBarExpanded | GS1DataBarLimited | I2OF5 |
              MicroQR | MicroPdf417 | Code39Mod43 | MSI | Plessey | RSS14 | RSSExpanded | UPCEAN
    }
}
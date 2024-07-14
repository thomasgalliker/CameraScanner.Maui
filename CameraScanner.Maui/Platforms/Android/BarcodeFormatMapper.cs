using Xamarin.Google.MLKit.Vision.Barcode.Common;

namespace CameraScanner.Maui
{
    internal static class BarcodeFormatMapper
    {
        public static int ToPlatform(this BarcodeFormats barcodeFormats)
        {
            var formats = Barcode.FormatAllFormats;

            if (barcodeFormats.HasFlag(BarcodeFormats.All))
            {
                formats = Barcode.FormatAllFormats;
            }
            else
            {
                if (barcodeFormats.HasFlag(BarcodeFormats.Code128))
                {
                    formats |= Barcode.FormatCode128;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Code39))
                {
                    formats |= Barcode.FormatCode39;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Code93))
                {
                    formats |= Barcode.FormatCode93;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Codabar))
                {
                    formats |= Barcode.FormatCodabar;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.DataMatrix))
                {
                    formats |= Barcode.FormatDataMatrix;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Ean13))
                {
                    formats |= Barcode.FormatEan13;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Ean8))
                {
                    formats |= Barcode.FormatEan8;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.ITF))
                {
                    formats |= Barcode.FormatItf;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.QR))
                {
                    formats |= Barcode.FormatQrCode;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.UPC_A))
                {
                    formats |= Barcode.FormatUpcA;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.UPC_E))
                {
                    formats |= Barcode.FormatUpcE;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Pdf417))
                {
                    formats |= Barcode.FormatPdf417;
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.Aztec))
                {
                    formats |= Barcode.FormatAztec;
                }
            }

            return formats;
        }

    }
}
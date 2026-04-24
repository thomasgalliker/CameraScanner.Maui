using UIKit;
using Vision;

namespace CameraScanner.Maui
{
    internal static class BarcodeFormatMapper
    {
        public static VNBarcodeSymbology[] ToPlatform(this BarcodeFormats barcodeFormats)
        {
            if (barcodeFormats.HasFlag(BarcodeFormats.All))
            {
                return [];
            }

            var symbologiesList = new List<VNBarcodeSymbology>();

            if (barcodeFormats.HasFlag(BarcodeFormats.Code128))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code128);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Code39))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code39);
                symbologiesList.Add(VNBarcodeSymbology.Code39Checksum);
                symbologiesList.Add(VNBarcodeSymbology.Code39FullAscii);
                symbologiesList.Add(VNBarcodeSymbology.Code39FullAsciiChecksum);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Code93))
            {
                symbologiesList.Add(VNBarcodeSymbology.Code93);
                symbologiesList.Add(VNBarcodeSymbology.Code93i);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.DataMatrix))
            {
                symbologiesList.Add(VNBarcodeSymbology.DataMatrix);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Ean13))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean13);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Ean8))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean8);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.ITF))
            {
                symbologiesList.Add(VNBarcodeSymbology.Itf14);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.QR))
            {
                symbologiesList.Add(VNBarcodeSymbology.QR);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.UPC_A))
            {
                symbologiesList.Add(VNBarcodeSymbology.Ean13);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.UPC_E))
            {
                symbologiesList.Add(VNBarcodeSymbology.Upce);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Pdf417))
            {
                symbologiesList.Add(VNBarcodeSymbology.Pdf417);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.Aztec))
            {
                symbologiesList.Add(VNBarcodeSymbology.Aztec);
            }

            if (barcodeFormats.HasFlag(BarcodeFormats.I2OF5))
            {
                symbologiesList.Add(VNBarcodeSymbology.I2OF5);
                symbologiesList.Add(VNBarcodeSymbology.I2OF5Checksum);
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                if (barcodeFormats.HasFlag(BarcodeFormats.Codabar))
                {
                    symbologiesList.Add(VNBarcodeSymbology.Codabar);
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.GS1DataBar))
                {
                    symbologiesList.Add(VNBarcodeSymbology.GS1DataBar);
                    symbologiesList.Add(VNBarcodeSymbology.GS1DataBarExpanded);
                    symbologiesList.Add(VNBarcodeSymbology.GS1DataBarLimited);
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.MicroQR))
                {
                    symbologiesList.Add(VNBarcodeSymbology.MicroQR);
                }

                if (barcodeFormats.HasFlag(BarcodeFormats.MicroPdf417))
                {
                    symbologiesList.Add(VNBarcodeSymbology.MicroPdf417);
                }
            }

            return symbologiesList.ToArray();
        }

        public static BarcodeFormats ToBarcodeFormats(this VNBarcodeSymbology barcodeSymbology)
        {
            return barcodeSymbology switch
            {
                VNBarcodeSymbology.Aztec => BarcodeFormats.Aztec,
                VNBarcodeSymbology.Code39 => BarcodeFormats.Code39,
                VNBarcodeSymbology.Code39Checksum => BarcodeFormats.Code39,
                VNBarcodeSymbology.Code39FullAscii => BarcodeFormats.Code39,
                VNBarcodeSymbology.Code39FullAsciiChecksum => BarcodeFormats.Code39,
                VNBarcodeSymbology.Code93 => BarcodeFormats.Code93,
                VNBarcodeSymbology.Code93i => BarcodeFormats.Code93,
                VNBarcodeSymbology.Code128 => BarcodeFormats.Code128,
                VNBarcodeSymbology.DataMatrix => BarcodeFormats.DataMatrix,
                VNBarcodeSymbology.Ean8 => BarcodeFormats.Ean8,
                VNBarcodeSymbology.Ean13 => BarcodeFormats.Ean13,
                VNBarcodeSymbology.I2OF5 => BarcodeFormats.I2OF5,
                VNBarcodeSymbology.I2OF5Checksum => BarcodeFormats.I2OF5,
                VNBarcodeSymbology.Itf14 => BarcodeFormats.ITF,
                VNBarcodeSymbology.Pdf417 => BarcodeFormats.Pdf417,
                VNBarcodeSymbology.QR => BarcodeFormats.QR,
                VNBarcodeSymbology.Upce => BarcodeFormats.UPC_E,
                VNBarcodeSymbology.Codabar => BarcodeFormats.Codabar,
                VNBarcodeSymbology.GS1DataBar => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.GS1DataBarExpanded => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.GS1DataBarLimited => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.MicroPdf417 => BarcodeFormats.MicroPdf417,
                VNBarcodeSymbology.MicroQR => BarcodeFormats.MicroQR,
                _ => BarcodeFormats.None
            };
        }
    }
}
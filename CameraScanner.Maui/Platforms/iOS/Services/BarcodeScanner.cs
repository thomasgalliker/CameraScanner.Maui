using AVFoundation;
using CameraScanner.Maui;
using CoreGraphics;
using CoreImage;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using System.Text;
using UIKit;
using Vision;

namespace CameraScanner.Maui
{
    public class BarcodeScanner : IBarcodeScanner
    {
        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(byte[] imageArray)
        {
            return await ProcessBitmapAsync(UIImage.LoadFromData(NSData.FromArray(imageArray)));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(FileResult file)
        {
            return await ProcessBitmapAsync(UIImage.LoadFromData(NSData.FromStream(await file.OpenReadAsync())));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(string url)
        {
            return await ProcessBitmapAsync(UIImage.LoadFromData(NSData.FromUrl(new NSUrl(url))));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(Stream stream)
        {
            return await ProcessBitmapAsync(UIImage.LoadFromData(NSData.FromStream(stream)));
        }

        private static async Task<HashSet<BarcodeResult>> ProcessBitmapAsync(UIImage image)
        {
            if (image is null)
            {
                return null;
            }

            VNBarcodeObservation[] observations = null;
            using var barcodeRequest = new VNDetectBarcodesRequest((request, error) =>
            {
                if (error is null)
                {
                    observations = request.GetResults<VNBarcodeObservation>();
                }
            });
            using var handler = new VNImageRequestHandler(image.CGImage, new NSDictionary());
            await Task.Run(() => handler.Perform([barcodeRequest], out _));
            var barcodeResults = new HashSet<BarcodeResult>();
            ProcessBarcodeResult(observations, barcodeResults);
            return barcodeResults;
        }

        internal static void ProcessBarcodeResult(VNBarcodeObservation[] inputResults, HashSet<BarcodeResult> outputResults,
            AVCaptureVideoPreviewLayer previewLayer = null)
        {
            if (inputResults is null || inputResults.Length == 0)
            {
                return;
            }

            lock (outputResults)
            {
                foreach (var barcode in inputResults)
                {
                    outputResults.Add(new BarcodeResult
                    {
                        BarcodeType = BarcodeTypes.Unknown, // TODO: Implement mapping
                        BarcodeFormat = ConvertFromIOSFormats(barcode.Symbology),
                        DisplayValue = barcode.PayloadStringValue,
                        RawValue = barcode.PayloadStringValue,
                        RawBytes = GetRawBytes(barcode) ?? Encoding.ASCII.GetBytes(barcode.PayloadStringValue),
                        PreviewBoundingBox = previewLayer?.MapToLayerCoordinates(InvertY(barcode.BoundingBox)).AsRectangleF() ?? RectF.Zero,
                        ImageBoundingBox = barcode.BoundingBox.AsRectangleF()
                    });
                }
            }
        }

        private static BarcodeFormats ConvertFromIOSFormats(VNBarcodeSymbology symbology)
        {
            return symbology switch
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
                VNBarcodeSymbology.Itf14 => BarcodeFormats.Itf,
                VNBarcodeSymbology.Pdf417 => BarcodeFormats.Pdf417,
                VNBarcodeSymbology.QR => BarcodeFormats.QRCode,
                VNBarcodeSymbology.Upce => BarcodeFormats.Upce,
                VNBarcodeSymbology.Codabar => BarcodeFormats.CodaBar,
                VNBarcodeSymbology.GS1DataBar => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.GS1DataBarExpanded => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.GS1DataBarLimited => BarcodeFormats.GS1DataBar,
                VNBarcodeSymbology.MicroPdf417 => BarcodeFormats.MicroPdf417,
                VNBarcodeSymbology.MicroQR => BarcodeFormats.MicroQR,
                _ => BarcodeFormats.None
            };
        }

        private static CGRect InvertY(CGRect rect)
        {
            return new CGRect(rect.X, 1 - rect.Y - rect.Height, rect.Width, rect.Height);
        }

        private static byte[] GetRawBytes(VNBarcodeObservation barcodeObservation)
        {
            return barcodeObservation.Symbology switch
            {
                VNBarcodeSymbology.QR => ((CIQRCodeDescriptor)barcodeObservation.BarcodeDescriptor)?.ErrorCorrectedPayload?.ToArray(),
                VNBarcodeSymbology.Aztec => ((CIAztecCodeDescriptor)barcodeObservation.BarcodeDescriptor)?.ErrorCorrectedPayload?.ToArray(),
                VNBarcodeSymbology.Pdf417 => ((CIPdf417CodeDescriptor)barcodeObservation.BarcodeDescriptor)?.ErrorCorrectedPayload
                    ?.ToArray(),
                VNBarcodeSymbology.DataMatrix => ((CIDataMatrixCodeDescriptor)barcodeObservation.BarcodeDescriptor)?.ErrorCorrectedPayload
                    ?.ToArray(),
                _ => null
            };
        }

    }
}
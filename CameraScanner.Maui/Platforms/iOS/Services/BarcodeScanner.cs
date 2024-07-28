using System.Text;
using AVFoundation;
using CameraScanner.Maui.Platforms.Extensions;
using CoreGraphics;
using CoreImage;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using UIKit;
using Vision;

namespace CameraScanner.Maui.Platforms.Services
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
                    var previewBoundingBox = previewLayer?.MapToLayerCoordinates(barcode.BoundingBox.InvertY()).AsRectangleF() ?? RectF.Zero;
                    var imageBoundingBox = barcode.BoundingBox.AsRectangleF();

                    outputResults.Add(new BarcodeResult
                    {
                        BarcodeType = BarcodeTypes.Unknown, // TODO: Implement mapping
                        BarcodeFormat = barcode.Symbology.ToBarcodeFormats(),
                        DisplayValue = barcode.PayloadStringValue,
                        RawValue = barcode.PayloadStringValue,
                        RawBytes = GetRawBytes(barcode) ?? Encoding.ASCII.GetBytes(barcode.PayloadStringValue),
                        PreviewBoundingBox = previewBoundingBox,
                        ImageBoundingBox = imageBoundingBox,
                    });
                }
            }
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
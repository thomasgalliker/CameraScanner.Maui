using System.Text;
using AVFoundation;
using CameraScanner.Maui.Platforms.Extensions;
using CoreGraphics;
using CoreImage;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Platform;
using UIKit;
using Vision;

namespace CameraScanner.Maui.Platforms.Services
{
    public class BarcodeScanner : IBarcodeScanner
    {
        private static readonly CGSize OneByOneSize = new CGSize(1f, 1f);

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
                    RectF previewBoundingBox;
                    Point[] cornerPoints;

                    if (previewLayer != null)
                    {
                        var boundingBoxInvertedY = barcode.BoundingBox.InvertY();
                        previewBoundingBox = previewLayer.MapToLayerCoordinates(boundingBoxInvertedY).AsRectangleF();

                        var topLeft = GetPoint(previewLayer, barcode.TopLeft);
                        var topRight = GetPoint(previewLayer, barcode.TopRight);
                        var bottomLeft = GetPoint(previewLayer, barcode.BottomLeft);
                        var bottomRight = GetPoint(previewLayer, barcode.BottomRight);

                        cornerPoints = new[]
                        {
                            topLeft,
                            bottomLeft,
                            bottomRight,
                            topRight,
                        };
                    }
                    else
                    {
                        previewBoundingBox = RectF.Zero;
                        cornerPoints = Array.Empty<Point>();
                    }

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
                        CornerPoints = cornerPoints,
                    });
                }
            }
        }

        private static Point GetPoint(AVCaptureVideoPreviewLayer previewLayer, CGPoint cornerPoint)
        {
            var rectF = previewLayer.MapToLayerCoordinates(new CGRect(cornerPoint, OneByOneSize).InvertY());
            return new Point(rectF.X, rectF.Y);
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
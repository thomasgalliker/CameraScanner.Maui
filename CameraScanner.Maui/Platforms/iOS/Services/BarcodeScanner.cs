using System.Text;
using AVFoundation;
using CameraScanner.Maui.Platforms.Extensions;
using CoreGraphics;
using CoreImage;
using CoreML;
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

            void VNDetectBarcodesRequestCompletionHandler(VNRequest request, NSError error)
            {
                if (error is null)
                {
                    observations = request.GetResults<VNBarcodeObservation>();
                }
                else
                {
                    // TODO Log error
                }
            }

            using (var barcodeRequest = new VNDetectBarcodesRequest(VNDetectBarcodesRequestCompletionHandler))
            {
#if DEBUG
                if (DeviceInfo.DeviceType == DeviceType.Virtual)
                {
                    // The iOS simulator does not have dedicated ML hardware
                    // but why does it not automatically enable CPU-based barcode detection?
                    barcodeRequest.UsesCpuOnly = true;

                    // For some reason the iOS simulator does not return barcode detection results
                    // if we use the latest revision.
                    barcodeRequest.Revision = (VNDetectBarcodesRequestRevision)2;
                }
#endif

                using (var handler = new VNImageRequestHandler(image.CGImage, new NSDictionary()))
                {
                    await Task.Run(() =>
                    {
                        if (!handler.Perform([barcodeRequest], out var error))
                        {
                        }
                    });

                    var barcodeResults = ProcessBarcodeResult(observations);
                    return barcodeResults;
                }
            }
        }

        internal static HashSet<BarcodeResult> ProcessBarcodeResult(VNBarcodeObservation[] inputResults, AVCaptureVideoPreviewLayer previewLayer = null)
        {
            var barcodeResults = new HashSet<BarcodeResult>();

            if (inputResults is null || inputResults.Length == 0)
            {
                return barcodeResults;
            }

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

                    cornerPoints = new[] { topLeft, bottomLeft, bottomRight, topRight, };
                }
                else
                {
                    previewBoundingBox = RectF.Zero;
                    cornerPoints = Array.Empty<Point>();
                }

                var imageBoundingBox = barcode.BoundingBox.AsRectangleF();

                // TODO: Implement mapping for BarcodeTypes

                var barcodeResult = new BarcodeResult(
                    displayValue: barcode.PayloadStringValue,
                    barcodeType: BarcodeTypes.Unknown,
                    barcodeFormat: barcode.Symbology.ToBarcodeFormats(),
                    rawValue: barcode.PayloadStringValue,
                    rawBytes: GetRawBytes(barcode) ?? Encoding.ASCII.GetBytes(barcode.PayloadStringValue),
                    previewBoundingBox: previewBoundingBox,
                    imageBoundingBox: imageBoundingBox,
                    cornerPoints: cornerPoints);

                barcodeResults.Add(barcodeResult);
            }

            return barcodeResults;
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
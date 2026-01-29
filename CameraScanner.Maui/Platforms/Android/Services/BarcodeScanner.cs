using Android.Gms.Extensions;
using Android.Graphics;
using Android.Runtime;
using AndroidX.Camera.View.Transform;
using Java.Net;
using Java.Util;
using Microsoft.Maui.Graphics.Platform;
using Xamarin.Google.MLKit.Vision.Barcode.Common;
using Xamarin.Google.MLKit.Vision.BarCode;
using Xamarin.Google.MLKit.Vision.Common;
using Image = Android.Media.Image;
using Paint = Android.Graphics.Paint;
using Point = Microsoft.Maui.Graphics.Point;
using RectF = Microsoft.Maui.Graphics.RectF;
using System.Linq;

namespace CameraScanner.Maui.Platforms.Services
{
    public partial class BarcodeScanner : IBarcodeScanner
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(byte[] imageArray)
        {
            ArgumentNullException.ThrowIfNull(imageArray);

            var bitmap = await BitmapFactory.DecodeByteArrayAsync(imageArray, 0, imageArray.Length);
            if (bitmap == null)
            {
                throw new InvalidOperationException("ScanFromImageAsync failed to load bitmap using BitmapFactory.DecodeByteArrayAsync");
            }

            return await ProcessBitmapAsync(bitmap);
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(FileResult file)
        {
            ArgumentNullException.ThrowIfNull(file);

            var stream = await file.OpenReadAsync();
            return await this.ScanFromImageAsync(stream);
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(string url)
        {
            ArgumentNullException.ThrowIfNull(url);

            var stream = new URL(url).OpenStream();
            return await this.ScanFromImageAsync(stream);
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(Stream? stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            var bitmap = await BitmapFactory.DecodeStreamAsync(stream);
            if (bitmap == null)
            {
                throw new InvalidOperationException("ScanFromImageAsync failed to load bitmap using BitmapFactory.DecodeStreamAsync");
            }

            return await ProcessBitmapAsync(bitmap);
        }

        private static async Task<HashSet<BarcodeResult>> ProcessBitmapAsync(Bitmap bitmap)
        {
            ArgumentNullException.ThrowIfNull(bitmap);

            var barcodeResults = new HashSet<BarcodeResult>();
            using var scanner = Xamarin.Google.MLKit.Vision.BarCode.BarcodeScanning.GetClient(new BarcodeScannerOptions.Builder()
                .SetBarcodeFormats(Barcode.FormatAllFormats)
                .Build());

            using var image = InputImage.FromBitmap(bitmap, 0);
            var results = ProcessBarcodeResult(await scanner.Process(image));
            barcodeResults.UnionWith(results);

            using var invertedBitmap = Bitmap.CreateBitmap(bitmap.Height, bitmap.Width, Bitmap.Config.Argb8888!);
            using var canvas = new Canvas(invertedBitmap);
            using var paint = new Paint();
            using var matrixInvert = new ColorMatrix();

            matrixInvert.Set(
            [
                -1.0f, 0.0f, 0.0f, 0.0f, 255.0f,
                0.0f, -1.0f, 0.0f, 0.0f, 255.0f,
                0.0f, 0.0f, -1.0f, 0.0f, 255.0f,
                0.0f, 0.0f, 0.0f, 1.0f, 0.0f
            ]);

            using var filter = new ColorMatrixColorFilter(matrixInvert);
            paint.SetColorFilter(filter);
            canvas.DrawBitmap(bitmap, 0, 0, paint);

            using (var invertedImage = InputImage.FromBitmap(invertedBitmap, 0))
            {
                var resultsInverted = ProcessBarcodeResult(await scanner.Process(invertedImage));
                barcodeResults.UnionWith(resultsInverted);
            }

            return barcodeResults;
        }

        internal static HashSet<BarcodeResult> ProcessBarcodeResult(Java.Lang.Object? inputResults, CoordinateTransform? transform = null)
        {
            var barcodeResults = new HashSet<BarcodeResult>();

            if (inputResults is null)
            {
                return barcodeResults;
            }

            using var inputResultsArrayList = inputResults.JavaCast<ArrayList>();
            if (inputResultsArrayList.IsEmpty)
            {
                return barcodeResults;
            }

            foreach (var inputResult in inputResultsArrayList.ToArray())
            {
                using var barcode = inputResult.JavaCast<Barcode>();
                var boundingBox = barcode.BoundingBox;
                using var rectF = boundingBox.AsRectF();
                var imageRect = rectF.AsRectangleF();

                RectF previewRect;
                Point[] cornerPoints;

                if (transform != null)
                {
                    transform.MapRect(rectF);
                    previewRect = rectF.AsRectangleF();

                    cornerPoints = barcode.GetCornerPoints()?
                        .Select(p =>
                        {
                            var pointF = new global::Android.Graphics.PointF(p);
                            transform.MapPoint(pointF);
                            return pointF;
                        })
                        .Select(p => new Point(p.X, p.Y))
                        .ToArray() ?? Array.Empty<Point>();
                }
                else
                {
                    previewRect = rectF.AsRectangleF();
                    cornerPoints = barcode.GetCornerPoints()?
                       .Select(p =>
                       {
                           var pointF = new global::Android.Graphics.PointF(p);
                           return pointF;
                       })
                       .Select(p => new Point(p.X, p.Y))
                       .ToArray() ?? Array.Empty<Point>();
                }

                var barcodeResult = new BarcodeResult(
                    displayValue: barcode.DisplayValue,
                    barcodeType: ConvertBarcodeResultTypes(barcode.ValueType),
                    barcodeFormat: ConvertBarcodeFormat(barcode),
                    rawValue: barcode.RawValue,
                    rawBytes: barcode.GetRawBytes(),
                    previewBoundingBox: previewRect,
                    imageBoundingBox: imageRect,
                    cornerPoints: cornerPoints);

                barcodeResults.Add(barcodeResult);
            }

            return barcodeResults;
        }

        private static BarcodeFormats ConvertBarcodeFormat(Barcode barcode)
        {
            // TODO: Add more safety + logging here
            return (BarcodeFormats)barcode.Format;
        }

        internal static void InvertLuminance(Image image)
        {
            var yBuffer = image.GetPlanes()?[0].Buffer;
            if (yBuffer != null)
            {
                if (yBuffer.IsDirect)
                {
                    unsafe
                    {
                        var data = (ulong*)yBuffer.GetDirectBufferAddress();
                        Parallel.For(0, yBuffer.Capacity() / sizeof(ulong), ParallelOptions, i => data[i] = ~data[i]);
                    }
                }
                else
                {
                    using var bits = BitSet.ValueOf(yBuffer);
                    if (bits != null)
                    {
                        bits.Flip(0, bits.Length());
                        yBuffer.Rewind();
                        var byteArray = bits.ToByteArray();
                        if (byteArray != null)
                        {
                            yBuffer.Put(byteArray);
                        }
                    }
                }
            }
        }

        internal static BarcodeTypes ConvertBarcodeResultTypes(int barcodeValueType)
        {
            return barcodeValueType switch
            {
                Barcode.TypeCalendarEvent => BarcodeTypes.CalendarEvent,
                Barcode.TypeContactInfo => BarcodeTypes.ContactInfo,
                Barcode.TypeDriverLicense => BarcodeTypes.DriversLicense,
                Barcode.TypeEmail => BarcodeTypes.Email,
                Barcode.TypeGeo => BarcodeTypes.GeographicCoordinates,
                Barcode.TypeIsbn => BarcodeTypes.Isbn,
                Barcode.TypePhone => BarcodeTypes.Phone,
                Barcode.TypeProduct => BarcodeTypes.Product,
                Barcode.TypeSms => BarcodeTypes.Sms,
                Barcode.TypeText => BarcodeTypes.Text,
                Barcode.TypeUrl => BarcodeTypes.Url,
                Barcode.TypeWifi => BarcodeTypes.WiFi,
                _ => BarcodeTypes.Unknown
            };
        }
    }
}
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
using RectF = Microsoft.Maui.Graphics.RectF;

namespace CameraScanner.Maui.Platforms.Services
{
    public partial class BarcodeScanner : IBarcodeScanner
    {
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(byte[] imageArray)
        {
            return await ProcessBitmapAsync(await BitmapFactory.DecodeByteArrayAsync(imageArray, 0, imageArray.Length));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(FileResult file)
        {
            return await ProcessBitmapAsync(await BitmapFactory.DecodeStreamAsync(await file.OpenReadAsync()));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(string url)
        {
            return await ProcessBitmapAsync(await BitmapFactory.DecodeStreamAsync(new URL(url).OpenStream()));
        }

        public async Task<HashSet<BarcodeResult>> ScanFromImageAsync(Stream stream)
        {
            return await ProcessBitmapAsync(await BitmapFactory.DecodeStreamAsync(stream));
        }

        private static async Task<HashSet<BarcodeResult>> ProcessBitmapAsync(Bitmap bitmap)
        {
            if (bitmap is null)
            {
                return null;
            }

            var barcodeResults = new HashSet<BarcodeResult>();
            using var scanner = Xamarin.Google.MLKit.Vision.BarCode.BarcodeScanning.GetClient(new BarcodeScannerOptions.Builder()
                .SetBarcodeFormats(Barcode.FormatAllFormats)
                .Build());

            using var image = InputImage.FromBitmap(bitmap, 0);
            ProcessBarcodeResult(await scanner.Process(image), barcodeResults);

            using var invertedBitmap = Bitmap.CreateBitmap(bitmap.Height, bitmap.Width, Bitmap.Config.Argb8888);
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

            using var invertedImage = InputImage.FromBitmap(invertedBitmap, 0);
            ProcessBarcodeResult(await scanner.Process(invertedImage), barcodeResults);

            return barcodeResults;
        }

        internal static void ProcessBarcodeResult(Java.Lang.Object inputResults, HashSet<BarcodeResult> outputResults, CoordinateTransform transform = null)
        {
            if (inputResults is null)
            {
                return;
            }

            using var inputResultsArrayList = inputResults.JavaCast<ArrayList>();
            if (inputResultsArrayList?.IsEmpty ?? true)
            {
                return;
            }

            foreach (var inputResult in inputResultsArrayList.ToArray())
            {
                using var barcode = inputResult.JavaCast<Barcode>();
                if (barcode is null)
                {
                    continue;
                }

                var boundingBox = barcode.BoundingBox;
                using var rectF = boundingBox.AsRectF();
                var imageRect = rectF.AsRectangleF();

                RectF previewRect;

                if (transform != null)
                {
                    transform.MapRect(rectF);
                    previewRect = rectF.AsRectangleF();
                }
                else
                {
                    previewRect = RectF.Zero;
                }
      

                outputResults.Add(new BarcodeResult
                {
                    BarcodeType = ConvertBarcodeResultTypes(barcode.ValueType),
                    BarcodeFormat = (BarcodeFormats)barcode.Format,
                    DisplayValue = barcode.DisplayValue,
                    RawValue = barcode.RawValue,
                    RawBytes = barcode.GetRawBytes(),
                    PreviewBoundingBox = previewRect,
                    ImageBoundingBox = imageRect
                });
            }
        }

        internal static void InvertLuminance(Image image)
        {
            var yBuffer = image.GetPlanes()[0].Buffer;
            if (yBuffer.IsDirect)
            {
                unsafe
                {
                    var data = (ulong*)yBuffer.GetDirectBufferAddress();
                    Parallel.For(0, yBuffer.Capacity() / sizeof(ulong), ParallelOptions, (i) => data[i] = ~data[i]);
                }
            }
            else
            {
                using var bits = BitSet.ValueOf(yBuffer);
                bits.Flip(0, bits.Length());
                yBuffer.Rewind();
                yBuffer.Put(bits.ToByteArray());
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
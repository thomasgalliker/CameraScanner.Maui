using CameraScanner.Maui.Utils;

namespace CameraScanner.Maui.Controls
{
    public partial class BarcodeResultOverlay : GraphicsView
    {
        private readonly TaskDelayer taskDelayer;

        public BarcodeResultOverlay()
        {
            this.InitializeComponent();
            this.taskDelayer = new TaskDelayer();
        }

        public static readonly BindableProperty BarcodeResultsProperty = BindableProperty.Create(
            nameof(BarcodeResults),
            typeof(BarcodeResult[]),
            typeof(CameraView),
            propertyChanged: OnBarcodeResultsPropertyChanged);

        private static void OnBarcodeResultsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (newValue is BarcodeResult[] barcodeResults)
            {
                UpdateDrawable(barcodeResultOverlay, barcodeResults, barcodeResultOverlay.StrokeSize, barcodeResultOverlay.StrokeColor);
            }
        }

        public BarcodeResult[] BarcodeResults
        {
            get => (BarcodeResult[])this.GetValue(BarcodeResultsProperty);
            set => this.SetValue(BarcodeResultsProperty, value);
        }

        public static readonly BindableProperty StrokeSizeProperty = BindableProperty.Create(
            nameof(StrokeSize),
            typeof(float),
            typeof(CameraView),
            8f,
            propertyChanged: OnStrokeSizePropertyChanged);

        private static void OnStrokeSizePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (newValue is float strokeSize)
            {
                UpdateDrawable(barcodeResultOverlay, barcodeResultOverlay.BarcodeResults, strokeSize, barcodeResultOverlay.StrokeColor);
            }
        }

        public float StrokeSize
        {
            get => (float)this.GetValue(StrokeSizeProperty);
            set => this.SetValue(StrokeSizeProperty, value);
        }

        public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
            nameof(StrokeColor),
            typeof(Color),
            typeof(CameraView),
            Colors.Red,
            propertyChanged: OnStrokeColorPropertyChanged);

        private static void OnStrokeColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (newValue is Color strokeColor)
            {
                UpdateDrawable(barcodeResultOverlay, barcodeResultOverlay.BarcodeResults, barcodeResultOverlay.StrokeSize, strokeColor);
            }
        }

        public Color StrokeColor
        {
            get => (Color)this.GetValue(StrokeColorProperty);
            set => this.SetValue(StrokeColorProperty, value);
        }

        private static void UpdateDrawable(BarcodeResultOverlay barcodeResultOverlay,
            BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor)
        {
            barcodeResultOverlay.BarcodeDrawable.Update(barcodeResults, strokeSize, strokeColor);
            barcodeResultOverlay.Invalidate();

            barcodeResultOverlay.taskDelayer.RunWithDelay(TimeSpan.FromSeconds(1), () =>
            {
                barcodeResultOverlay.BarcodeDrawable.Reset();
                MainThread.BeginInvokeOnMainThread(() => barcodeResultOverlay.Invalidate());
            });
        }
    }
}
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
            null,
            propertyChanged: OnBarcodeResultsPropertyChanged);

        private static void OnBarcodeResultsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (newValue is BarcodeResult[] barcodeResults)
            {
                barcodeResultOverlay.BarcodeDrawable.BarcodeResults = barcodeResults;
                barcodeResultOverlay.Invalidate();

                barcodeResultOverlay.taskDelayer.RunWithDelay(TimeSpan.FromSeconds(1), () =>
                {
                    barcodeResultOverlay.BarcodeDrawable.BarcodeResults = null;
                    MainThread.BeginInvokeOnMainThread(() => barcodeResultOverlay.Invalidate());
                });
            }
        }

        public BarcodeResult[] BarcodeResults
        {
            get => (BarcodeResult[])this.GetValue(BarcodeResultsProperty);
            set => this.SetValue(BarcodeResultsProperty, value);
        }
    }
}
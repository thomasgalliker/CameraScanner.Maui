using System.Windows.Input;
using CameraScanner.Maui.Utils;

namespace CameraScanner.Maui.Controls
{
    public partial class BarcodeResultOverlay : GraphicsView
    {
        private static readonly TimeSpan ClearResultOverlayDelay = TimeSpan.FromMilliseconds(500);

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
                UpdateDrawable(
                    barcodeResultOverlay,
                    barcodeResults,
                    barcodeResultOverlay.StrokeSize,
                    barcodeResultOverlay.StrokeColor,
                    barcodeResultOverlay.TextColor);
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
                UpdateDrawable(
                    barcodeResultOverlay,
                    barcodeResultOverlay.BarcodeResults,
                    strokeSize,
                    barcodeResultOverlay.StrokeColor,
                    barcodeResultOverlay.TextColor);
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
                UpdateDrawable(
                    barcodeResultOverlay,
                    barcodeResultOverlay.BarcodeResults,
                    barcodeResultOverlay.StrokeSize,
                    strokeColor,
                    barcodeResultOverlay.TextColor);
            }
        }

        public Color StrokeColor
        {
            get => (Color)this.GetValue(StrokeColorProperty);
            set => this.SetValue(StrokeColorProperty, value);
        }

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
            nameof(TextColor),
            typeof(Color),
            typeof(CameraView),
            Colors.White,
            propertyChanged: OnTextColorPropertyChanged);

        private static void OnTextColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (newValue is Color textColor)
            {
                UpdateDrawable(
                    barcodeResultOverlay,
                    barcodeResultOverlay.BarcodeResults,
                    barcodeResultOverlay.StrokeSize,
                    barcodeResultOverlay.StrokeColor,
                    textColor);
            }
        }

        public Color TextColor
        {
            get => (Color)this.GetValue(TextColorProperty);
            set => this.SetValue(TextColorProperty, value);
        }

        private static void UpdateDrawable(BarcodeResultOverlay barcodeResultOverlay,
            BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor)
        {
            barcodeResultOverlay.BarcodeDrawable.Update(barcodeResults, strokeSize, strokeColor, textColor);
            barcodeResultOverlay.Invalidate();

            barcodeResultOverlay.taskDelayer.RunWithDelay(ClearResultOverlayDelay, () =>
            {
                barcodeResultOverlay.BarcodeDrawable.Reset();
                MainThread.BeginInvokeOnMainThread(barcodeResultOverlay.Invalidate);
            });
        }

        private void TapGestureRecognizer_OnTapped(object sender, TappedEventArgs e)
        {
            var tabPosition = e.GetPosition(this);
            if (tabPosition is Point point)
            {
                var barcodeResult = this.BarcodeResults?.FirstOrDefault(r => r.PreviewBoundingBox.Contains(point));
                if (barcodeResult != null &&
                    this.BarcodeResultTappedCommand is ICommand barcodeResultTappedCommand &&
                    barcodeResultTappedCommand.CanExecute(barcodeResult))
                {
                    barcodeResultTappedCommand.Execute(barcodeResult);
                }
            }
        }

        public static readonly BindableProperty BarcodeResultTappedCommandProperty = BindableProperty.Create(
            nameof(BarcodeResultTappedCommand),
            typeof(ICommand),
            typeof(BarcodeResultOverlay));

        public ICommand BarcodeResultTappedCommand
        {
            get => (ICommand)this.GetValue(BarcodeResultTappedCommandProperty);
            set => this.SetValue(BarcodeResultTappedCommandProperty, value);
        }
    }
}
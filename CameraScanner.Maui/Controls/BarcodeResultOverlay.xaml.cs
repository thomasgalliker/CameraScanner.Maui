using System.Windows.Input;
using CameraScanner.Maui.Utils;
using Microsoft.Maui.Graphics.Text;

namespace CameraScanner.Maui.Controls
{
    public partial class BarcodeResultOverlay : GraphicsView
    {
        private readonly TaskDelayer taskDelayer;

        public BarcodeResultOverlay()
        {
            this.InitializeComponent();
            this.taskDelayer = new TaskDelayer();
            this.Drawable = this.BarcodeDrawable;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            //UpdateDrawable(
            //   this,
            //   this.BarcodeResults,
            //   this.StrokeSize,
            //   this.StrokeColor,
            //   this.TextColor,
            //   this.InvalidateDrawableAfter);
        }

        public static readonly BindableProperty BarcodeDrawableProperty = BindableProperty.Create(
            nameof(BarcodeDrawable),
            typeof(IBarcodeDrawable),
            typeof(BarcodeResultOverlay),
            new BoundingBoxBarcodeDrawable(),
            propertyChanged: OnBarcodeDrawablePropertyChanged);

        private static void OnBarcodeDrawablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            if (oldValue is IBarcodeDrawable oldBarcodeDrawable)
            {
                oldBarcodeDrawable.Reset();
            }

            if (newValue is IBarcodeDrawable newBarcodeDrawable)
            {
                barcodeResultOverlay.Drawable = newBarcodeDrawable;
            }
        }

        public IBarcodeDrawable BarcodeDrawable
        {
            get => (IBarcodeDrawable)this.GetValue(BarcodeDrawableProperty);
            set => this.SetValue(BarcodeDrawableProperty, value);
        }

        public static readonly BindableProperty BarcodeResultsProperty = BindableProperty.Create(
            nameof(BarcodeResults),
            typeof(BarcodeResult[]),
            typeof(BarcodeResultOverlay),
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
                    barcodeResultOverlay.TextColor,
                    barcodeResultOverlay.InvalidateDrawableAfter);
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
            typeof(BarcodeResultOverlay),
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
                    barcodeResultOverlay.TextColor,
                    barcodeResultOverlay.InvalidateDrawableAfter);
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
            typeof(BarcodeResultOverlay),
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
                    barcodeResultOverlay.TextColor,
                    barcodeResultOverlay.InvalidateDrawableAfter);
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
            typeof(BarcodeResultOverlay),
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
                    textColor,
                    barcodeResultOverlay.InvalidateDrawableAfter);
            }
        }

        public Color TextColor
        {
            get => (Color)this.GetValue(TextColorProperty);
            set => this.SetValue(TextColorProperty, value);
        }

        public static readonly BindableProperty InvalidateDrawableAfterProperty = BindableProperty.Create(
            nameof(InvalidateDrawableAfter),
            typeof(TimeSpan?),
            typeof(BarcodeResultOverlay),
            TimeSpan.FromMilliseconds(500),
            propertyChanged: OnInvalidateDrawableAfterPropertyChanged);


        private static void OnInvalidateDrawableAfterPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var barcodeResultOverlay = (BarcodeResultOverlay)bindable;

            UpdateDrawable(
                 barcodeResultOverlay,
                 barcodeResultOverlay.BarcodeResults,
                 barcodeResultOverlay.StrokeSize,
                 barcodeResultOverlay.StrokeColor,
                 barcodeResultOverlay.TextColor,
                 newValue as TimeSpan?);
        }

        public TimeSpan? InvalidateDrawableAfter
        {
            get => (TimeSpan?)this.GetValue(InvalidateDrawableAfterProperty);
            set => this.SetValue(InvalidateDrawableAfterProperty, value);
        }

        private static void UpdateDrawable(BarcodeResultOverlay barcodeResultOverlay,
            BarcodeResult[] barcodeResults, float strokeSize, Color strokeColor, Color textColor, TimeSpan? invalidateDrawableAfter)
        {
            barcodeResultOverlay.BarcodeDrawable.Update(barcodeResults, strokeSize, strokeColor, textColor);
            barcodeResultOverlay.Invalidate();

            if (invalidateDrawableAfter is TimeSpan delay)
            {
                barcodeResultOverlay.taskDelayer.RunWithDelay(delay, () =>
                {
                    barcodeResultOverlay.BarcodeDrawable.Reset();
                    MainThread.BeginInvokeOnMainThread(barcodeResultOverlay.Invalidate);
                });
            }
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
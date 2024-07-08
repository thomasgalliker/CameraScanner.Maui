namespace CameraScanner.Maui.Controls
{
    public partial class QRCodeOverlay : Grid
    {
        private const double BorderSizeRatio = 0.1d;

        private double width;
        private double height;

        public QRCodeOverlay()
        {
            this.InitializeComponent();
        }

        public static readonly BindableProperty ShadeColorProperty =
            BindableProperty.Create(
                nameof(ShadeColor),
                typeof(Color),
                typeof(QRCodeOverlay),
                defaultValue: Colors.Black);

        public Color ShadeColor
        {
            get => (Color)this.GetValue(ShadeColorProperty);
            set => this.SetValue(ShadeColorProperty, value);
        }

        public static readonly BindableProperty ShadeOpacityProperty =
            BindableProperty.Create(
                nameof(ShadeOpacity),
                typeof(double),
                typeof(QRCodeOverlay),
                defaultValue: 0.5d);

        public double ShadeOpacity
        {
            get => (double)this.GetValue(ShadeOpacityProperty);
            set => this.SetValue(ShadeOpacityProperty, value);
        }

        public static readonly BindableProperty BracketColorProperty =
            BindableProperty.Create(
                nameof(BracketColor),
                typeof(Color),
                typeof(QRCodeOverlay),
                Colors.White);

        public Color BracketColor
        {
            get => (Color)this.GetValue(BracketColorProperty);
            set => this.SetValue(BracketColorProperty, value);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if ((this.width != width && width > 0d) || (this.height != height && height > 0d))
            {
                this.width = width;
                this.height = height;

                // Dynamically calculate the grid size if the size of the parent grid has changed.

                if (width < height)
                {
                    // Uniform borders on left/right side
                    var (borderWidth, cutoutWindowSize) = CalculateGridSize(width);
                    this.Column0.Width = borderWidth;
                    this.Column1.Width = cutoutWindowSize;
                    this.Column2.Width = borderWidth;

                    this.Row0.Height = new GridLength(1d, GridUnitType.Star);
                    this.Row1.Height = cutoutWindowSize;
                    this.Row2.Height = new GridLength(1d, GridUnitType.Star);
                }
                else
                {
                    // Uniform borders on top/bottom side
                    var (borderHeight, cutoutWindowSize) = CalculateGridSize(height);
                    this.Row0.Height = borderHeight;
                    this.Row1.Height = cutoutWindowSize;
                    this.Row2.Height = borderHeight;

                    this.Column0.Width = new GridLength(1d, GridUnitType.Star);
                    this.Column1.Width = cutoutWindowSize;
                    this.Column2.Width = new GridLength(1d, GridUnitType.Star);
                }
            }
        }

        private static (double, double) CalculateGridSize(double totalSize)
        {
            var borderSize = (int)(totalSize * BorderSizeRatio);
            var cutoutWindowSize = totalSize - 2 * borderSize;
            return (borderSize, cutoutWindowSize);
        }
    }
}
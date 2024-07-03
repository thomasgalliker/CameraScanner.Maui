using System.Windows.Input;
using Microsoft.Maui.Graphics.Platform;
using Timer = System.Timers.Timer;

namespace CameraScanner.Maui
{
    public partial class CameraView : View
    {
        private readonly HashSet<BarcodeResult> pooledResults;
        private readonly Timer poolingTimer;

        public CameraView()
        {
            this.pooledResults = [];
            this.poolingTimer = new Timer { AutoReset = false };
            this.poolingTimer.Elapsed += this.PoolingTimer_Elapsed; // TODO: Memory leak!
        }

        public static readonly BindableProperty OnDetectionFinishedCommandProperty = BindableProperty.Create(
            nameof(OnDetectionFinishedCommand),
            typeof(ICommand),
            typeof(CameraView));

        public ICommand OnDetectionFinishedCommand
        {
            get => (ICommand)this.GetValue(OnDetectionFinishedCommandProperty);
            set => this.SetValue(OnDetectionFinishedCommandProperty, value);
        }

        public static readonly BindableProperty OnImageCapturedCommandProperty = BindableProperty.Create(
            nameof(OnImageCapturedCommand),
            typeof(ICommand),
            typeof(CameraView));

        public ICommand OnImageCapturedCommand
        {
            get => (ICommand)this.GetValue(OnImageCapturedCommandProperty);
            set => this.SetValue(OnImageCapturedCommandProperty, value);
        }

        public static readonly BindableProperty VibrationOnDetectedProperty = BindableProperty.Create(
            nameof(VibrationOnDetected),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Enables or disables vibration on successful barcode detection.
        /// Default: <c>false</c>
        /// </summary>
        public bool VibrationOnDetected
        {
            get => (bool)this.GetValue(VibrationOnDetectedProperty);
            set => this.SetValue(VibrationOnDetectedProperty, value);
        }

        public static readonly BindableProperty CameraEnabledProperty = BindableProperty.Create(
            nameof(CameraEnabled),
            typeof(bool),
            typeof(CameraView),
            true);

        /// <summary>
        /// Enables or disables the camera preview.
        /// Default: <c>true</c>
        /// </summary>
        public bool CameraEnabled
        {
            get => (bool)this.GetValue(CameraEnabledProperty);
            set => this.SetValue(CameraEnabledProperty, value);
        }

        public static readonly BindableProperty PauseScanningProperty = BindableProperty.Create(
            nameof(PauseScanning),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Pauses barcode scanning.
        /// </summary>
        public bool PauseScanning
        {
            get => (bool)this.GetValue(PauseScanningProperty);
            set => this.SetValue(PauseScanningProperty, value);
        }

        public static readonly BindableProperty ForceInvertedProperty = BindableProperty.Create(
            nameof(ForceInverted),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Forces scanning of inverted barcodes. Reduces performance significantly. Android only.
        /// </summary>
        public bool ForceInverted
        {
            get => (bool)this.GetValue(ForceInvertedProperty);
            set => this.SetValue(ForceInvertedProperty, value);
        }

        public static readonly BindableProperty PoolingIntervalProperty = BindableProperty.Create(
            nameof(PoolingInterval),
            typeof(int),
            typeof(CameraView),
            0);

        /// <summary>
        /// Enables pooling of detections for better detection of multiple barcodes at once.
        /// Value in milliseconds. Default: 0 (disabled).
        /// </summary>
        public int PoolingInterval
        {
            get => (int)this.GetValue(PoolingIntervalProperty);
            set => this.SetValue(PoolingIntervalProperty, value);
        }

        public static readonly BindableProperty TorchOnProperty = BindableProperty.Create(
            nameof(TorchOn),
            typeof(bool),
            typeof(CameraView),
            false,
            BindingMode.TwoWay);

        /// <summary>
        /// Enables or disables the torch.
        /// </summary>
        public bool TorchOn
        {
            get => (bool)this.GetValue(TorchOnProperty);
            set => this.SetValue(TorchOnProperty, value);
        }

        public static readonly BindableProperty TapToFocusEnabledProperty = BindableProperty.Create(
            nameof(TapToFocusEnabled),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Disables or enables tap-to-focus.
        /// </summary>
        public bool TapToFocusEnabled
        {
            get => (bool)this.GetValue(TapToFocusEnabledProperty);
            set => this.SetValue(TapToFocusEnabledProperty, value);
        }

        public static readonly BindableProperty CameraFacingProperty = BindableProperty.Create(
            nameof(CameraFacing),
            typeof(CameraFacing),
            typeof(CameraView),
            CameraFacing.Back);

        /// <summary>
        /// Select front or back facing camera.
        /// Default: <see cref="CameraFacing.Back"/>
        /// </summary>
        public CameraFacing CameraFacing
        {
            get => (CameraFacing)this.GetValue(CameraFacingProperty);
            set => this.SetValue(CameraFacingProperty, value);
        }

        public static readonly BindableProperty CaptureQualityProperty = BindableProperty.Create(
            nameof(CaptureQuality),
            typeof(CaptureQuality),
            typeof(CameraView),
            CaptureQuality.Medium);

        /// <summary>
        /// Set the capture quality for the image analysis.
        /// Recommended and default value is Medium.
        /// Use the highest values for more precision or lower for fast scanning.
        /// </summary>
        public CaptureQuality CaptureQuality
        {
            get => (CaptureQuality)this.GetValue(CaptureQualityProperty);
            set => this.SetValue(CaptureQualityProperty, value);
        }

        public static readonly BindableProperty BarcodeFormatsProperty = BindableProperty.Create(
            nameof(BarcodeFormats),
            typeof(BarcodeFormats),
            typeof(CameraView),
            BarcodeFormats.All);

        /// <summary>
        /// Set the enabled barcode formats.
        /// Default: <see cref="BarcodeFormats.All"/>.
        /// </summary>
        public BarcodeFormats BarcodeFormats
        {
            get => (BarcodeFormats)this.GetValue(BarcodeFormatsProperty);
            set => this.SetValue(BarcodeFormatsProperty, value);
        }

        public static readonly BindableProperty AimModeProperty = BindableProperty.Create(
            nameof(AimMode),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Disables or enables aim mode. When enabled only barcode that is in the center of the preview will be detected.
        /// </summary>
        public bool AimMode
        {
            get => (bool)this.GetValue(AimModeProperty);
            set => this.SetValue(AimModeProperty, value);
        }

        public static readonly BindableProperty ViewfinderModeProperty = BindableProperty.Create(
            nameof(ViewfinderMode),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Disables or enables viewfinder mode. When enabled only barcode that is visible in the preview will be detected.
        /// </summary>
        public bool ViewfinderMode
        {
            get => (bool)this.GetValue(ViewfinderModeProperty);
            set => this.SetValue(ViewfinderModeProperty, value);
        }

        public static readonly BindableProperty CaptureNextFrameProperty = BindableProperty.Create(
            nameof(CaptureNextFrame),
            typeof(bool),
            typeof(CameraView),
            false);

        /// <summary>
        /// Captures the next frame from the camera feed.
        /// </summary>
        public bool CaptureNextFrame
        {
            get => (bool)this.GetValue(CaptureNextFrameProperty);
            set => this.SetValue(CaptureNextFrameProperty, value);
        }

        public static readonly BindableProperty BarcodeDetectionFrameRateProperty =
            BindableProperty.Create(
                nameof(BarcodeDetectionFrameRate),
                typeof(uint?),
                typeof(CameraView));

        /// <summary>
        /// Specifies the frequency at which frames are processed for barcode detection.
        /// Default: null (no frame skipping)
        /// </summary>
        /// <example>
        /// If the value is null, 0 or 1, every frame from the camera preview is analyzed.
        /// If the value is 2, every 2nd frame from the camera preview is analyzed.
        /// If the value is 3, every 3rd frame from the camera preview is analyzed.
        /// </example>
        public uint? BarcodeDetectionFrameRate
        {
            get => (uint?)this.GetValue(BarcodeDetectionFrameRateProperty);
            set => this.SetValue(BarcodeDetectionFrameRateProperty, value);
        }

        public static readonly BindableProperty RequestZoomFactorProperty = BindableProperty.Create(
            nameof(RequestZoomFactor),
            typeof(float),
            typeof(CameraView),
            -1f);

        /// <summary>
        /// Setting this value changes the zoom factor of the camera. Value has to be between MinZoomFactor and MaxZoomFactor.
        /// More info:
        /// iOS/macOS - https://developer.apple.com/documentation/avfoundation/avcapturedevice/zoom
        /// Android - https://developer.android.com/reference/kotlin/androidx/camera/view/CameraController#setZoomRatio(float)
        /// Only logical multi-camera is supported - https://developer.android.com/media/camera/camera2/multi-camera
        /// </summary>
        public float RequestZoomFactor
        {
            get => (float)this.GetValue(RequestZoomFactorProperty);
            set => this.SetValue(RequestZoomFactorProperty, value);
        }

        public static readonly BindableProperty CurrentZoomFactorProperty = BindableProperty.Create(
            nameof(CurrentZoomFactor),
            typeof(float),
            typeof(CameraView),
            -1f,
            BindingMode.OneWayToSource);

        /// <summary>
        /// Returns current zoom factor value.
        /// </summary>
        public float CurrentZoomFactor
        {
            get => (float)this.GetValue(CurrentZoomFactorProperty);
            set => this.SetValue(CurrentZoomFactorProperty, value);
        }

        public static readonly BindableProperty MinZoomFactorProperty = BindableProperty.Create(
            nameof(MinZoomFactor),
            typeof(float),
            typeof(CameraView),
            -1f,
            BindingMode.OneWayToSource);

        /// <summary>
        /// Returns minimum zoom factor for current camera.
        /// </summary>
        public float MinZoomFactor
        {
            get => (float)this.GetValue(MinZoomFactorProperty);
            set => this.SetValue(MinZoomFactorProperty, value);
        }

        public static readonly BindableProperty MaxZoomFactorProperty = BindableProperty.Create(
            nameof(MaxZoomFactor),
            typeof(float),
            typeof(CameraView),
            -1f,
            BindingMode.OneWayToSource);

        /// <summary>
        /// Returns maximum zoom factor for current camera.
        /// </summary>
        public float MaxZoomFactor
        {
            get => (float)this.GetValue(MaxZoomFactorProperty);
            set => this.SetValue(MaxZoomFactorProperty, value);
        }

        public static readonly BindableProperty DeviceSwitchZoomFactorProperty = BindableProperty.Create(
            nameof(DeviceSwitchZoomFactor),
            typeof(float[]),
            typeof(CameraView),
            Array.Empty<float>(),
            BindingMode.OneWayToSource);

        /// <summary>
        /// Returns zoom factors that switch between camera lenses (iOS only).
        /// </summary>
        public float[] DeviceSwitchZoomFactor
        {
            get => (float[])this.GetValue(DeviceSwitchZoomFactorProperty);
            set => this.SetValue(DeviceSwitchZoomFactorProperty, value);
        }

        public event EventHandler<OnDetectionFinishedEventArg> OnDetectionFinished;
        public event EventHandler<OnImageCapturedEventArg> OnImageCaptured;

        internal void DetectionFinished(HashSet<BarcodeResult> barCodeResults)
        {
            if (barCodeResults is null)
            {
                return;
            }

            if (this.PoolingInterval > 0)
            {
                if (!this.poolingTimer.Enabled)
                {
                    this.poolingTimer.Interval = this.PoolingInterval;
                    this.poolingTimer.Start();
                }

                foreach (var result in barCodeResults)
                {
                    if (!this.pooledResults.Add(result))
                    {
                        if (this.pooledResults.TryGetValue(result, out var currentResult))
                        {
                            currentResult.PreviewBoundingBox = result.PreviewBoundingBox;
                            currentResult.ImageBoundingBox = result.ImageBoundingBox;
                        }
                    }
                }
            }
            else
            {
                this.TriggerOnDetectionFinished(barCodeResults.ToArray());
            }
        }

        private void PoolingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.TriggerOnDetectionFinished(this.pooledResults.ToArray());
            this.pooledResults.Clear();
        }

        private void TriggerOnDetectionFinished(BarcodeResult[] barcodeResults)
        {
            if (!barcodeResults.Any())
            {
                return;
            }

            if (this.PauseScanning)
            {
                return;
            }

            try
            {
                if (this.VibrationOnDetected)
                {
                    Vibration.Vibrate();
                }
            }
            catch
            {
                // Ignore exceptions
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.OnDetectionFinished?.Invoke(this, new OnDetectionFinishedEventArg { BarcodeResults = barcodeResults });

                if (this.OnDetectionFinishedCommand is ICommand c && c.CanExecute(barcodeResults))
                {
                    c.Execute(barcodeResults);
                }
            });
        }

        internal void TriggerOnImageCaptured(PlatformImage image)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.OnImageCaptured?.Invoke(this, new OnImageCapturedEventArg { Image = image });
                if (this.OnImageCapturedCommand?.CanExecute(image) ?? false)
                {
                    this.OnImageCapturedCommand?.Execute(image);
                }
            });
        }
    }
}
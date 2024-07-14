using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class UniversalScannerViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IPopupService popupService;

        private IAsyncRelayCommand<BarcodeResult[]> onDetectionFinishedCommand;
        private BarcodeFormats barcodeFormats;
        private CaptureQuality captureQuality;
        private bool isScannerPause;
        private bool isScannerEnabled;
        private IRelayCommand startStopCameraCommand;
        private IRelayCommand toggleCameraPauseCommand;
        private IAsyncRelayCommand configureCommand;
        private IRelayCommand<BarcodeResult> barcodeResultTappedCommand;
        private bool torchOn;
        private IRelayCommand toggleTorchCommand;
        private uint? barcodeDetectionFrameRate;

        public UniversalScannerViewModel(
            ILogger<UniversalScannerViewModel> logger,
            IDialogService dialogService,
            IPopupService popupService)
        {
            this.logger = logger;
            this.dialogService = dialogService;
            this.popupService = popupService;

            this.BarcodeFormats = BarcodeFormats.All;
            this.CaptureQuality = CaptureQuality.Medium;
            this.IsScannerEnabled = true;
        }

        public bool IsScannerEnabled
        {
            get => this.isScannerEnabled;
            private set
            {
                if (this.SetProperty(ref this.isScannerEnabled, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public bool IsScannerPause
        {
            get => this.isScannerPause;
            private set
            {
                if (this.SetProperty(ref this.isScannerPause, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public BarcodeFormats BarcodeFormats
        {
            get => this.barcodeFormats;
            private set
            {
                if (this.SetProperty(ref this.barcodeFormats, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public CaptureQuality CaptureQuality
        {
            get => this.captureQuality;
            private set
            {
                if (this.SetProperty(ref this.captureQuality, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IAsyncRelayCommand<BarcodeResult[]> OnDetectionFinishedCommand
        {
            get => this.onDetectionFinishedCommand ??= new AsyncRelayCommand<BarcodeResult[]>(this.HandleScanResultAsync);
        }

        private async Task HandleScanResultAsync(BarcodeResult[] barcodeResults)
        {
            try
            {
                if (barcodeResults.FirstOrDefault() is BarcodeResult barcodeResult)
                {
                    // this.IsScannerPause = true;
                    //
                    // var stop = await this.dialogService.DisplayAlertAsync(
                    //     "Barcode found",
                    //     $"BarcodeType=\"{barcodeResult.BarcodeType}\"{Environment.NewLine}" +
                    //     $"BarcodeFormat=\"{barcodeResult.BarcodeFormat}\"{Environment.NewLine}" +
                    //     $"DisplayValue=\"{barcodeResult.DisplayValue}\"",
                    //     "Stop", "Continue");
                    //
                    // if (stop)
                    // {
                    //     this.StopCamera();
                    // }
                    // else
                    // {
                    //     this.IsScannerPause = false;
                    // }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "HandleScanResultAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Scan result handling failed with exception: {ex.Message}", "OK");
            }
        }

        public IRelayCommand StartStopCameraCommand
        {
            get => this.startStopCameraCommand ??= new RelayCommand(this.StartStopCamera);
        }

        private void StartStopCamera()
        {
            if (this.IsScannerEnabled)
            {
                this.IsScannerEnabled = false;
            }
            else
            {
                this.IsScannerEnabled = true;
                this.IsScannerPause = false;
            }
        }

        public IRelayCommand ToggleCameraPauseCommand
        {
            get => this.toggleCameraPauseCommand ??= new RelayCommand(this.ToggleCameraPause);
        }

        private void ToggleCameraPause()
        {
            this.IsScannerPause = !this.IsScannerPause;
        }

        public bool TorchOn
        {
            get => this.torchOn;
            set
            {
                if (this.SetProperty(ref this.torchOn, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IRelayCommand ToggleTorchCommand
        {
            get => this.toggleTorchCommand ??= new RelayCommand(this.ToggleTorch);
        }

        private void ToggleTorch()
        {
            this.TorchOn = !this.TorchOn;
        }

        public uint? BarcodeDetectionFrameRate
        {
            get => this.barcodeDetectionFrameRate;
            private set
            {
                if (this.SetProperty(ref this.barcodeDetectionFrameRate, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IRelayCommand<BarcodeResult> BarcodeResultTappedCommand
        {
            get => this.barcodeResultTappedCommand ??= new RelayCommand<BarcodeResult>(this.OnBarcodeResultTapped);
        }

        private async void OnBarcodeResultTapped(BarcodeResult barcodeResult)
        {
            await this.dialogService.DisplayAlertAsync(
                "OnBarcodeResultTapped",
                $"BarcodeType=\"{barcodeResult.BarcodeType}\"{Environment.NewLine}" +
                $"BarcodeFormat=\"{barcodeResult.BarcodeFormat}\"{Environment.NewLine}" +
                $"DisplayValue=\"{barcodeResult.DisplayValue}\"",
                "OK");
        }

        public string DebugInfo
        {
            get
            {
                return
                    $"IsScannerEnabled: {this.IsScannerEnabled}{Environment.NewLine}" +
                    $"IsScannerPause: {this.IsScannerPause}{Environment.NewLine}" +
                    $"TorchOn: {this.TorchOn}{Environment.NewLine}" +
                    $"CaptureQuality: {this.CaptureQuality}{Environment.NewLine}" +
                    $"BarcodeFormats: {this.BarcodeFormats}{Environment.NewLine}" +
                    $"BarcodeDetectionFrameRate: {this.BarcodeDetectionFrameRate?.ToString() ?? "null"}";
            }
        }

        public IAsyncRelayCommand ConfigureCommand
        {
            get => this.configureCommand ??= new AsyncRelayCommand(this.ConfigureAsync);
        }

        private async Task ConfigureAsync()
        {
            var navigationParameter = new ScannerConfigViewModel.NavigationParameter
            {
                BarcodeFormat = this.BarcodeFormats,
                CaptureQuality = this.CaptureQuality,
                BarcodeDetectionFrameRate = this.BarcodeDetectionFrameRate,
            };

            var result = await this.popupService.ShowPopupAsync<ScannerConfigViewModel>(onPresenting: vm =>
                vm.Initialize(navigationParameter));
            if (result is ScannerConfigViewModel.PopupResult popupResult)
            {
                this.BarcodeFormats = popupResult.BarcodeFormats;
                this.CaptureQuality = popupResult.CaptureQuality;
                this.BarcodeDetectionFrameRate = popupResult.BarcodeDetectionFrameRate;
            }
        }
    }
}
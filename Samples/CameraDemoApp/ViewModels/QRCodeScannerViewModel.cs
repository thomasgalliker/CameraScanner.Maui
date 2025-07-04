using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class QRCodeScannerViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IBarcodeParser barcodeParser;

        private IAsyncRelayCommand<BarcodeResult[]> onDetectionFinishedCommand;
        private bool isScannerPause;
        private bool isScannerEnabled;
        private IRelayCommand startCameraCommand;
        private IRelayCommand stopCameraCommand;
        private IRelayCommand toggleTorchCommand;
        private bool torchOn;

        public QRCodeScannerViewModel(
            ILogger<QRCodeScannerViewModel> logger,
            IDialogService dialogService,
            IBarcodeParser barcodeParser)
        {
            this.logger = logger;
            this.dialogService = dialogService;
            this.barcodeParser = barcodeParser;

            this.IsScannerEnabled = true;

            // _ = Enumerable.Range(1, count: 3).Select(async i =>
            // {
            //     await Task.Delay(i * 1000);
            //     this.IsScannerEnabled = true;
            //     this.IsScannerEnabled = false;
            //     this.IsScannerEnabled = true;
            // }).ToArray();
        }

        public bool IsScannerEnabled
        {
            get => this.isScannerEnabled;
            private set => this.SetProperty(ref this.isScannerEnabled, value);
        }

        public bool IsScannerPause
        {
            get => this.isScannerPause;
            private set => this.SetProperty(ref this.isScannerPause, value);
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
                    this.IsScannerPause = true;

                    var parsedResult = this.barcodeParser.Parse(barcodeResult.RawValue);

                    var stop = await this.dialogService.DisplayAlertAsync(
                        "Barcode found",
                        $"BarcodeType=\"{barcodeResult.BarcodeType}\"{Environment.NewLine}" +
                        $"BarcodeFormat=\"{barcodeResult.BarcodeFormat}\"{Environment.NewLine}" +
                        $"ParsedResult=\"{parsedResult.GetType().Name}\"{Environment.NewLine}" +
                        $"DisplayValue=\"{barcodeResult.DisplayValue}\"",
                        "Stop", "Continue");

                    if (stop)
                    {
                        this.StopCamera();
                    }
                    else
                    {
                        this.IsScannerPause = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "HandleScanResultAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Scan result handling failed with exception: {ex.Message}", "OK");
            }
        }

        public IRelayCommand StartCameraCommand
        {
            get => this.startCameraCommand ??= new RelayCommand(this.StartCamera);
        }

        private void StartCamera()
        {
            this.IsScannerEnabled = true;
            this.IsScannerPause = false;
        }

        public IRelayCommand StopCameraCommand
        {
            get => this.stopCameraCommand ??= new RelayCommand(this.StopCamera);
        }

        private void StopCamera()
        {
            this.IsScannerEnabled = false;
        }

        public bool TorchOn
        {
            get => this.torchOn;
            set => this.SetProperty(ref this.torchOn, value);
        }

        public IRelayCommand ToggleTorchCommand
        {
            get => this.toggleTorchCommand ??= new RelayCommand(this.ToggleTorch);
        }

        private void ToggleTorch()
        {
            this.TorchOn = !this.TorchOn;
        }
    }
}

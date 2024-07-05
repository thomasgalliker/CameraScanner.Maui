using CameraDemoApp.Services;
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
        private IAsyncRelayCommand<BarcodeResult[]> onDetectionFinishedCommand;
        private bool isScannerPause;
        private bool isScannerEnabled;
        private IRelayCommand startCameraCommand;
        private IRelayCommand stopCameraCommand;

        public QRCodeScannerViewModel(
            ILogger<QRCodeScannerViewModel> logger,
            IDialogService dialogService)
        {
            this.logger = logger;
            this.dialogService = dialogService;

            this.IsScannerEnabled = true;
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

                    var stop = await this.dialogService.DisplayAlertAsync(
                        "Barcode found",
                        barcodeResult.DisplayValue,
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
    }
}

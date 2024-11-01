using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class BarcodeResultItemViewModel
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;

        private IAsyncRelayCommand barcodeResultTappedCommand;

        public BarcodeResultItemViewModel(
            ILogger<BarcodeResultItemViewModel> logger,
            INavigationService navigationService,
            BarcodeResult barcodeResult)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.BarcodeResult = barcodeResult;
        }

        public BarcodeResult BarcodeResult { get; }

        public IAsyncRelayCommand BarcodeResultTappedCommand
        {
            get => this.barcodeResultTappedCommand ??= new AsyncRelayCommand(this.OnBarcodeResultTapped);
        }

        private async Task OnBarcodeResultTapped()
        {
            try
            {
                await this.navigationService.PushAsync("BarcodeResultDetailPage", this.BarcodeResult);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OnBarcodeResultTapped failed with exception");
            }
        }
    }
}
using System.Windows.Input;
using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using UIKit;

namespace CameraDemoApp.ViewModels
{
    public class BarcodeResultDetailViewModel : ObservableObject, INavigatedTo<BarcodeResult>
    {
        private readonly ILogger logger;
        private readonly IClipboard clipboard;

        private BarcodeResult barcodeResult;
        private IAsyncRelayCommand copyDisplayValueCommand;

        public BarcodeResultDetailViewModel(
            ILogger<BarcodeResultDetailViewModel> logger,
            IClipboard clipboard)
        {
            this.logger = logger;
            this.clipboard = clipboard;
        }

        public Task NavigatedToAsync(BarcodeResult barcodeResult)
        {
            this.BarcodeResult = barcodeResult;
            return Task.CompletedTask;
        }

        public BarcodeResult BarcodeResult
        {
            get => this.barcodeResult;
            private set => this.SetProperty(ref this.barcodeResult, value);
        }

        public IAsyncRelayCommand CopyDisplayValueCommand
        {
            get => this.copyDisplayValueCommand ??= new AsyncRelayCommand(this.CopyDisplayValueAsync);
        }

        private async Task CopyDisplayValueAsync()
        {
            if (this.BarcodeResult?.DisplayValue is string displayValue)
            {
                await this.clipboard.SetTextAsync(displayValue);
                _ = Toast.Make("Copied!").Show();
            }
        }
    }
}
using CameraScanner.Maui;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CameraDemoApp.ViewModels
{
    public class ScannerConfigViewModel : ObservableObject
    {
        private IAsyncRelayCommand<Popup> cancelCommand;
        private IAsyncRelayCommand<Popup> confirmCommand;
        private BarcodeFormats barcodeFormat;
        private BarcodeFormats[] barcodeFormats;

        public ScannerConfigViewModel()
        {
        }

        public void Initialize(NavigationParameter navigationParameter)
        {
            this.BarcodeFormats = Enum.GetValues(typeof(BarcodeFormats))
                .Cast<BarcodeFormats>()
                .ToArray();

            this.BarcodeFormat = navigationParameter.BarcodeFormats;
        }

        public BarcodeFormats[] BarcodeFormats
        {
            get => this.barcodeFormats;
            private set => this.SetProperty(ref this.barcodeFormats, value);
        }

        public BarcodeFormats BarcodeFormat
        {
            get => this.barcodeFormat;
            set => this.SetProperty(ref this.barcodeFormat, value);
        }

        public IAsyncRelayCommand<Popup> CancelCommand
        {
            get => this.cancelCommand ??= new AsyncRelayCommand<Popup>(this.CancelAsync);
        }

        private async Task CancelAsync(Popup popup)
        {
            await popup.CloseAsync();
        }

        public IAsyncRelayCommand<Popup> ConfirmCommand
        {
            get => this.confirmCommand ??= new AsyncRelayCommand<Popup>(this.ConfirmAsync);
        }

        private async Task ConfirmAsync(Popup popup)
        {
            var popupResult = new PopupResult()
            {
                BarcodeFormats = new[] { this.BarcodeFormat }
            };

            await popup.CloseAsync(popupResult);
        }


        public class PopupResult
        {
            public BarcodeFormats[] BarcodeFormats { get; set; }
        }

        public class NavigationParameter
        {
            public BarcodeFormats BarcodeFormats { get; set; }
        }
    }
}

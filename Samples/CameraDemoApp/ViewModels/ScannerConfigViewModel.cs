using CameraScanner.Maui;
using CameraScanner.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CameraDemoApp.ViewModels
{
    public class ScannerConfigViewModel : ObservableObject
    {
        private IAsyncRelayCommand<Popup> cancelCommand;
        private IAsyncRelayCommand<Popup> confirmCommand;
        private BarcodeFormatViewModel[] barcodeFormats;
        private CaptureQualityViewModel[] captureQualities;
        private string barcodeDetectionFrameRate;

        public ScannerConfigViewModel()
        {
        }

        public void Initialize(NavigationParameter navigationParameter)
        {
            var selectedBarcodeFormats = navigationParameter.BarcodeFormat.ToArray();

            this.BarcodeFormats = Enum.GetValues(typeof(BarcodeFormats))
                .Cast<BarcodeFormats>()
                .Where(b => b != CameraScanner.Maui.BarcodeFormats.None && b != CameraScanner.Maui.BarcodeFormats.All)
                .Select(b => new BarcodeFormatViewModel(b, isSelected: selectedBarcodeFormats.Contains(b)))
                .ToArray();

            this.CaptureQualities =  Enum.GetValues(typeof(CaptureQuality))
                .Cast<CaptureQuality>()
                .Select(q => new CaptureQualityViewModel(q, isSelected: navigationParameter.CaptureQuality == q))
                .ToArray();

            this.BarcodeDetectionFrameRate = navigationParameter.BarcodeDetectionFrameRate?.ToString();
        }

        public BarcodeFormatViewModel[] BarcodeFormats
        {
            get => this.barcodeFormats;
            private set => this.SetProperty(ref this.barcodeFormats, value);
        }

        public CaptureQualityViewModel[] CaptureQualities
        {
            get => this.captureQualities;
            private set => this.SetProperty(ref this.captureQualities, value);
        }

        public string BarcodeDetectionFrameRate
        {
            get => this.barcodeDetectionFrameRate;
            set => this.SetProperty(ref this.barcodeDetectionFrameRate, value);
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
            var popupResult = new PopupResult
            {
                BarcodeFormats = this.BarcodeFormats
                    .Where(b => b.IsSelected)
                    .Select(b => b.Value)
                    .ToEnum(),

                CaptureQuality = this.CaptureQualities.SingleOrDefault(q => q.IsSelected)?.Value ?? CaptureQuality.Medium
            };

            if (uint.TryParse(this.BarcodeDetectionFrameRate, out var frameRate))
            {
                popupResult.BarcodeDetectionFrameRate = frameRate;
            }

            await popup.CloseAsync(popupResult);
        }

        public class PopupResult
        {
            public BarcodeFormats BarcodeFormats { get; set; }

            public CaptureQuality CaptureQuality { get; set; }

            public uint? BarcodeDetectionFrameRate { get; set; }
        }

        public class NavigationParameter
        {
            public BarcodeFormats BarcodeFormat { get; set; }

            public CaptureQuality CaptureQuality { get; set; }

            public uint? BarcodeDetectionFrameRate { get; set; }
        }
    }
}

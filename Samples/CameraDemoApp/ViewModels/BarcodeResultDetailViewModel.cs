using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class BarcodeResultDetailViewModel : ObservableObject, INavigatedTo<BarcodeResult>
    {
        private readonly IClipboard clipboard;
        private readonly DumpOptions dumpOptions;
        private readonly ILogger logger;

        private BarcodeResult barcodeResult;
        private string barcodeResultDump;
        private IAsyncRelayCommand copyDisplayValueCommand;

        public BarcodeResultDetailViewModel(
            ILogger<BarcodeResultDetailViewModel> logger,
            IClipboard clipboard)
        {
            this.logger = logger;
            this.clipboard = clipboard;

            this.dumpOptions = new DumpOptions { DumpStyle = DumpStyle.Console };
            this.dumpOptions.CustomInstanceFormatters.AddFormatter<byte[]>(b => Convert.ToHexString(b));
            this.dumpOptions.CustomInstanceFormatters.AddFormatter<RectF>(rectF => $"{rectF.Width} x {rectF.Height}");
            this.dumpOptions.CustomInstanceFormatters.AddFormatter<Point[]>(points =>
                $"[{string.Join(", ", points.Select(x => $"{{{x.X}, {x.Y}}}"))}]");
        }

        public BarcodeResult BarcodeResult
        {
            get => this.barcodeResult;
            private set => this.SetProperty(ref this.barcodeResult, value);
        }

        public string BarcodeResultDump
        {
            get => this.barcodeResultDump;
            private set => this.SetProperty(ref this.barcodeResultDump, value);
        }

        public IAsyncRelayCommand CopyDisplayValueCommand =>
            this.copyDisplayValueCommand ??= new AsyncRelayCommand(this.CopyDisplayValueAsync);

        public Task NavigatedToAsync(BarcodeResult barcodeResult)
        {
            this.BarcodeResult = barcodeResult;
            this.BarcodeResultDump = ObjectDumper.Dump(barcodeResult, this.dumpOptions);
            return Task.CompletedTask;
        }

        private async Task CopyDisplayValueAsync()
        {
            if (this.BarcodeResultDump is string barcodeResult)
            {
                await this.clipboard.SetTextAsync(barcodeResult);
                _ = Toast.Make("Copied!").Show();
            }
        }
    }
}
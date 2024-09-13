using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class FilePickerViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IMediaPicker mediaPicker;
        private readonly IBarcodeScanner barcodeScanner;

        private ImageSource image;
        private IAsyncRelayCommand pickPhotoCommand;
        private BarcodeResult[] barcodeResults;

        public FilePickerViewModel(
            ILogger<FilePickerViewModel> logger,
            IMediaPicker mediaPicker,
            IBarcodeScanner barcodeScanner)
        {
            this.logger = logger;
            this.mediaPicker = mediaPicker;
            this.barcodeScanner = barcodeScanner;
        }

        public ImageSource Image
        {
            get => this.image;
            private set => this.SetProperty(ref this.image, value);
        }

        public BarcodeResult[] BarcodeResults
        {
            get => this.barcodeResults;
            private set
            {
                if (this.SetProperty(ref this.barcodeResults, value))
                {
                    this.OnPropertyChanged(nameof(this.BarcodeResultsCountText));
                }
            }
        }

        public string BarcodeResultsCountText
        {
            get
            {
                if (this.BarcodeResults is not BarcodeResult[] barcodeResults)
                {
                    return null;
                }

                return string.Format("Number of barcodes found: {0}", barcodeResults.Length);
            }
        }

        public IAsyncRelayCommand PickPhotoCommand
        {
            get => this.pickPhotoCommand ??= new AsyncRelayCommand(this.PickPhotoAsync);
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var options = new MediaPickerOptions { Title = "Test picker" };
                var fileResult = await this.mediaPicker.PickPhotoAsync(options);
                if (fileResult != null)
                {
                    var barcodeResults = await this.barcodeScanner.ScanFromImageAsync(fileResult);
                    this.Image = ImageSource.FromFile(fileResult.FullPath);
                    this.BarcodeResults = barcodeResults.ToArray();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PickPhotoAsync failed with exception");
                this.BarcodeResults = [];
                this.Image = null;
            }
        }
    }
}
